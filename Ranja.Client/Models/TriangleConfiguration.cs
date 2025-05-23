namespace Ranja.Client.Models;

public enum LabelPosition
{
    VerticalAbove,  // vert+
    VerticalBelow,  // vert-
    HorizontalRight, // horz+
    HorizontalLeft   // horz-
}

public record struct Vector2D(double X, double Y)
{
    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2D operator *(Vector2D v, double s) => new(v.X * s, v.Y * s);
    public static Vector2D operator *(double s, Vector2D v) => new(v.X * s, v.Y * s);
    
    public double Magnitude => Math.Sqrt(X * X + Y * Y);
    public Vector2D Normalized => Magnitude == 0 ? new(0, 0) : this * (1.0 / Magnitude);
    public double Dot(Vector2D other) => X * other.X + Y * other.Y;
    public double Cross(Vector2D other) => X * other.Y - Y * other.X; // Z component of 3D cross product
}

public record struct LogicalCoordinates(double MinX, double MaxX, double MinY, double MaxY);
public record struct SvgViewBox(double Width, double Height);

public record struct LineSegment(Vector2D P1, Vector2D P2);

public record PointDefinition(Vector2D LogicalCoords, LabelPosition LabelPosition, string LabelOffset);

public record LineDefinition(string Point1, string Point2);

public record AngleDefinition(
    string Id,
    string P1,
    string Vertex, 
    string P2,
    string DisplayValue,
    double Radius,
    double TextOffsetScale = 1.5,
    string Color = "#7c3aed",
    string FontWeight = "normal"
);

public static class GeometryUtils
{
    public static Vector2D TransformPoint(Vector2D logicalPoint, LogicalCoordinates logicalBox, SvgViewBox svgBox)
    {
        var logicalWidth = logicalBox.MaxX - logicalBox.MinX;
        var logicalHeight = logicalBox.MaxY - logicalBox.MinY;
        
        if (logicalWidth == 0 || logicalHeight == 0)
            return new Vector2D(0, 0);
            
        var svgX = svgBox.Width * (logicalPoint.X - logicalBox.MinX) / logicalWidth;
        // SVG Y-coordinate is inverted (0 at top, increases downwards)
        var svgY = svgBox.Height * (1 - (logicalPoint.Y - logicalBox.MinY) / logicalHeight);
        
        return new Vector2D(svgX, svgY);
    }
    
    public static Vector2D? LineIntersection(LineSegment line1, LineSegment line2)
    {
        var dx1 = line1.P2.X - line1.P1.X;
        var dy1 = line1.P2.Y - line1.P1.Y;
        var dx2 = line2.P2.X - line2.P1.X;
        var dy2 = line2.P2.Y - line2.P1.Y;
        
        var denom = dx1 * dy2 - dy1 * dx2;
        if (Math.Abs(denom) < 1e-10) return null; // Parallel lines
        
        var dx3 = line2.P1.X - line1.P1.X;
        var dy3 = line2.P1.Y - line1.P1.Y;
        
        var t = (dx3 * dy2 - dy3 * dx2) / denom;
        
        return new Vector2D(line1.P1.X + t * dx1, line1.P1.Y + t * dy1);
    }
    
    public static double VectorAngle(Vector2D v1, Vector2D v2)
    {
        var m1 = v1.Magnitude;
        var m2 = v2.Magnitude;
        
        if (m1 == 0 || m2 == 0) return 0;
        
        var dotProd = v1.Dot(v2);
        var val = dotProd / (m1 * m2);
        return Math.Acos(Math.Max(-1, Math.Min(1, val)));
    }
    
    public static string CalculateAngleArc(Vector2D p1Svg, Vector2D vSvg, Vector2D p2Svg, double radius)
    {
        var vec1 = (p1Svg - vSvg).Normalized;
        var vec2 = (p2Svg - vSvg).Normalized;
        
        var startPoint = vSvg + vec1 * radius;
        var endPoint = vSvg + vec2 * radius;
        
        var angleRad = VectorAngle(vec1, vec2);
        var largeArcFlag = angleRad > Math.PI ? 1 : 0;
        var sweepFlag = vec1.Cross(vec2) > 0 ? 1 : 0;
        
        return $"M {startPoint.X:F2},{startPoint.Y:F2} A {radius:F2},{radius:F2} 0 {largeArcFlag},{sweepFlag} {endPoint.X:F2},{endPoint.Y:F2}";
    }
    
    public static Vector2D CalculateAngleLabelPosition(Vector2D p1Svg, Vector2D vSvg, Vector2D p2Svg, double radius, double textOffsetScale)
    {
        var vec1 = (p1Svg - vSvg).Normalized;
        var vec2 = (p2Svg - vSvg).Normalized;
        var bisectorVecRaw = vec1 + vec2;
        var bisectorVec = bisectorVecRaw.Normalized;
        
        if (bisectorVecRaw.Magnitude == 0) // Opposite vectors
            return vSvg + vec1 * (radius * textOffsetScale * 0.7);
            
        return vSvg + bisectorVec * (radius * textOffsetScale);
    }
}

public record TriangleConfiguration
{
    public LogicalCoordinates LogicalCoords { get; init; } = new(-80, 80, -48, 48);
    public SvgViewBox SvgViewBox { get; init; } = new(600, 400);
    public Dictionary<string, PointDefinition> Points { get; init; } = new();
    public List<LineDefinition> Lines { get; init; } = [];
    public List<AngleDefinition> Angles { get; init; } = [];
} 