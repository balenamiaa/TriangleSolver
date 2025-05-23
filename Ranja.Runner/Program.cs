using Ranja.Solver;
using System.Diagnostics;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Running Test Case: x = 20 ---");
        RunTest(20.0);

        Console.WriteLine("\n--- Running Test Case: x = 30 ---");
        RunTest(30.0);

        Console.WriteLine("\n--- Running Test Case: x = 36.4 ---");
        RunTest(36.4);
    }

    public static void RunTest(double xValue)
    {
        var solver = new Solver();
        solver.Rules = Problem1Rules.Rules;

        Console.WriteLine($"Initial values for x = {xValue}°:");

        try
        {
            // Given angles from the diagram
            // Point B related angles for triangle ABC
            solver.AddGivenAngleValue(Angle.FromString("DBA"), 20); // ∠ABD = 20° (labeled as FBD on image, F on AB)
            solver.AddGivenAngleValue(Angle.FromString("CBD"), 60); // ∠DBC = 60°

            // Point C related angles for triangle ABC
            solver.AddGivenAngleValue(Angle.FromString("ACD"), 30); // ∠ACD = 30° (labeled as ECD on image, E on AC)
            solver.AddGivenAngleValue(Angle.FromString("DCB"), 50); // ∠DCB = 50°

            // The angle x
            solver.AddGivenAngleValue(Angle.FromString("FED"), xValue); // ∠FED = x

            // To make the geometry solvable for segments, we need one reference length.
            solver.AddGivenSegmentValue(Segment.FromString("BC"), 1.0);

            Console.WriteLine("\nGiven values added. Starting solver...");

            // Enable debug output to see what's happening
            var originalLevel = Trace.Listeners.Count;
            Trace.Listeners.Add(new ConsoleTraceListener());

            var result = solver.Solve();

            // Remove the trace listener
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