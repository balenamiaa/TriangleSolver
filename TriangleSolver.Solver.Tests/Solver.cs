namespace TriangleSolver.Solver.Tests;

using System.Linq;
using Xunit;

public class SolverTests
{
    public static List<IRule<Solver>> TestRules => [
        new IsATriangleRule(Triangle.FromString("ABC")),
        new IsATriangleRule(Triangle.FromString("AFE")),
        new IsATriangleRule(Triangle.FromString("ABE")),
        new IsATriangleRule(Triangle.FromString("ACD")),
        new IsATriangleRule(Triangle.FromString("FBE")),
        new IsATriangleRule(Triangle.FromString("FDE")),
        new IsATriangleRule(Triangle.FromString("FBD")),
        new IsATriangleRule(Triangle.FromString("FBC")),
        new IsATriangleRule(Triangle.FromString("FCE")),
        new IsATriangleRule(Triangle.FromString("DBC")),
        new IsATriangleRule(Triangle.FromString("DCE")),
        new IsATriangleRule(Triangle.FromString("CDE")),
        new IsATriangleRule(Triangle.FromString("BDF")),
        new IsATriangleRule(Triangle.FromString("FDE")),

        new AngleEqualityRule(Angle.FromString("FAE"), Angle.FromString("BAC")),
        new AngleEqualityRule(Angle.FromString("ABD"), Angle.FromString("FBD")),
        new AngleEqualityRule(Angle.FromString("ACD"), Angle.FromString("ECD")),
        new AngleEqualityRule(Angle.FromString("CBD"), Angle.FromString("CBE")),
        new AngleEqualityRule(Angle.FromString("BCD"), Angle.FromString("BCF")),
        new AngleEqualityRule(Angle.FromString("FDB"), Angle.FromString("EDC")),
        new AngleEqualityRule(Angle.FromString("FDE"), Angle.FromString("BDC")),

        new AnglesAddUpToRule([Angle.FromString("BDC"), Angle.FromString("CDE")], 180),
        new AnglesAddUpToRule([Angle.FromString("CDB"), Angle.FromString("BDF")], 180),
        new AnglesAddUpToRule([Angle.FromString("AFC"), Angle.FromString("CFB")], 180),
        new AnglesAddUpToRule([Angle.FromString("AEB"), Angle.FromString("BEC")], 180),

        new AngleContainsSubAnglesRule(Angle.FromString("CBA"), [Angle.FromString("CBD"), Angle.FromString("DBA")]),
        new AngleContainsSubAnglesRule(Angle.FromString("BCA"), [Angle.FromString("BCD"), Angle.FromString("DCA")]),
        new AngleContainsSubAnglesRule(Angle.FromString("BFE"), [Angle.FromString("BFD"), Angle.FromString("DFE")]),
        new AngleContainsSubAnglesRule(Angle.FromString("CEF"), [Angle.FromString("CED"), Angle.FromString("DEF")]),
        new AngleContainsSubAnglesRule(Angle.FromString("AFC"), [Angle.FromString("AFD"), Angle.FromString("DFC")]),
        new AngleContainsSubAnglesRule(Angle.FromString("AEB"), [Angle.FromString("AED"), Angle.FromString("DEB")]),
    ];

    [Fact]
    public void TestAngleEquality()
    {
        var angle1 = new Angle(new Symbol("A"), new Symbol("B"), new Symbol("C"));
        var angle2 = new Angle(new Symbol("B"), new Symbol("A"), new Symbol("C"));
        Assert.True(angle1 == angle2);

        var angle3 = Angle.FromString("ABC");
        var angle4 = Angle.FromString("CBA");
        Assert.True(angle3 == angle4);
    }

    [Fact]
    public void TestSegmentEquality()
    {
        var segment1 = new Segment(new Symbol("A"), new Symbol("B"));
        var segment2 = new Segment(new Symbol("B"), new Symbol("A"));
        Assert.True(segment1 == segment2);

        var segment3 = Segment.FromString("AB");
        var segment4 = Segment.FromString("BA");
        Assert.True(segment3 == segment4);
    }

    [Fact]
    public void TestTriangleEquality()
    {
        var triangle3 = new Triangle(new Symbol("A"), new Symbol("B"), new Symbol("C"));
        var triangle4 = new Triangle(new Symbol("C"), new Symbol("B"), new Symbol("A"));
        var triangle5 = new Triangle(new Symbol("B"), new Symbol("A"), new Symbol("C"));
        var triangle6 = Triangle.FromString("ABC");
        var triangle7 = Triangle.FromString("CBA");

        Assert.True(triangle3 == triangle4);
        Assert.True(triangle3 == triangle5);
        Assert.True(triangle3 == triangle6);
        Assert.True(triangle3 == triangle7);
    }

    [Fact]
    public void TestAddGivenAngleValue()
    {
        var solver = new Solver();
        var angle = Angle.FromString("ABC");

        solver.AddGivenAngleValue(angle, 45.0);

        var value = solver.GetConsistentAngleValue(angle);
        Assert.True(value.HasValue);
        Assert.Equal(45.0, value.Value, precision: 5);
    }

    [Fact]
    public void TestAddGivenSegmentValue()
    {
        var solver = new Solver();
        var segment = Segment.FromString("AB");

        solver.AddGivenSegmentValue(segment, 10.0);

        var value = solver.GetConsistentSegmentValue(segment);
        Assert.True(value.HasValue);
        Assert.Equal(10.0, value.Value, precision: 5);
    }

    [Fact]
    public void TestAddComputedAngleValue()
    {
        var solver = new Solver();
        var angle = Angle.FromString("ABC");

        var added = solver.AddAngleValue(angle, 60.0, "Test reason");
        Assert.True(added);

        var value = solver.GetConsistentAngleValue(angle);
        Assert.True(value.HasValue);
        Assert.Equal(60.0, value.Value, precision: 5);
    }

    [Fact]
    public void TestAddDuplicateAngleValue()
    {
        var solver = new Solver();
        var angle = Angle.FromString("ABC");

        solver.AddAngleValue(angle, 60.0, "First");
        var secondAdd = solver.AddAngleValue(angle, 60.0, "Second");

        Assert.False(secondAdd);
        Assert.Single(solver.AngleStorage.AngleValues[angle]);
    }

    [Fact]
    public void TestConflictingAngleValues()
    {
        var solver = new Solver();
        var angle = Angle.FromString("ABC");

        solver.AddAngleValue(angle, 60.0, "First");
        solver.AddAngleValue(angle, 45.0, "Second");

        var value = solver.GetConsistentAngleValue(angle);
        Assert.True(value.HasValue);
        Assert.Equal(60.0, value.Value, precision: 5);
        Assert.Single(solver.Inconsistencies);
    }

    [Fact]
    public void TestAngleEqualityRule()
    {
        var solver = new Solver();
        var angle1 = Angle.FromString("ABC");
        var angle2 = Angle.FromString("DEF");
        var rule = new AngleEqualityRule(angle1, angle2);

        solver.AddGivenAngleValue(angle1, 45.0);

        var applied = rule.ApplyRule(solver);
        Assert.True(applied);

        var value2 = solver.GetConsistentAngleValue(angle2);
        Assert.True(value2.HasValue);
        Assert.Equal(45.0, value2.Value, precision: 5);
    }

    [Fact]
    public void TestAngleEqualityRuleConflict()
    {
        var solver = new Solver();
        var angle1 = Angle.FromString("ABC");
        var angle2 = Angle.FromString("DEF");
        var rule = new AngleEqualityRule(angle1, angle2);

        solver.AddGivenAngleValue(angle1, 45.0);
        solver.AddGivenAngleValue(angle2, 60.0);

        rule.ApplyRule(solver);
        Assert.Single(solver.Inconsistencies);
        Assert.Contains("Angle Equality Conflict", solver.Inconsistencies[0]);
    }

    [Fact]
    public void TestAnglesAddUpToRule()
    {
        var solver = new Solver();
        var angle1 = Angle.FromString("ABC");
        var angle2 = Angle.FromString("CBD");
        var angle3 = Angle.FromString("DBE");
        var rule = new AnglesAddUpToRule([angle1, angle2, angle3], 180.0);

        solver.AddGivenAngleValue(angle1, 60.0);
        solver.AddGivenAngleValue(angle2, 70.0);

        var applied = rule.ApplyRule(solver);
        Assert.True(applied);

        var value3 = solver.GetConsistentAngleValue(angle3);
        Assert.True(value3.HasValue);
        Assert.Equal(50.0, value3.Value, precision: 5);
    }

    [Fact]
    public void TestIsATriangleRuleAngleSum()
    {
        var solver = new Solver();
        var triangle = Triangle.FromString("ABC");
        var rule = new IsATriangleRule(triangle);

        solver.AddGivenAngleValue(triangle.AngleAtP1, 60.0);
        solver.AddGivenAngleValue(triangle.AngleAtP2, 70.0);

        var applied = rule.ApplyRule(solver);
        Assert.True(applied);

        var angleC = solver.GetConsistentAngleValue(triangle.AngleAtP3);
        Assert.True(angleC.HasValue);
        Assert.Equal(50.0, angleC.Value, precision: 5);
    }

    [Fact]
    public void TestIsATriangleRuleInvalidSum()
    {
        var solver = new Solver();
        var triangle = Triangle.FromString("ABC");
        var rule = new IsATriangleRule(triangle);

        solver.AddGivenAngleValue(triangle.AngleAtP1, 60.0);
        solver.AddGivenAngleValue(triangle.AngleAtP2, 70.0);
        solver.AddGivenAngleValue(triangle.AngleAtP3, 60.0);

        rule.ApplyRule(solver);
        Assert.Single(solver.Inconsistencies);
        Assert.Contains("sum to", solver.Inconsistencies[0]);
    }

    [Fact]
    public void TestSolveEmptySolver()
    {
        var solver = new Solver();
        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.ConsistentStable, result);
        Assert.Empty(solver.Inconsistencies);
    }

    [Fact]
    public void TestSolveBasicTriangle()
    {
        var solver = new Solver();
        var triangle = Triangle.FromString("ABC");
        solver.Rules = [new IsATriangleRule(triangle)];

        solver.AddGivenAngleValue(triangle.AngleAtP1, 60.0);
        solver.AddGivenAngleValue(triangle.AngleAtP2, 70.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.ConsistentStable, result);
        Assert.Empty(solver.Inconsistencies);

        var angleC = solver.GetConsistentAngleValue(triangle.AngleAtP3);
        Assert.True(angleC.HasValue);
        Assert.Equal(50.0, angleC.Value, precision: 5);
    }

    [Fact]
    public void TestSolveWithInconsistency()
    {
        var solver = new Solver();
        var triangle = Triangle.FromString("ABC");
        solver.Rules = [new IsATriangleRule(triangle)];

        solver.AddGivenAngleValue(triangle.AngleAtP1, 60.0);
        solver.AddGivenAngleValue(triangle.AngleAtP2, 70.0);
        solver.AddGivenAngleValue(triangle.AngleAtP3, 60.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.InconsistentButComplete, result);
        Assert.NotEmpty(solver.Inconsistencies);
    }

    [Fact]
    public void TestSolveWithAngleEquality()
    {
        var solver = new Solver();
        var angle1 = Angle.FromString("ABC");
        var angle2 = Angle.FromString("DEF");
        solver.Rules = [new AngleEqualityRule(angle1, angle2)];

        solver.AddGivenAngleValue(angle1, 45.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.ConsistentStable, result);
        Assert.Empty(solver.Inconsistencies);

        var value2 = solver.GetConsistentAngleValue(angle2);
        Assert.True(value2.HasValue);
        Assert.Equal(45.0, value2.Value, precision: 5);
    }

    [Fact]
    public void TestGetAngleValues()
    {
        var solver = new Solver();
        var angle = Angle.FromString("ABC");

        solver.AddGivenAngleValue(angle, 45.0);
        solver.AddAngleValue(angle, 46.0, "Computed");

        var values = solver.GetAngleValues(angle).ToList();
        Assert.Equal(2, values.Count);
        Assert.Contains(45.0, values);
        Assert.Contains(46.0, values);
    }

    [Fact]
    public void TestGetSegmentValues()
    {
        var solver = new Solver();
        var segment = Segment.FromString("AB");

        solver.AddGivenSegmentValue(segment, 10.0);
        solver.AddSegmentValue(segment, 11.0, "Computed");

        var values = solver.GetSegmentValues(segment).ToList();
        Assert.Equal(2, values.Count);
        Assert.Contains(10.0, values);
        Assert.Contains(11.0, values);
    }

    [Fact]
    public void TestAngleContainsSubAnglesRule()
    {
        var solver = new Solver();
        var parent = Angle.FromString("ABC");
        var sub1 = Angle.FromString("ABD");
        var sub2 = Angle.FromString("DBC");
        var rule = new AngleContainsSubAnglesRule(parent, [sub1, sub2]);

        solver.AddGivenAngleValue(sub1, 30.0);
        solver.AddGivenAngleValue(sub2, 40.0);

        var applied = rule.ApplyRule(solver);
        Assert.True(applied);

        var parentValue = solver.GetConsistentAngleValue(parent);
        Assert.True(parentValue.HasValue);
        Assert.Equal(70.0, parentValue.Value, precision: 5);
    }

    [Fact]
    public void TestGeometryProblemX20()
    {
        var solver = new Solver
        {
            Rules = TestRules
        };

        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), 20);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.InconsistentButComplete, result);
        Assert.NotEmpty(solver.Inconsistencies);
    }

    [Fact]
    public void TestGeometryProblemX30()
    {
        var solver = new Solver
        {
            Rules = TestRules
        };

        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), 30);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.ConsistentStable, result);
        Assert.Empty(solver.Inconsistencies);
    }

    [Fact]
    public void TestGeometryProblemX80()
    {
        var solver = new Solver
        {
            Rules = TestRules
        };

        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), 80);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.InconsistentButComplete, result);
        Assert.NotEmpty(solver.Inconsistencies);
    }

    [Fact]
    public void TestGeometryProblemX35()
    {
        var solver = new Solver
        {
            Rules = TestRules
        };

        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), 35);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.InconsistentButComplete, result);
        Assert.NotEmpty(solver.Inconsistencies);
        Assert.Contains(solver.Inconsistencies, inc => inc.Contains("Sine Rule violation"));
    }

    [Fact]
    public void TestGeometryProblemX36()
    {
        var solver = new Solver
        {
            Rules = TestRules
        };

        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), 36);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        var result = solver.Solve();

        Assert.Equal(Solver.SolveResult.InconsistentButComplete, result);
        Assert.NotEmpty(solver.Inconsistencies);
        Assert.Contains(solver.Inconsistencies, inc => inc.Contains("Sine Rule violation"));
    }
}