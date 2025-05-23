using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ranja.Solver;

public class InconsistentDataException : Exception
{
    public InconsistentDataException(string message) : base(message) { }
}

public record struct Symbol(string Content);

public readonly struct Angle(Symbol p1, Symbol p2, Symbol vertex)
{
    public Symbol P1 { get; init; } = p1;
    public Symbol P2 { get; init; } = p2;
    public Symbol Vertex { get; init; } = vertex;

    public static Angle FromString(string str)
    {
        Debug.Assert(str.Length == 3, "Angle string must be 3 characters long");
        var p1Symbol = new Symbol(str[0].ToString());
        var vertexSymbol = new Symbol(str[1].ToString());
        var p2Symbol = new Symbol(str[2].ToString());
        return new Angle(p1Symbol, p2Symbol, vertexSymbol);
    }

    public override readonly bool Equals(object? obj) =>
        obj is Angle other && Vertex == other.Vertex &&
        ((P1 == other.P1 && P2 == other.P2) || (P1 == other.P2 && P2 == other.P1));

    public static bool operator ==(Angle left, Angle right) => left.Equals(right);
    public static bool operator !=(Angle left, Angle right) => !(left == right);

    public override int GetHashCode()
    {
        int hashP1 = P1.GetHashCode();
        int hashP2 = P2.GetHashCode();
        return HashCode.Combine(Vertex, Math.Min(hashP1, hashP2), Math.Max(hashP1, hashP2));
    }

    public override readonly string ToString() => $"∠{P1.Content}{Vertex.Content}{P2.Content}";
}

public readonly struct Segment(Symbol p1, Symbol p2)
{
    public Symbol P1 { get; init; } = p1;
    public Symbol P2 { get; init; } = p2;

    public static Segment FromString(string str)
    {
        Debug.Assert(str.Length == 2, "Segment string must be 2 characters long");
        var p1Symbol = new Symbol(str[0].ToString());
        var p2Symbol = new Symbol(str[1].ToString());
        return new Segment(p1Symbol, p2Symbol);
    }

    public override readonly bool Equals(object? obj) =>
        obj is Segment other &&
        ((P1 == other.P1 && P2 == other.P2) || (P1 == other.P2 && P2 == other.P1));

    public static bool operator ==(Segment left, Segment right) => left.Equals(right);
    public static bool operator !=(Segment left, Segment right) => !(left == right);

    public override readonly int GetHashCode()
    {
        int hashP1 = P1.GetHashCode();
        int hashP2 = P2.GetHashCode();
        return HashCode.Combine(Math.Min(hashP1, hashP2), Math.Max(hashP1, hashP2));
    }

    public override readonly string ToString() => $"Segment({P1.Content}{P2.Content})";
}

public readonly struct Triangle
{
    public Symbol P1 { get; }
    public Symbol P2 { get; }
    public Symbol P3 { get; }

    public Angle AngleAtP1 => new(P2, P3, P1);
    public Angle AngleAtP2 => new(P1, P3, P2);
    public Angle AngleAtP3 => new(P1, P2, P3);

    public Segment SideOppositeP1 => new(P2, P3);
    public Segment SideOppositeP2 => new(P1, P3);
    public Segment SideOppositeP3 => new(P1, P2);

    public Triangle(Symbol p1, Symbol p2, Symbol p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }

    public static Triangle FromString(string str)
    {
        Debug.Assert(str.Length == 3, "Triangle string must be 3 characters long");
        var p1Symbol = new Symbol(str[0].ToString());
        var p2Symbol = new Symbol(str[1].ToString());
        var p3Symbol = new Symbol(str[2].ToString());
        return new Triangle(p1Symbol, p2Symbol, p3Symbol);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is not Triangle other) return false;
        var pointsThis = new HashSet<Symbol> { P1, P2, P3 };
        var pointsOther = new HashSet<Symbol> { other.P1, other.P2, other.P3 };
        return pointsThis.SetEquals(pointsOther);
    }

    public static bool operator ==(Triangle left, Triangle right) => left.Equals(right);
    public static bool operator !=(Triangle left, Triangle right) => !(left == right);

    public override readonly int GetHashCode()
    {
        var hashes = new List<int> { P1.GetHashCode(), P2.GetHashCode(), P3.GetHashCode() };
        hashes.Sort();
        return HashCode.Combine(hashes[0], hashes[1], hashes[2]);
    }

    public override string ToString() => $"Triangle({P1.Content}{P2.Content}{P3.Content})";
}

public interface IValue { }

public record struct ComputedAngleValue(Angle Angle, double Value, string Reason) : IValue
{
    public override readonly string ToString() => $"{Angle}={Value:F2}° (Computed: {Reason})";
}

public record struct ComputedSegmentValue(Segment Segment, double Value, string Reason) : IValue
{
    public override readonly string ToString() => $"{Segment}={Value:F2} (Computed: {Reason})";
}

public record struct GivenAngleValue(Angle Angle, double Value) : IValue
{
    public override readonly string ToString() => $"{Angle}={Value:F2}° (Given)";
}

public record struct GivenSegmentValue(Segment Segment, double Value) : IValue
{
    public override readonly string ToString() => $"{Segment}={Value:F2} (Given)";
}

public class AngleStorage
{
    public Dictionary<Angle, List<IValue>> AngleValues { get; set; } = [];
}

public class SegmentStorage
{
    public Dictionary<Segment, List<IValue>> SegmentValues { get; set; } = [];
}

public interface ISolver { }

public interface IRule<TSolver> where TSolver : ISolver
{
    bool ApplyRule(TSolver solver);
}

public class Solver : ISolver
{
    public AngleStorage AngleStorage { get; set; } = new();
    public SegmentStorage SegmentStorage { get; set; } = new();
    public List<IRule<Solver>> Rules { get; set; } = [];
    public List<string> Inconsistencies { get; set; } = [];

    public enum SolveResult
    {
        ConsistentStable,
        InconsistentButComplete,
        MaxIterationsReached
    }

    public IEnumerable<double> GetAngleValues(Angle angle) =>
        AngleStorage.AngleValues.TryGetValue(angle, out var values)
            ? values.Select(ExtractAngleValue)
            : [];

    public IEnumerable<double> GetSegmentValues(Segment segment) =>
        SegmentStorage.SegmentValues.TryGetValue(segment, out var values)
            ? values.Select(ExtractSegmentValue)
            : [];

    public double? GetConsistentAngleValue(Angle angle)
    {
        var values = GetAngleValues(angle).ToList();
        if (values.Count == 0) return null;

        var firstValue = values.First();
        var hasInconsistency = values.Any(v => Math.Abs(v - firstValue) > Constants.EPSILON);

        if (hasInconsistency)
        {
            var inconsistency = $"Angle {angle} has conflicting values: {string.Join(", ", values.Select(v => $"{v:F2}°"))}";
            if (!Inconsistencies.Contains(inconsistency))
                Inconsistencies.Add(inconsistency);
        }

        return firstValue;
    }

    public double? GetConsistentSegmentValue(Segment segment)
    {
        var values = GetSegmentValues(segment).ToList();
        if (values.Count == 0) return null;

        var firstValue = values.First();
        var hasInconsistency = values.Any(v => Math.Abs(v - firstValue) > Constants.EPSILON);

        if (hasInconsistency)
        {
            var inconsistency = $"Segment {segment} has conflicting values: {string.Join(", ", values.Select(v => $"{v:F2}"))}";
            if (!Inconsistencies.Contains(inconsistency))
                Inconsistencies.Add(inconsistency);
        }

        return firstValue;
    }

    public bool AddAngleValue(Angle angle, double value, string reason)
    {
        var computedValue = new ComputedAngleValue(angle, value, reason);

        if (!AngleStorage.AngleValues.TryGetValue(angle, out var valueList))
        {
            valueList = [];
            AngleStorage.AngleValues[angle] = valueList;
        }

        var hasExistingValue = valueList.Any(v => Math.Abs(ExtractAngleValue(v) - value) <= Constants.EPSILON);
        if (!hasExistingValue)
        {
            valueList.Add(computedValue);
            return true;
        }

        return false;
    }

    public void AddGivenAngleValue(Angle angle, double value)
    {
        var givenValue = new GivenAngleValue(angle, value);
        if (!AngleStorage.AngleValues.TryGetValue(angle, out var valueList))
        {
            valueList = [];
            AngleStorage.AngleValues[angle] = valueList;
        }
        valueList.Add(givenValue);
    }

    public bool AddSegmentValue(Segment segment, double value, string reason)
    {
        var computedValue = new ComputedSegmentValue(segment, value, reason);

        if (!SegmentStorage.SegmentValues.TryGetValue(segment, out var valueList))
        {
            valueList = [];
            SegmentStorage.SegmentValues[segment] = valueList;
        }

        var hasExistingValue = valueList.Any(v => Math.Abs(ExtractSegmentValue(v) - value) <= Constants.EPSILON);
        if (!hasExistingValue)
        {
            valueList.Add(computedValue);
            return true;
        }

        return false;
    }

    public void AddGivenSegmentValue(Segment segment, double value)
    {
        var givenValue = new GivenSegmentValue(segment, value);
        if (!SegmentStorage.SegmentValues.TryGetValue(segment, out var valueList))
        {
            valueList = [];
            SegmentStorage.SegmentValues[segment] = valueList;
        }
        valueList.Add(givenValue);
    }

    public SolveResult Solve(int maxIterations = Constants.MAX_ITERATIONS)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            bool newInformationDerived = false;
            foreach (var rule in Rules)
            {
                try
                {
                    if (rule.ApplyRule(this))
                        newInformationDerived = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying rule {rule.GetType().Name}: {ex.Message}");
                }
            }

            if (!newInformationDerived)
            {
                return Inconsistencies.Count != 0 ? SolveResult.InconsistentButComplete : SolveResult.ConsistentStable;
            }
        }

        return SolveResult.MaxIterationsReached;
    }

    private static double ExtractAngleValue(IValue value) => value switch
    {
        GivenAngleValue gav => gav.Value,
        ComputedAngleValue cav => cav.Value,
        _ => throw new InvalidOperationException("Unknown angle value type")
    };

    private static double ExtractSegmentValue(IValue value) => value switch
    {
        GivenSegmentValue gsv => gsv.Value,
        ComputedSegmentValue csv => csv.Value,
        _ => throw new InvalidOperationException("Unknown segment value type")
    };
}

public record IsATriangleRule(Triangle Triangle) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var angle1 = Triangle.AngleAtP1;
        var angle2 = Triangle.AngleAtP2;
        var angle3 = Triangle.AngleAtP3;
        var side1 = Triangle.SideOppositeP1;
        var side2 = Triangle.SideOppositeP2;
        var side3 = Triangle.SideOppositeP3;

        var valA1 = solver.GetConsistentAngleValue(angle1);
        var valA2 = solver.GetConsistentAngleValue(angle2);
        var valA3 = solver.GetConsistentAngleValue(angle3);

        newInfo |= TryApplyAngleSum(solver, angle1, angle2, angle3, valA1, valA2, valA3);
        newInfo |= TryApplyIsoscelesRules(solver, angle1, angle2, angle3, side1, side2, side3, valA1, valA2, valA3);
        newInfo |= TryApplySineRule(solver, angle1, angle2, angle3, side1, side2, side3, valA1, valA2, valA3);

        return newInfo;
    }

    private bool TryApplyAngleSum(Solver solver, Angle angle1, Angle angle2, Angle angle3, double? valA1, double? valA2, double? valA3)
    {
        bool newInfo = false;

        if (valA1.HasValue && valA2.HasValue && !valA3.HasValue)
            newInfo |= solver.AddAngleValue(angle3, 180.0 - valA1.Value - valA2.Value, $"Sum of angles in {Triangle}");
        else if (valA1.HasValue && !valA2.HasValue && valA3.HasValue)
            newInfo |= solver.AddAngleValue(angle2, 180.0 - valA1.Value - valA3.Value, $"Sum of angles in {Triangle}");
        else if (!valA1.HasValue && valA2.HasValue && valA3.HasValue)
            newInfo |= solver.AddAngleValue(angle1, 180.0 - valA2.Value - valA3.Value, $"Sum of angles in {Triangle}");
        else if (valA1.HasValue && valA2.HasValue && valA3.HasValue)
        {
            var sum = valA1.Value + valA2.Value + valA3.Value;
            if (Math.Abs(sum - 180.0) > Constants.EPSILON)
            {
                var inconsistency = $"Angles in {Triangle} ({valA1.Value:F2}°, {valA2.Value:F2}°, {valA3.Value:F2}°) sum to {sum:F2}°, not 180°";
                if (!solver.Inconsistencies.Contains(inconsistency))
                    solver.Inconsistencies.Add(inconsistency);
            }
        }

        return newInfo;
    }

    private bool TryApplyIsoscelesRules(Solver solver, Angle angle1, Angle angle2, Angle angle3, Segment side1, Segment side2, Segment side3, double? valA1, double? valA2, double? valA3)
    {
        bool newInfo = false;
        var valS1 = solver.GetConsistentSegmentValue(side1);
        var valS2 = solver.GetConsistentSegmentValue(side2);
        var valS3 = solver.GetConsistentSegmentValue(side3);

        newInfo |= TryApplyIsoscelesRule(solver, angle1, angle2, side1, side2, valA1, valA2, valS1, valS2);
        newInfo |= TryApplyIsoscelesRule(solver, angle1, angle3, side1, side3, valA1, valA3, valS1, valS3);
        newInfo |= TryApplyIsoscelesRule(solver, angle2, angle3, side2, side3, valA2, valA3, valS2, valS3);

        return newInfo;
    }

    private bool TryApplyIsoscelesRule(Solver solver, Angle angleA, Angle angleB, Segment sideA, Segment sideB, double? valA, double? valB, double? valSA, double? valSB)
    {
        bool newInfo = false;

        if (valSA.HasValue && valSB.HasValue && Math.Abs(valSA.Value - valSB.Value) < Constants.EPSILON)
        {
            if (valA.HasValue && !valB.HasValue)
                newInfo |= solver.AddAngleValue(angleB, valA.Value, $"Isosceles {Triangle}, {sideA}={sideB}");
            else if (!valA.HasValue && valB.HasValue)
                newInfo |= solver.AddAngleValue(angleA, valB.Value, $"Isosceles {Triangle}, {sideA}={sideB}");
        }

        if (valA.HasValue && valB.HasValue && Math.Abs(valA.Value - valB.Value) < Constants.EPSILON)
        {
            if (valSA.HasValue && !valSB.HasValue)
                newInfo |= solver.AddSegmentValue(sideB, valSA.Value, $"Isosceles {Triangle}, {angleA}={angleB}");
            else if (!valSA.HasValue && valSB.HasValue)
                newInfo |= solver.AddSegmentValue(sideA, valSB.Value, $"Isosceles {Triangle}, {angleA}={angleB}");
        }

        return newInfo;
    }

    private bool TryApplySineRule(Solver solver, Angle angle1, Angle angle2, Angle angle3, Segment side1, Segment side2, Segment side3, double? valA1, double? valA2, double? valA3)
    {
        bool newInfo = false;
        static double degToRad(double deg) => deg * Math.PI / 180.0;
        static double radToDeg(double rad) => rad * 180.0 / Math.PI;

        var valS1 = solver.GetConsistentSegmentValue(side1);
        var valS2 = solver.GetConsistentSegmentValue(side2);
        var valS3 = solver.GetConsistentSegmentValue(side3);

        var ratio = GetSineRatio(valA1, valS1, degToRad) ?? GetSineRatio(valA2, valS2, degToRad) ?? GetSineRatio(valA3, valS3, degToRad);

        if (ratio.HasValue)
        {
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle1, side1, valA1, valS1, ratio.Value, degToRad, radToDeg);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle2, side2, valA2, valS2, ratio.Value, degToRad, radToDeg);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle3, side3, valA3, valS3, ratio.Value, degToRad, radToDeg);
        }

        return newInfo;
    }

    private static double? GetSineRatio(double? angle, double? side, Func<double, double> degToRad) =>
        angle.HasValue && side.HasValue && angle.Value > Constants.EPSILON && Math.Abs(angle.Value - 180.0) > Constants.EPSILON
            ? side.Value / Math.Sin(degToRad(angle.Value))
            : null;

    private bool TryApplySineRuleForAngleAndSide(Solver solver, Angle angle, Segment side, double? valA, double? valS, double ratio, Func<double, double> degToRad, Func<double, double> radToDeg)
    {
        bool newInfo = false;

        if (!valS.HasValue && valA.HasValue && valA.Value > Constants.EPSILON && Math.Abs(valA.Value - 180.0) > Constants.EPSILON)
            newInfo |= solver.AddSegmentValue(side, ratio * Math.Sin(degToRad(valA.Value)), $"Sine Rule on {Triangle} for {side}");
        else if (!valA.HasValue && valS.HasValue && valS.Value > Constants.EPSILON && ratio > Constants.EPSILON)
        {
            var sinA = valS.Value / ratio;
            if (sinA >= -1.0 && sinA <= 1.0)
                newInfo |= solver.AddAngleValue(angle, radToDeg(Math.Asin(sinA)), $"Sine Rule on {Triangle} for {angle}");
        }

        return newInfo;
    }
}

public record AngleEqualityRule(Angle Angle1, Angle Angle2) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var val1 = solver.GetConsistentAngleValue(Angle1);
        var val2 = solver.GetConsistentAngleValue(Angle2);

        if (val1.HasValue && !val2.HasValue)
            newInfo |= solver.AddAngleValue(Angle2, val1.Value, $"Angle Equality {Angle1}={Angle2}");
        else if (!val1.HasValue && val2.HasValue)
            newInfo |= solver.AddAngleValue(Angle1, val2.Value, $"Angle Equality {Angle2}={Angle1}");
        else if (val1.HasValue && val2.HasValue && Math.Abs(val1.Value - val2.Value) > Constants.EPSILON)
        {
            var inconsistency = $"Angle Equality Conflict: {Angle1} ({val1.Value:F2}°) != {Angle2} ({val2.Value:F2}°)";
            if (!solver.Inconsistencies.Contains(inconsistency))
                solver.Inconsistencies.Add(inconsistency);
        }

        return newInfo;
    }
}

public record AnglesAddUpToRule(IEnumerable<Angle> Angles, double Value) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var unknownAngles = new List<Angle>();
        double knownSum = 0;

        foreach (var angle in Angles)
        {
            var val = solver.GetConsistentAngleValue(angle);
            if (val.HasValue)
                knownSum += val.Value;
            else
                unknownAngles.Add(angle);
        }

        if (unknownAngles.Count == 1)
            newInfo |= solver.AddAngleValue(unknownAngles[0], Value - knownSum, $"Sum of angles = {Value}°");
        else if (unknownAngles.Count == 0 && Angles.Any())
        {
            if (Math.Abs(knownSum - Value) > Constants.EPSILON)
            {
                var inconsistency = $"Angles sum to {knownSum:F2}°, expected {Value}°";
                if (!solver.Inconsistencies.Contains(inconsistency))
                    solver.Inconsistencies.Add(inconsistency);
            }
        }

        return newInfo;
    }
}

public record AngleContainsSubAnglesRule(Angle Parent, IEnumerable<Angle> SubAngles) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var parentVal = solver.GetConsistentAngleValue(Parent);
        var unknownSubAngles = new List<Angle>();
        double knownSubAnglesSum = 0;

        foreach (var subAngle in SubAngles)
        {
            var val = solver.GetConsistentAngleValue(subAngle);
            if (val.HasValue)
                knownSubAnglesSum += val.Value;
            else
                unknownSubAngles.Add(subAngle);
        }

        if (!parentVal.HasValue && unknownSubAngles.Count == 0 && SubAngles.Any())
            newInfo |= solver.AddAngleValue(Parent, knownSubAnglesSum, $"Parent angle {Parent} from sum of sub-angles");
        else if (parentVal.HasValue && unknownSubAngles.Count == 1)
            newInfo |= solver.AddAngleValue(unknownSubAngles[0], parentVal.Value - knownSubAnglesSum, $"Sub-angle from parent {Parent}");
        else if (parentVal.HasValue && unknownSubAngles.Count == 0 && SubAngles.Any())
        {
            if (Math.Abs(parentVal.Value - knownSubAnglesSum) > Constants.EPSILON)
            {
                var inconsistency = $"Parent angle {Parent} ({parentVal.Value:F2}°) != sum of sub-angles ({knownSubAnglesSum:F2}°)";
                if (!solver.Inconsistencies.Contains(inconsistency))
                    solver.Inconsistencies.Add(inconsistency);
            }
        }

        return newInfo;
    }
}

public static class Constants
{
    public const double EPSILON = 1e-6;
    public const double EPSILON_SINE_RATIO = 1e-4;
    public const int MAX_ITERATIONS = 100;
}