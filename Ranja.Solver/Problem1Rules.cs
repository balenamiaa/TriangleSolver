using System.Collections.Generic;

namespace Ranja.Solver;

public static class Problem1Rules
{
    public static List<IRule<Solver>> Rules =>
    [
        new IsATriangleRule(Triangle.FromString("ABC")),
        new IsATriangleRule(Triangle.FromString("AFE")),
        new IsATriangleRule(Triangle.FromString("ABE")),
        new IsATriangleRule(Triangle.FromString("ACD")), // D related, ensure D is defined or handled
        new IsATriangleRule(Triangle.FromString("FBE")),
        new IsATriangleRule(Triangle.FromString("FDE")),
        new IsATriangleRule(Triangle.FromString("FBD")),
        new IsATriangleRule(Triangle.FromString("FBC")),
        new IsATriangleRule(Triangle.FromString("FCE")),
        new IsATriangleRule(Triangle.FromString("DBC")),
        new IsATriangleRule(Triangle.FromString("DCE")),
        new IsATriangleRule(Triangle.FromString("CDE")),
        new IsATriangleRule(Triangle.FromString("BDF")),
        // Note: FDE is listed twice in original Program.cs, consolidated here
        // new IsATriangleRule(Triangle.FromString("FDE")), 

        new AngleEqualityRule(Angle.FromString("FAE"), Angle.FromString("BAC")),
        new AngleEqualityRule(Angle.FromString("ABD"), Angle.FromString("FBD")),
        new AngleEqualityRule(Angle.FromString("ACD"), Angle.FromString("ECD")),
        new AngleEqualityRule(Angle.FromString("CBD"), Angle.FromString("CBE")),
        new AngleEqualityRule(Angle.FromString("BCD"), Angle.FromString("BCF")),
        new AngleEqualityRule(Angle.FromString("FDB"), Angle.FromString("EDC")), // D point related
        new AngleEqualityRule(Angle.FromString("FDE"), Angle.FromString("BDC")), // D point related

        // Rules involving points that might not be explicitly defined in all contexts (like D)
        // Need to ensure points like D are conceptually part of the solver's understanding if these rules are active.
        // The point D is an intersection point. The solver itself doesn't compute intersections,
        // it works with symbolic relationships. These rules define how angles around D relate.
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
} 