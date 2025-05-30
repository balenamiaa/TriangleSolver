namespace TriangleSolver.Client.Models;

using TriangleSolver.Solver;

public static class Problem1Rules
{
    public static List<IRule<Solver>> Rules => [
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

        new AngleEqualityRule(Angle.FromString("FAE"), Angle.FromString("BAC")),
        new AngleEqualityRule(Angle.FromString("ABD"), Angle.FromString("FBD")),
        new AngleEqualityRule(Angle.FromString("ACD"), Angle.FromString("ECD")),
        new AngleEqualityRule(Angle.FromString("CBD"), Angle.FromString("CBE")),
        new AngleEqualityRule(Angle.FromString("BCD"), Angle.FromString("BCF")),
        new AngleEqualityRule(Angle.FromString("FDB"), Angle.FromString("EDC")),
        new AngleEqualityRule(Angle.FromString("FDE"), Angle.FromString("BDC")),
        new AngleEqualityRule(Angle.FromString("CEB"), Angle.FromString("CED")),

        new AnglesAddUpToRule([Angle.FromString("BDC"), Angle.FromString("CDE")], 180),
        new AnglesAddUpToRule([Angle.FromString("CDB"), Angle.FromString("BDF")], 180),
        new AnglesAddUpToRule([Angle.FromString("AFE"), Angle.FromString("EFC"), Angle.FromString("BFC")], 180),
        new AnglesAddUpToRule([Angle.FromString("AEF"), Angle.FromString("FEB"), Angle.FromString("BEC")], 180),

        new AngleContainsSubAnglesRule(Angle.FromString("CBA"), [Angle.FromString("CBD"), Angle.FromString("DBA")]),
        new AngleContainsSubAnglesRule(Angle.FromString("BCA"), [Angle.FromString("BCD"), Angle.FromString("DCA")]),
        new AngleContainsSubAnglesRule(Angle.FromString("BFE"), [Angle.FromString("BFD"), Angle.FromString("DFE")]),
        new AngleContainsSubAnglesRule(Angle.FromString("CEF"), [Angle.FromString("CED"), Angle.FromString("DEF")]),
        new AngleContainsSubAnglesRule(Angle.FromString("AFC"), [Angle.FromString("AFD"), Angle.FromString("DFC")]),
        new AngleContainsSubAnglesRule(Angle.FromString("AEB"), [Angle.FromString("AED"), Angle.FromString("DEB")]),
    ];
}