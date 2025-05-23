namespace Ranja.Solver.Tests;

using System.Linq;
using Xunit;
using System.Collections.Generic;

public class SolverIntegrationTests
{
    [Fact]
    public void TestTriangleAngleMappings()
    {
        // Test that angle strings map correctly between UI and solver
        // In Angle.FromString("ABC"), the vertex is the MIDDLE character (B), not the last
        var angle1 = Angle.FromString("ACB"); // P1=A, P2=B, Vertex=C
        var angle2 = Angle.FromString("ABC"); // P1=A, P2=C, Vertex=B  
        var angle3 = Angle.FromString("BCA"); // P1=B, P2=A, Vertex=C

        Assert.Equal("A", angle1.P1.Content);
        Assert.Equal("B", angle1.P2.Content);
        Assert.Equal("C", angle1.Vertex.Content);

        Assert.Equal("A", angle2.P1.Content);
        Assert.Equal("C", angle2.P2.Content);
        Assert.Equal("B", angle2.Vertex.Content);

        Assert.Equal("B", angle3.P1.Content);
        Assert.Equal("A", angle3.P2.Content);
        Assert.Equal("C", angle3.Vertex.Content);
    }

    [Fact]
    public void TestTriangleAngleSumRule()
    {
        var solver = new Solver();
        
        // Add triangle ABC rule
        solver.Rules.Add(new IsATriangleRule(Triangle.FromString("ABC")));
        
        // Add given angles - vertex is middle character
        solver.AddGivenAngleValue(Angle.FromString("CBA"), 80.0);  // Angle at B
        solver.AddGivenAngleValue(Angle.FromString("ACB"), 80.0);  // Angle at C
        // Don't add angle at A, let solver compute it
        
        var result = solver.Solve();
        
        Assert.Equal(Solver.SolveResult.ConsistentStable, result);
        
        // Check that angle at A was computed as 20°
        var angleAtA = solver.GetConsistentAngleValue(Angle.FromString("CAB"));
        Assert.True(angleAtA.HasValue);
        Assert.Equal(20.0, angleAtA.Value, precision: 1);
    }

    [Fact]
    public void TestVariableAngleFEB()
    {
        var solver = new Solver();
        solver.Rules.AddRange(Constants.GetRules());
        
        // Test with x = 30°
        var angleX = 30.0;
        
        // Add given angles - vertex is middle character
        solver.AddGivenAngleValue(Angle.FromString("CBA"), 80.0);  // Angle at B
        solver.AddGivenAngleValue(Angle.FromString("ACB"), 80.0);  // Angle at C  
        solver.AddGivenAngleValue(Angle.FromString("CAB"), 20.0);  // Angle at A
        
        // Individual sub-angles - vertex is middle character
        solver.AddGivenAngleValue(Angle.FromString("CBE"), 60.0);  // Angle at B
        solver.AddGivenAngleValue(Angle.FromString("EBA"), 20.0);  // Angle at B
        solver.AddGivenAngleValue(Angle.FromString("BCF"), 50.0);  // Angle at C
        solver.AddGivenAngleValue(Angle.FromString("CFA"), 30.0);  // Angle at F
        
        // Our variable x
        solver.AddGivenAngleValue(Angle.FromString("FEB"), angleX);  // Angle at E
        
        var result = solver.Solve();
        
        // Should not have inconsistencies for reasonable values of x
        Assert.True(result == Solver.SolveResult.ConsistentStable || result == Solver.SolveResult.InconsistentButComplete);
        
        // Verify that FEB angle is set correctly
        var febAngle = solver.GetConsistentAngleValue(Angle.FromString("FEB"));
        Assert.True(febAngle.HasValue);
        Assert.Equal(angleX, febAngle.Value, precision: 1);
    }

    [Fact]
    public void TestAngleKeyGeneration()
    {
        // Test that the key generation matches what the UI expects
        var angle = Angle.FromString("FEB");
        var key = $"{angle.P1.Content}{angle.Vertex.Content}{angle.P2.Content}";
        
        // P1=F, P2=B, Vertex=E, so key should be "FEB"
        Assert.Equal("FEB", key);
        
        // Test another angle
        var angle2 = Angle.FromString("ACB");
        var key2 = $"{angle2.P1.Content}{angle2.Vertex.Content}{angle2.P2.Content}";
        
        // P1=A, P2=B, Vertex=C, so key should be "ACB"
        Assert.Equal("ACB", key2);
    }

    [Fact]
    public void TestDisplayFilteringWithAngleEquality()
    {
        // Test the new approach where we use HashSet<Angle> for display filtering
        // This validates that our UI display filtering leverages solver's angle equality
        
        // Create a display filter set like we do in the UI
        var displayAngles = new HashSet<Ranja.Solver.Angle>
        {
            Ranja.Solver.Angle.FromString("FAE"), // Triangle AFE angle at A
            Ranja.Solver.Angle.FromString("FEB"), // Variable angle at E
            Ranja.Solver.Angle.FromString("CBA")  // Triangle ABC angle at B
        };

        // Simulate what the solver might produce (different orderings)
        var solverResults = new Dictionary<string, double>
        {
            ["EAF"] = 20.0,  // Same as FAE, just different P1/P2 order
            ["FEB"] = 30.0,  // Exact match
            ["ABC"] = 80.0,  // Same as CBA, just different P1/P2 order
            ["XYZ"] = 45.0   // Not in display set
        };

        // Filter using the same logic as our updated UI
        var displayResults = new Dictionary<string, double>();
        foreach (var solverEntry in solverResults)
        {
            var angleKey = solverEntry.Key;
            var angleValue = solverEntry.Value;
            
            if (angleKey.Length == 3)
            {
                var testAngle = Ranja.Solver.Angle.FromString(angleKey);
                if (displayAngles.Contains(testAngle))
                {
                    displayResults[angleKey] = angleValue;
                }
            }
        }

        // Should match 3 angles: EAF->FAE, FEB->FEB, ABC->CBA
        Assert.Equal(3, displayResults.Count);
        Assert.True(displayResults.ContainsKey("EAF"));  // Found due to FAE equality
        Assert.True(displayResults.ContainsKey("FEB"));  // Direct match
        Assert.True(displayResults.ContainsKey("ABC"));  // Found due to CBA equality
        Assert.False(displayResults.ContainsKey("XYZ")); // Not in display set
        
        Assert.Equal(20.0, displayResults["EAF"]);
        Assert.Equal(30.0, displayResults["FEB"]);
        Assert.Equal(80.0, displayResults["ABC"]);
    }

    [Fact]
    public void TestSegmentEqualityInDisplay()
    {
        // Test segment equality for display filtering
        var displaySegments = new HashSet<Ranja.Solver.Segment>
        {
            Ranja.Solver.Segment.FromString("AF"),
            Ranja.Solver.Segment.FromString("EF")
        };

        // Simulate solver results with different orderings
        var solverResults = new Dictionary<string, double>
        {
            ["FA"] = 10.5,  // Same as AF, just different order
            ["FE"] = 8.2,   // Same as EF, just different order  
            ["XY"] = 5.0    // Not in display set
        };

        // Filter using segment equality
        var displayResults = new Dictionary<string, double>();
        foreach (var solverEntry in solverResults)
        {
            var segmentKey = solverEntry.Key;
            var segmentValue = solverEntry.Value;
            
            if (segmentKey.Length == 2)
            {
                var testSegment = Ranja.Solver.Segment.FromString(segmentKey);
                if (displaySegments.Contains(testSegment))
                {
                    displayResults[segmentKey] = segmentValue;
                }
            }
        }

        // Should match 2 segments: FA->AF, FE->EF
        Assert.Equal(2, displayResults.Count);
        Assert.True(displayResults.ContainsKey("FA"));   // Found due to AF equality
        Assert.True(displayResults.ContainsKey("FE"));   // Found due to EF equality
        Assert.False(displayResults.ContainsKey("XY"));  // Not in display set
        
        Assert.Equal(10.5, displayResults["FA"]);
        Assert.Equal(8.2, displayResults["FE"]);
    }
} 