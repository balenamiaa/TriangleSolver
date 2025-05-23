namespace Ranja.Solver.Tests;

using Xunit;
using System.Collections.Generic;

public class AngleParsingTest
{
    [Fact]
    public void TestAngleFromStringParsing()
    {
        var angle = Angle.FromString("FEB");
        
        // Let's see what we actually get
        var p1 = angle.P1.Content;
        var p2 = angle.P2.Content;
        var vertex = angle.Vertex.Content;
        
        // According to the constructor: Angle(Symbol p1, Symbol p2, Symbol vertex)
        // And FromString does: new Angle(p1Symbol, p2Symbol, vertexSymbol)
        // Where p1Symbol = str[0], p2Symbol = str[2], vertexSymbol = str[1]
        
        // So for "FEB": p1=F, p2=B, vertex=E (middle character!)
        Assert.Equal("F", p1);
        Assert.Equal("B", p2);
        Assert.Equal("E", vertex);
        
        // Now test ToString to see the format
        var toString = angle.ToString();
        // ToString does: $"∠{P1.Content}{Vertex.Content}{P2.Content}"
        // So it should be "∠FEB"
        Assert.Equal("∠FEB", toString);
    }

    [Fact]
    public void TestAngleEquality()
    {
        // Test the key functionality we're using in UI display filtering
        // Angle.FromString("FAE") should equal Angle.FromString("EAF")
        var angle1 = Angle.FromString("FAE"); // P1=F, P2=E, Vertex=A
        var angle2 = Angle.FromString("EAF"); // P1=E, P2=F, Vertex=A
        
        // These should be equal because they have the same vertex and the P1/P2 are swapped
        Assert.Equal(angle1, angle2);
        Assert.True(angle1 == angle2);
        Assert.False(angle1 != angle2);
        
        // HashSet should treat them as the same
        var angleSet = new HashSet<Angle> { angle1 };
        Assert.Contains(angle2, angleSet);
        Assert.Single(angleSet);
        
        // Adding the "same" angle shouldn't increase count
        angleSet.Add(angle2);
        Assert.Single(angleSet);
    }

    [Fact]
    public void TestAngleInequalityDifferentVertex()
    {
        // Test that angles with different vertices are NOT equal
        var angle1 = Angle.FromString("FAE"); // Vertex=A
        var angle2 = Angle.FromString("FBE"); // Vertex=B
        
        Assert.NotEqual(angle1, angle2);
        Assert.False(angle1 == angle2);
        Assert.True(angle1 != angle2);
        
        // HashSet should treat them as different
        var angleSet = new HashSet<Angle> { angle1, angle2 };
        Assert.Equal(2, angleSet.Count);
    }
} 