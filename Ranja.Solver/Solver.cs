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

public record struct InconsistencyAngleValue(Angle Angle, string Description) : IValue
{
    public override readonly string ToString() => $"{Angle} INCONSISTENCY: {Description}";
}

public record struct InconsistencySegmentValue(Segment Segment, string Description) : IValue
{
    public override readonly string ToString() => $"{Segment} INCONSISTENCY: {Description}";
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
            ? values.Select(ExtractAngleValue).Where(v => !double.IsNaN(v))
            : [];

    public IEnumerable<double> GetSegmentValues(Segment segment) =>
        SegmentStorage.SegmentValues.TryGetValue(segment, out var values)
            ? values.Select(ExtractSegmentValue).Where(v => !double.IsNaN(v))
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

    public IEnumerable<double> GetDistinctAngleValues(Angle angle)
    {
        var values = GetAngleValues(angle).ToList();
        if (values.Count == 0) return [];

        // Group by value with epsilon tolerance and return distinct values
        var distinctValues = new List<double>();
        foreach (var value in values)
        {
            if (!distinctValues.Any(existing => Math.Abs(existing - value) <= Constants.EPSILON))
            {
                distinctValues.Add(value);
            }
        }

        // If we have conflicting values, record the inconsistency
        if (distinctValues.Count > 1)
        {
            var inconsistency = $"Angle {angle} has conflicting values: {string.Join(", ", distinctValues.Select(v => $"{v:F2}°"))}";
            if (!Inconsistencies.Contains(inconsistency))
                Inconsistencies.Add(inconsistency);
        }

        return distinctValues;
    }

    public IEnumerable<double> GetDistinctSegmentValues(Segment segment)
    {
        var values = GetSegmentValues(segment).ToList();
        if (values.Count == 0) return [];

        // Group by value with epsilon tolerance and return distinct values
        var distinctValues = new List<double>();
        foreach (var value in values)
        {
            if (!distinctValues.Any(existing => Math.Abs(existing - value) <= Constants.EPSILON))
            {
                distinctValues.Add(value);
            }
        }

        // If we have conflicting values, record the inconsistency
        if (distinctValues.Count > 1)
        {
            var inconsistency = $"Segment {segment} has conflicting values: {string.Join(", ", distinctValues.Select(v => $"{v:F2}"))}";
            if (!Inconsistencies.Contains(inconsistency))
                Inconsistencies.Add(inconsistency);
        }

        return distinctValues;
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
        InconsistencyAngleValue _ => double.NaN,
        _ => throw new InvalidOperationException("Unknown angle value type")
    };

    private static double ExtractSegmentValue(IValue value) => value switch
    {
        GivenSegmentValue gsv => gsv.Value,
        ComputedSegmentValue csv => csv.Value,
        InconsistencySegmentValue _ => double.NaN,
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

        // Derivation logic: Use distinct values
        var distinctA1 = solver.GetDistinctAngleValues(angle1).ToList();
        var distinctA2 = solver.GetDistinctAngleValues(angle2).ToList();
        var distinctA3 = solver.GetDistinctAngleValues(angle3).ToList();

        if (distinctA3.Count == 0) // If angle3 is unknown
        {
            if (distinctA1.Count != 0 && distinctA2.Count != 0)
            {
                foreach (var v1 in distinctA1)
                    foreach (var v2 in distinctA2)
                        newInfo |= solver.AddAngleValue(angle3, 180.0 - v1 - v2, $"Sum of angles in {Triangle} ({angle1}={v1:F2}°, {angle2}={v2:F2}°)");
            }
        }

        if (distinctA2.Count == 0) // If angle2 is unknown
        {
            if (distinctA1.Count != 0 && distinctA3.Count != 0)
            {
                foreach (var v1 in distinctA1)
                    foreach (var v3 in distinctA3)
                        newInfo |= solver.AddAngleValue(angle2, 180.0 - v1 - v3, $"Sum of angles in {Triangle} ({angle1}={v1:F2}°, {angle3}={v3:F2}°)");
            }
        }

        if (distinctA1.Count == 0) // If angle1 is unknown
        {
            if (distinctA2.Count != 0 && distinctA3.Count != 0)
            {
                foreach (var v2 in distinctA2)
                    foreach (var v3 in distinctA3)
                        newInfo |= solver.AddAngleValue(angle1, 180.0 - v2 - v3, $"Sum of angles in {Triangle} ({angle2}={v2:F2}°, {angle3}={v3:F2}°)");
            }
        }

        // Consistency check logic: Use GetConsistentAngleValue to leverage its existing inconsistency reporting
        // This part relies on the original valA1, valA2, valA3 passed to the function which come from GetConsistentAngleValue
        if (valA1.HasValue && valA2.HasValue && valA3.HasValue)
        {
            var sum = valA1.Value + valA2.Value + valA3.Value;
            if (Math.Abs(sum - 180.0) > Constants.EPSILON)
            {
                // Check if any angles were computed via sine rule to provide better error context
                var angle1Source = GetValueSource(solver, angle1);
                var angle2Source = GetValueSource(solver, angle2);
                var angle3Source = GetValueSource(solver, angle3);

                var sineRuleInvolved = angle1Source.Contains("Sine Rule") ||
                                     angle2Source.Contains("Sine Rule") ||
                                     angle3Source.Contains("Sine Rule");

                var causeInfo = sineRuleInvolved
                    ? " (potentially due to new inconsistent angle values calculated from sine rule or sine rule floating-point precision)"
                    : "";

                var inconsistency = $"Angles in {Triangle} ({valA1.Value:F2}°, {valA2.Value:F2}°, {valA3.Value:F2}°) sum to {sum:F2}°, not 180°{causeInfo}";

                if (!solver.Inconsistencies.Contains(inconsistency))
                {
                    solver.Inconsistencies.Add(inconsistency);

                    // Store the inconsistency in the angle storage as well
                    var inconsistencyValue = new InconsistencyAngleValue(angle1, inconsistency);
                    if (!solver.AngleStorage.AngleValues.TryGetValue(angle1, out var valueList))
                    {
                        valueList = [];
                        solver.AngleStorage.AngleValues[angle1] = valueList;
                    }
                    valueList.Add(inconsistencyValue);
                }
            }
        }

        return newInfo;
    }

    private string GetValueSource(Solver solver, Angle angle)
    {
        if (solver.AngleStorage.AngleValues.TryGetValue(angle, out var values))
        {
            var lastValue = values.LastOrDefault();
            return lastValue?.ToString() ?? "Unknown";
        }
        return "Unknown";
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

        // Derivation: If sides are equal, angles are equal
        var distinctSA = solver.GetDistinctSegmentValues(sideA).ToList();
        var distinctSB = solver.GetDistinctSegmentValues(sideB).ToList();
        var distinctA = solver.GetDistinctAngleValues(angleA).ToList();
        var distinctB = solver.GetDistinctAngleValues(angleB).ToList();

        foreach (var sAVal in distinctSA)
        {
            foreach (var sBVal in distinctSB)
            {
                if (Math.Abs(sAVal - sBVal) < Constants.EPSILON)
                {
                    // Sides are equal (sAVal == sBVal)
                    if (distinctA.Count != 0 && distinctB.Count == 0)
                    {
                        foreach (var aVal in distinctA)
                            newInfo |= solver.AddAngleValue(angleB, aVal, $"Isosceles {Triangle}, {sideA}({sAVal:F2})={sideB}({sBVal:F2}) -> {angleA}({aVal:F2}°)");
                    }
                    else if (distinctA.Count == 0 && distinctB.Count != 0)
                    {
                        foreach (var bVal in distinctB)
                            newInfo |= solver.AddAngleValue(angleA, bVal, $"Isosceles {Triangle}, {sideA}({sAVal:F2})={sideB}({sBVal:F2}) -> {angleB}({bVal:F2}°)");
                    }
                }
            }
        }

        // Derivation: If angles are equal, sides are equal
        foreach (var aVal in distinctA)
        {
            foreach (var bVal in distinctB)
            {
                if (Math.Abs(aVal - bVal) < Constants.EPSILON)
                {
                    // Angles are equal (aVal == bVal)
                    if (distinctSA.Count != 0 && distinctSB.Count == 0)
                    {
                        foreach (var sAVal in distinctSA)
                            newInfo |= solver.AddSegmentValue(sideB, sAVal, $"Isosceles {Triangle}, {angleA}({aVal:F2}°)={angleB}({bVal:F2}°) -> {sideA}({sAVal:F2})");
                    }
                    else if (distinctSA.Count == 0 && distinctSB.Count != 0)
                    {
                        foreach (var sBVal in distinctSB)
                            newInfo |= solver.AddSegmentValue(sideA, sBVal, $"Isosceles {Triangle}, {angleA}({aVal:F2}°)={angleB}({bVal:F2}°) -> {sideB}({sBVal:F2})");
                    }
                }
            }
        }

        // Consistency checks are implicitly handled by GetConsistentAngleValue/GetConsistentSegmentValue
        // when the main ApplyRule method calls them, or when other rules use these values.
        // For example, if sideA and sideB are known and equal, but angleA and angleB become known and unequal,
        // other rules or the GetConsistent... methods will flag the inconsistency.

        return newInfo;
    }

    private bool TryApplySineRule(Solver solver, Angle angle1, Angle angle2, Angle angle3, Segment side1, Segment side2, Segment side3, double? valA1, double? valA2, double? valA3)
    {
        bool newInfo = false;
        static double degToRad(double deg) => deg * Math.PI / 180.0;
        static double radToDeg(double rad) => rad * 180.0 / Math.PI;

        var distinctA1 = solver.GetDistinctAngleValues(angle1).ToList();
        var distinctA2 = solver.GetDistinctAngleValues(angle2).ToList();
        var distinctA3 = solver.GetDistinctAngleValues(angle3).ToList();
        var distinctS1 = solver.GetDistinctSegmentValues(side1).ToList();
        var distinctS2 = solver.GetDistinctSegmentValues(side2).ToList();
        var distinctS3 = solver.GetDistinctSegmentValues(side3).ToList();

        var allPossibleRatios = new List<double>();
        allPossibleRatios.AddRange(GetDistinctSineRatios(distinctA1, distinctS1, degToRad));
        allPossibleRatios.AddRange(GetDistinctSineRatios(distinctA2, distinctS2, degToRad));
        allPossibleRatios.AddRange(GetDistinctSineRatios(distinctA3, distinctS3, degToRad));

        var uniqueRatios = allPossibleRatios.Distinct().ToList();

        foreach (var ratio in uniqueRatios)
        {
            if (ratio <= Constants.EPSILON) continue; // Ratio must be positive

            // Try to derive sides from angles
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle1, side1, distinctA1, [], ratio, degToRad, radToDeg, true);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle2, side2, distinctA2, [], ratio, degToRad, radToDeg, true);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle3, side3, distinctA3, [], ratio, degToRad, radToDeg, true);

            // Try to derive angles from sides
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle1, side1, [], distinctS1, ratio, degToRad, radToDeg, false);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle2, side2, [], distinctS2, ratio, degToRad, radToDeg, false);
            newInfo |= TryApplySineRuleForAngleAndSide(solver, angle3, side3, [], distinctS3, ratio, degToRad, radToDeg, false);
        }

        // Consistency check logic uses the original valA1, valS1 etc. which are from GetConsistent...
        // This is to leverage the existing inconsistency reporting in those methods.
        var currentConsistentS1 = solver.GetConsistentSegmentValue(side1);
        var currentConsistentS2 = solver.GetConsistentSegmentValue(side2);
        var currentConsistentS3 = solver.GetConsistentSegmentValue(side3);

        if (!newInfo) // Only check consistency if no new information was derived from any combination
        {
            TryCheckSineRuleConsistency(solver, angle1, angle2, angle3, side1, side2, side3, valA1, valA2, valA3, currentConsistentS1, currentConsistentS2, currentConsistentS3, degToRad);
        }

        return newInfo;
    }

    private static IEnumerable<double> GetDistinctSineRatios(IEnumerable<double> angles, IEnumerable<double> sides, Func<double, double> degToRad)
    {
        var ratios = new List<double>();
        if (!angles.Any() || !sides.Any()) return ratios;

        foreach (var angleVal in angles)
        {
            if (angleVal <= Constants.EPSILON || Math.Abs(angleVal - 180.0) <= Constants.EPSILON) continue;
            var sinAngle = Math.Sin(degToRad(angleVal));
            if (Math.Abs(sinAngle) < Constants.EPSILON) continue; // Avoid division by zero or near-zero

            foreach (var sideVal in sides)
            {
                if (sideVal <= Constants.EPSILON) continue; // Sides must be positive
                ratios.Add(sideVal / sinAngle);
            }
        }
        return ratios.Distinct();
    }

    private bool TryApplySineRuleForAngleAndSide(Solver solver, Angle targetAngle, Segment targetSide,
                                                IEnumerable<double> knownAngles, IEnumerable<double> knownSides,
                                                double ratio, Func<double, double> degToRad, Func<double, double> radToDeg,
                                                bool deriveSide)
    {
        bool newInfo = false;

        if (deriveSide) // Derive side from angle
        {
            if (!solver.GetDistinctSegmentValues(targetSide).Any()) // If target side is unknown
            {
                foreach (var angleVal in knownAngles)
                {
                    if (angleVal > Constants.EPSILON && Math.Abs(angleVal - 180.0) > Constants.EPSILON)
                    {
                        var sinAngle = Math.Sin(degToRad(angleVal));
                        // Ensure sinAngle is not too small to prevent huge segment values from small errors
                        if (Math.Abs(sinAngle) > Constants.EPSILON_SINE_RATIO)
                        {
                            var derivedSide = ratio * sinAngle;
                            if (derivedSide > Constants.EPSILON) // Derived side must be positive
                                newInfo |= solver.AddSegmentValue(targetSide, derivedSide, $"Sine Rule on {Triangle} for {targetSide} (A={angleVal:F2}°, R={ratio:F4})");
                        }
                    }
                }
            }
        }
        else // Derive angle from side
        {
            if (!solver.GetDistinctAngleValues(targetAngle).Any()) // If target angle is unknown
            {
                foreach (var sideVal in knownSides)
                {
                    if (sideVal > Constants.EPSILON && ratio > Constants.EPSILON_SINE_RATIO) // side and ratio must be positive
                    {
                        var sinA_val = sideVal / ratio;
                        if (sinA_val >= -1.0 && sinA_val <= 1.0)
                        {
                            var potentialAngleRad = Math.Asin(sinA_val);
                            var potentialAngleDeg = radToDeg(potentialAngleRad);

                            if (potentialAngleDeg > Constants.EPSILON && Math.Abs(potentialAngleDeg - 180.0) > Constants.EPSILON)
                            {
                                // Sine rule can yield two possible angles (theta and 180-theta)
                                // Add the acute angle
                                newInfo |= solver.AddAngleValue(targetAngle, potentialAngleDeg, $"Sine Rule on {Triangle} for {targetAngle} (S={sideVal:F2}, R={ratio:F4}) -> {potentialAngleDeg:F2}°");

                                // Consider adding the obtuse angle if it's valid within the triangle context
                                // (e.g. if other two angles are known and sum < potentialAngleDeg)
                                // For now, we only add the direct Asin() result. 
                                // Advanced logic could check if 180-potentialAngleDeg is also a candidate.
                                // This is often referred to as the "ambiguous case" of the sine rule.
                                // The solver might find the obtuse angle via other rules (e.g. sum of angles) if it's correct.
                            }
                        }
                    }
                }
            }
        }
        return newInfo;
    }

    private void TryCheckSineRuleConsistency(Solver solver, Angle angle1, Angle angle2, Angle angle3, Segment side1, Segment side2, Segment side3, double? valA1, double? valA2, double? valA3, double? valS1, double? valS2, double? valS3, Func<double, double> degToRad)
    {
        // Only check consistency when we have complete triangle data and angles are properly computed
        var ratios = new List<(double ratio, string desc, Angle angle, Segment side)>();

        if (valA1.HasValue && valS1.HasValue && valA1.Value > Constants.EPSILON && Math.Abs(valA1.Value - 180.0) > Constants.EPSILON)
        {
            var ratio1 = valS1.Value / Math.Sin(degToRad(valA1.Value));
            ratios.Add((ratio1, $"{side1}/sin({angle1})", angle1, side1));
        }

        if (valA2.HasValue && valS2.HasValue && valA2.Value > Constants.EPSILON && Math.Abs(valA2.Value - 180.0) > Constants.EPSILON)
        {
            var ratio2 = valS2.Value / Math.Sin(degToRad(valA2.Value));
            ratios.Add((ratio2, $"{side2}/sin({angle2})", angle2, side2));
        }

        if (valA3.HasValue && valS3.HasValue && valA3.Value > Constants.EPSILON && Math.Abs(valA3.Value - 180.0) > Constants.EPSILON)
        {
            var ratio3 = valS3.Value / Math.Sin(degToRad(valA3.Value));
            ratios.Add((ratio3, $"{side3}/sin({angle3})", angle3, side3));
        }

        // Only check if we have all three ratios (complete triangle data) and significant differences
        if (ratios.Count == 3)
        {
            for (int i = 0; i < ratios.Count - 1; i++)
            {
                for (int j = i + 1; j < ratios.Count; j++)
                {
                    var ratio1 = ratios[i].ratio;
                    var ratio2 = ratios[j].ratio;
                    var desc1 = ratios[i].desc;
                    var desc2 = ratios[j].desc;

                    // Use a more strict threshold for reporting sine rule violations to avoid false positives
                    var maxRatio = Math.Max(Math.Abs(ratio1), Math.Abs(ratio2));
                    var absoluteDiff = Math.Abs(ratio1 - ratio2);
                    var relativeDiff = maxRatio > Constants.EPSILON ? absoluteDiff / maxRatio : absoluteDiff;

                    // Only report if the difference is significant (both absolute and relative)
                    if (absoluteDiff > Constants.EPSILON_SINE_RATIO * 10 && relativeDiff > 0.01)
                    {
                        var inconsistency = $"Sine Rule violation in {Triangle}: {desc1} = {ratio1:F4} ≠ {desc2} = {ratio2:F4} (difference: {absoluteDiff:F4})";
                        if (!solver.Inconsistencies.Contains(inconsistency))
                        {
                            solver.Inconsistencies.Add(inconsistency);
                            
                            // Mark the involved angles and segments as inconsistent
                            var conflictAngle1 = ratios[i].angle;
                            var conflictAngle2 = ratios[j].angle;
                            var conflictSegment1 = ratios[i].side;
                            var conflictSegment2 = ratios[j].side;
                            
                            // Add inconsistency values to the storage
                            if (!solver.AngleStorage.AngleValues.TryGetValue(conflictAngle1, out var angleList1))
                            {
                                angleList1 = [];
                                solver.AngleStorage.AngleValues[conflictAngle1] = angleList1;
                            }
                            angleList1.Add(new InconsistencyAngleValue(conflictAngle1, inconsistency));
                            
                            if (!solver.AngleStorage.AngleValues.TryGetValue(conflictAngle2, out var angleList2))
                            {
                                angleList2 = [];
                                solver.AngleStorage.AngleValues[conflictAngle2] = angleList2;
                            }
                            angleList2.Add(new InconsistencyAngleValue(conflictAngle2, inconsistency));
                            
                            if (!solver.SegmentStorage.SegmentValues.TryGetValue(conflictSegment1, out var segmentList1))
                            {
                                segmentList1 = [];
                                solver.SegmentStorage.SegmentValues[conflictSegment1] = segmentList1;
                            }
                            segmentList1.Add(new InconsistencySegmentValue(conflictSegment1, inconsistency));
                            
                            if (!solver.SegmentStorage.SegmentValues.TryGetValue(conflictSegment2, out var segmentList2))
                            {
                                segmentList2 = [];
                                solver.SegmentStorage.SegmentValues[conflictSegment2] = segmentList2;
                            }
                            segmentList2.Add(new InconsistencySegmentValue(conflictSegment2, inconsistency));
                        }
                    }
                }
            }
        }
    }
}

public record AngleEqualityRule(Angle Angle1, Angle Angle2) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var distinctVals1 = solver.GetDistinctAngleValues(Angle1).ToList();
        var distinctVals2 = solver.GetDistinctAngleValues(Angle2).ToList();

        // Propagate from Angle1 to Angle2
        if (distinctVals1.Count != 0 && distinctVals2.Count == 0)
        {
            foreach (var val1 in distinctVals1)
            {
                newInfo |= solver.AddAngleValue(Angle2, val1, $"Angle Equality from {Angle1} ({val1:F2}°)");
            }
        }
        // Propagate from Angle2 to Angle1
        else if (distinctVals1.Count == 0 && distinctVals2.Count != 0)
        {
            foreach (var val2 in distinctVals2)
            {
                newInfo |= solver.AddAngleValue(Angle1, val2, $"Angle Equality from {Angle2} ({val2:F2}°)");
            }
        }
        // If both have values, consistency is checked by GetConsistentAngleValue when other rules use these angles
        // or when the solver checks for overall consistency based on the Inconsistencies list.
        // However, we can still add a direct check here for immediate feedback if both are known and different.
        else if (distinctVals1.Count != 0 && distinctVals2.Count != 0)
        {
            // This specific check is about the *rule itself* being violated if distinct sets don't overlap perfectly.
            // The GetConsistentAngleValue checks if *one* angle has multiple, conflicting internally derived values.
            bool oneFoundEqual = false;
            foreach (var v1 in distinctVals1)
            {
                foreach (var v2 in distinctVals2)
                {
                    if (Math.Abs(v1 - v2) < Constants.EPSILON)
                    {
                        oneFoundEqual = true;
                        break;
                    }
                }
                if (oneFoundEqual) break;
            }
            // If after checking all pairs, no equal pair is found, then the equality rule is violated given the current sets of values.
            if (!oneFoundEqual && distinctVals1.Count > 0 && distinctVals2.Count > 0) // ensure there are values to compare
            {
                var inconsistency = $"Angle Equality Conflict: {Angle1} values [{string.Join(", ", distinctVals1.Select(v => v.ToString("F2")))}] do not align with {Angle2} values [{string.Join(", ", distinctVals2.Select(v => v.ToString("F2")))}]";
                if (!solver.Inconsistencies.Contains(inconsistency))
                    solver.Inconsistencies.Add(inconsistency);
            }
        }

        return newInfo;
    }
}

public record AnglesAddUpToRule(IEnumerable<Angle> Angles, double Value) : IRule<Solver>
{
    public bool ApplyRule(Solver solver)
    {
        bool newInfo = false;
        var anglesList = Angles.ToList();
        if (anglesList.Count == 0) return false;

        var knownAngleValuesMap = new Dictionary<Angle, List<double>>();
        var unknownAngles = new List<Angle>();
        double minPossibleKnownSum = 0;
        double maxPossibleKnownSum = 0;
        bool firstSumCalculation = true;

        foreach (var angle in anglesList)
        {
            var distinctVals = solver.GetDistinctAngleValues(angle).ToList();
            if (distinctVals.Count != 0)
            {
                knownAngleValuesMap[angle] = distinctVals;
                if (firstSumCalculation)
                {
                    minPossibleKnownSum = distinctVals.Min();
                    maxPossibleKnownSum = distinctVals.Max();
                    firstSumCalculation = false;
                }
                else
                {
                    // This is a simplification. For a truly exhaustive sum, we'd need to sum all permutations.
                    // For now, we sum the mins and maxs to get a range.
                    minPossibleKnownSum += distinctVals.Min();
                    maxPossibleKnownSum += distinctVals.Max();
                }
            }
            else
            {
                unknownAngles.Add(angle);
            }
        }

        if (unknownAngles.Count == 1)
        {
            var targetAngle = unknownAngles[0];
            // We need to iterate over all combinations of known sums to derive the unknown angle
            var knownAnglesForSum = anglesList.Where(a => a != targetAngle).ToList();

            // Generate all combinations of sums from the known angles
            var sumCombinations = new List<double> { 0.0 }; // Start with a sum of 0 for the base case
            foreach (var knownAngle in knownAnglesForSum)
            {
                var newSums = new List<double>();
                var angleValues = knownAngleValuesMap[knownAngle];
                foreach (var currentSum in sumCombinations)
                {
                    foreach (var angleVal in angleValues)
                    {
                        newSums.Add(currentSum + angleVal);
                    }
                }
                sumCombinations = newSums.Distinct().ToList();
            }

            foreach (var knownSumVariant in sumCombinations)
            {
                newInfo |= solver.AddAngleValue(targetAngle, Value - knownSumVariant, $"Sum of angles ({string.Join("+", anglesList.Select(a => a.ToString()))}={Value:F2}°, known part sum {knownSumVariant:F2}°)");
            }
        }
        else if (unknownAngles.Count == 0) // All angles have at least one value
        {
            // Check consistency: Do *any* combination of known values sum up to Value?
            // This is complex. A simpler check (as before) is to use GetConsistentAngleValue for each
            // and check if *those specific* consistent values sum up. If GetConsistentAngleValue itself found internal
            // inconsistencies for any angle, that would already be logged.

            double consistentSum = 0;
            bool allConsistentKnown = true;
            foreach (var angle in anglesList)
            {
                var val = solver.GetConsistentAngleValue(angle); // This logs inconsistency if 'angle' has multiple values
                if (val.HasValue)
                {
                    consistentSum += val.Value;
                }
                else
                {
                    allConsistentKnown = false;
                    break;
                }
            }

            if (allConsistentKnown && Math.Abs(consistentSum - Value) > Constants.EPSILON)
            {
                var angleValuesStr = string.Join(", ", anglesList.Select(a => $"{a}({solver.GetConsistentAngleValue(a).Value:F2}°)"));
                var inconsistency = $"Angles ({angleValuesStr}) sum to {consistentSum:F2}°, expected {Value:F2}°";
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
        var subAnglesList = SubAngles.ToList();
        if (subAnglesList.Count == 0) return false;

        var distinctParentVals = solver.GetDistinctAngleValues(Parent).ToList();

        var knownSubAngleValuesMap = new Dictionary<Angle, List<double>>();
        var unknownSubAngles = new List<Angle>();

        foreach (var subAngle in subAnglesList)
        {
            var distinctVals = solver.GetDistinctAngleValues(subAngle).ToList();
            if (distinctVals.Count != 0)
            {
                knownSubAngleValuesMap[subAngle] = distinctVals;
            }
            else
            {
                unknownSubAngles.Add(subAngle);
            }
        }

        // Case 1: Derive Parent from sum of all SubAngles (if Parent is unknown and all SubAngles are known)
        if (distinctParentVals.Count == 0 && unknownSubAngles.Count == 0)
        {
            var subAngleSumCombinations = new List<double> { 0.0 };
            foreach (var subAngle in subAnglesList)
            {
                var newSums = new List<double>();
                var subAngleValues = knownSubAngleValuesMap[subAngle];
                foreach (var currentSum in subAngleSumCombinations)
                {
                    foreach (var subAngleVal in subAngleValues)
                    {
                        newSums.Add(currentSum + subAngleVal);
                    }
                }
                subAngleSumCombinations = newSums.Distinct().ToList();
            }
            foreach (var sumVal in subAngleSumCombinations)
            {
                newInfo |= solver.AddAngleValue(Parent, sumVal, $"Parent angle {Parent} from sum of sub-angles ({string.Join("+", subAnglesList.Select(s => s.ToString()))} = {sumVal:F2}°)");
            }
        }
        // Case 2: Derive a single unknown SubAngle (if Parent is known and all other SubAngles are known)
        else if (distinctParentVals.Count != 0 && unknownSubAngles.Count == 1)
        {
            var targetSubAngle = unknownSubAngles[0];
            var knownSubAnglesForSum = subAnglesList.Where(sa => sa != targetSubAngle).ToList();

            var otherSubAnglesSumCombinations = new List<double> { 0.0 };
            if (knownSubAnglesForSum.Count != 0)
            {
                foreach (var knownSubAngle in knownSubAnglesForSum)
                {
                    var newSums = new List<double>();
                    // Ensure the knownSubAngle is in the map (it should be if it's not the target)
                    if (knownSubAngleValuesMap.TryGetValue(knownSubAngle, out var subAngleValues))
                    {
                        foreach (var currentSum in otherSubAnglesSumCombinations)
                        {
                            foreach (var subAngleVal in subAngleValues)
                            {
                                newSums.Add(currentSum + subAngleVal);
                            }
                        }
                        otherSubAnglesSumCombinations = newSums.Distinct().ToList();
                    }
                    else
                    {
                        // This case should ideally not happen if logic is correct, means a known angle wasn't in map
                        otherSubAnglesSumCombinations.Clear(); // Invalidate sums if data is missing
                        break;
                    }
                }
            }

            if (otherSubAnglesSumCombinations.Count != 0) // Ensure we have valid sums for other sub-angles
            {
                foreach (var parentVal in distinctParentVals)
                {
                    foreach (var otherSumVal in otherSubAnglesSumCombinations)
                    {
                        newInfo |= solver.AddAngleValue(targetSubAngle, parentVal - otherSumVal, $"Sub-angle {targetSubAngle} from parent {Parent}({parentVal:F2}°) - others sum({otherSumVal:F2}°)");
                    }
                }
            }
        }
        // Case 3: Consistency Check (if Parent and all SubAngles are known)
        else if (distinctParentVals.Count != 0 && unknownSubAngles.Count == 0)
        {
            // Use GetConsistentAngleValue for simplicity in consistency check, leveraging its internal conflict reporting.
            var consistentParentVal = solver.GetConsistentAngleValue(Parent);
            if (consistentParentVal.HasValue)
            {
                double consistentSubAnglesSum = 0;
                bool allSubAnglesConsistentlyKnown = true;
                foreach (var subAngle in subAnglesList)
                {
                    var val = solver.GetConsistentAngleValue(subAngle);
                    if (val.HasValue)
                    {
                        consistentSubAnglesSum += val.Value;
                    }
                    else
                    {
                        allSubAnglesConsistentlyKnown = false;
                        break;
                    }
                }

                if (allSubAnglesConsistentlyKnown && Math.Abs(consistentParentVal.Value - consistentSubAnglesSum) > Constants.EPSILON)
                {
                    var parentStr = $"{Parent}({consistentParentVal.Value:F2}°)";
                    var subAnglesStr = string.Join(" + ", subAnglesList.Select(sa => $"{sa}({solver.GetConsistentAngleValue(sa).Value:F2}°)"));
                    var inconsistency = $"Parent angle {parentStr} != sum of sub-angles ({subAnglesStr} = {consistentSubAnglesSum:F2}°)";
                    if (!solver.Inconsistencies.Contains(inconsistency))
                        solver.Inconsistencies.Add(inconsistency);
                }
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