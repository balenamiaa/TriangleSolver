using Ranja.Solver;
using System.Diagnostics;

public class Program
{
    public static List<IRule<Solver>> ProblemRules => [
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

    public static void Main(string[] args)
    {
        Console.WriteLine("--- Running Test Case: x = 60 ---");
        RunTest(60.0);

        Console.WriteLine("\n--- Running Test Case: x = 30 ---");
        RunTest(30.0);

        Console.WriteLine("\n--- Running Test Case: x = 29.9 ---");
        RunTest(29.9);

        Console.WriteLine("\n--- Running Test Case: x = 80 ---");
        RunTest(80.0);

        // Add detailed debugging for x=20 case
        Console.WriteLine("\n--- Detailed Analysis for x = 20 ---");
        RunDetailedAnalysis(20.0);
    }

    public static void RunTest(double xValue)
    {
        var solver = new Solver();
        solver.Rules = ProblemRules;

        Console.WriteLine($"Initial values for x = {xValue}°:");

        try
        {
            solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
            solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
            solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
            solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
            solver.AddGivenAngleValue(Angle.FromString("FED"), xValue);
            solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

            Console.WriteLine("\nGiven values added. Starting solver...");

            var originalLevel = Trace.Listeners.Count;
            Trace.Listeners.Add(new ConsoleTraceListener());

            var result = solver.Solve();

            if (Trace.Listeners.Count > originalLevel)
            {
                Trace.Listeners.RemoveAt(Trace.Listeners.Count - 1);
            }

            Console.WriteLine($"\nSolver finished with result: {result}");

            if (result == Solver.SolveResult.ConsistentStable)
            {
                Console.WriteLine("✓ Solution is consistent and stable!");
            }
            else if (result == Solver.SolveResult.InconsistentButComplete)
            {
                Console.WriteLine("❌ Solution has inconsistencies but computation completed");
            }
            else if (result == Solver.SolveResult.MaxIterationsReached)
            {
                Console.WriteLine("⚠ Maximum iterations reached - solution may be incomplete");
            }

            if (solver.Inconsistencies.Any())
            {
                Console.WriteLine($"\n❌ INCONSISTENCIES DETECTED ({solver.Inconsistencies.Count}):");
                foreach (var inconsistency in solver.Inconsistencies)
                {
                    Console.WriteLine($"   • {inconsistency}");
                }
                Console.WriteLine($"\n   This proves that x = {xValue}° is INCORRECT for this geometry problem.");
            }

            Console.WriteLine("\n--- UI-Style Angle Keys ---");
            foreach (var kvp in solver.AngleStorage.AngleValues)
            {
                var angle = kvp.Key;
                var value = solver.GetConsistentAngleValue(angle);
                if (value.HasValue)
                {
                    var angleKey = $"{angle.P1.Content}{angle.Vertex.Content}{angle.P2.Content}";
                    Console.WriteLine($"{angleKey}: {value.Value:F1}°");
                }
            }

            Console.WriteLine("\n--- UI-Style Segment Keys ---");
            foreach (var kvp in solver.SegmentStorage.SegmentValues)
            {
                var segment = kvp.Key;
                var value = solver.GetConsistentSegmentValue(segment);
                if (value.HasValue)
                {
                    var segmentKey = string.Join("", new[] { segment.P1.Content, segment.P2.Content }.OrderBy(s => s));
                    Console.WriteLine($"{segmentKey}: {value.Value:F2}");
                }
            }

            Console.WriteLine("\n--- All Angle Values ---");
            DisplayValues(solver.AngleStorage.AngleValues.OrderBy(x => x.Key.ToString()));

            Console.WriteLine("\n--- All Segment Values ---");
            DisplayValues(solver.SegmentStorage.SegmentValues.OrderBy(x => x.Key.ToString()));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n💥 UNEXPECTED ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\n" + new string('=', 60));
    }

    public static void RunDetailedAnalysis(double xValue)
    {
        var solver = new Solver();
        solver.Rules = ProblemRules;

        Console.WriteLine($"=== STEP-BY-STEP ANALYSIS FOR x = {xValue}° ===");

        // Add given values
        solver.AddGivenAngleValue(Angle.FromString("DBA"), 20);
        solver.AddGivenAngleValue(Angle.FromString("CBD"), 60);
        solver.AddGivenAngleValue(Angle.FromString("ACD"), 30);
        solver.AddGivenAngleValue(Angle.FromString("DCB"), 50);
        solver.AddGivenAngleValue(Angle.FromString("FED"), xValue);
        solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

        Console.WriteLine("GIVEN VALUES:");
        Console.WriteLine($"  DBA = 20°");
        Console.WriteLine($"  CBD = 60°");
        Console.WriteLine($"  ACD = 30°");
        Console.WriteLine($"  DCB = 50°");
        Console.WriteLine($"  FED = {xValue}°");
        Console.WriteLine($"  BC = 1.0");

        // Run solver
        var result = solver.Solve();

        Console.WriteLine($"\nSOLVER RESULT: {result}");

        // Manual calculation verification
        Console.WriteLine("\n=== MANUAL CALCULATION VERIFICATION ===");

        // Triangle DBC: DBA=20°, CBD=60°, DCB=50°
        // Should give: BDC = 180° - 20° - 60° = 100°? NO!
        // Actually: DBC has angles DBC, BCD, BDC
        // We have CBD=60° and DCB=50°, so BDC = 180° - 60° - 50° = 70°
        Console.WriteLine("Triangle DBC:");
        Console.WriteLine($"  CBD = 60° (given)");
        Console.WriteLine($"  DCB = 50° (given)");
        Console.WriteLine($"  BDC = 180° - 60° - 50° = 70°");

        // From angle equality: FDE = BDC = 70°
        Console.WriteLine("\nFrom angle equality FDE = BDC:");
        Console.WriteLine($"  FDE = 70°");

        // Triangle FED: FED=x°, FDE=70°
        Console.WriteLine($"\nTriangle FED:");
        Console.WriteLine($"  FED = {xValue}° (given)");
        Console.WriteLine($"  FDE = 70°");
        Console.WriteLine($"  DFE = 180° - {xValue}° - 70° = {180 - xValue - 70}°");

        // From AnglesAddUpToRule: BDC + CDE = 180°
        // So: CDE = 180° - BDC = 180° - 70° = 110°
        Console.WriteLine("\nFrom angles add up rule BDC + CDE = 180°:");
        Console.WriteLine($"  CDE = 180° - 70° = 110°");

        // Triangle CDE: CDE=110°, ECD=30° (from ACD=ECD equality)
        Console.WriteLine("\nTriangle CDE:");
        Console.WriteLine($"  CDE = 110°");
        Console.WriteLine($"  ECD = 30° (from ACD = ECD equality)");
        Console.WriteLine($"  CED = 180° - 110° - 30° = 40°");

        // From angle containment: CEF = CED + DEF
        // We need DEF. In triangle DEF: DEF is angle at E
        // But triangle DEF is same as triangle FED, so DEF = FED = x°
        Console.WriteLine($"\nFor angle containment CEF = CED + DEF:");
        Console.WriteLine($"  CED = 40°");
        Console.WriteLine($"  DEF = FED = {xValue}°");
        Console.WriteLine($"  CEF = 40° + {xValue}° = {40 + xValue}°");
        Console.WriteLine($"  So FEC = CEF = {40 + xValue}°");

        // User expects: CFE = 90°, FCE = 30°, FEC = 60°
        Console.WriteLine($"\n=== USER'S EXPECTED VALUES ===");
        Console.WriteLine($"  CFE = 90° (from triangle FED: DFE = {180 - xValue - 70}°)");
        Console.WriteLine($"  FCE = 30° (user claims this comes from givens)");
        Console.WriteLine($"  FEC = {40 + xValue}° (our calculation above)");
        Console.WriteLine($"  Sum = 90° + 30° + {40 + xValue}° = {90 + 30 + 40 + xValue}°");

        // Check what the solver actually computed
        Console.WriteLine($"\n=== SOLVER'S ACTUAL VALUES ===");
        var cfeSolver = solver.GetConsistentAngleValue(Angle.FromString("CFE"));
        var fceSolver = solver.GetConsistentAngleValue(Angle.FromString("FCE"));
        var fecSolver = solver.GetConsistentAngleValue(Angle.FromString("FEC"));

        Console.WriteLine($"  CFE = {cfeSolver?.ToString("F2") ?? "null"}°");
        Console.WriteLine($"  FCE = {fceSolver?.ToString("F2") ?? "null"}°");
        Console.WriteLine($"  FEC = {fecSolver?.ToString("F2") ?? "null"}°");

        if (cfeSolver.HasValue && fceSolver.HasValue && fecSolver.HasValue)
        {
            var sum = cfeSolver.Value + fceSolver.Value + fecSolver.Value;
            Console.WriteLine($"  Sum = {sum:F2}°");
        }

        Console.WriteLine($"\n=== INCONSISTENCIES ===");
        foreach (var inconsistency in solver.Inconsistencies)
        {
            Console.WriteLine($"  • {inconsistency}");
        }
    }

    private static void DisplayValues<T>(IEnumerable<KeyValuePair<T, List<IValue>>> values)
    {
        if (values.Any())
        {
            foreach (var kvp in values)
            {
                Console.WriteLine($"{kvp.Key}:");
                foreach (var val in kvp.Value)
                {
                    Console.WriteLine($"  - {val}");
                }
            }
        }
        else
        {
            Console.WriteLine("No values were derived or given.");
        }
    }
}