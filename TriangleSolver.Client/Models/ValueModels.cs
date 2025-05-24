namespace TriangleSolver.Client.Models;

public record ValueDetail(double Value, string Reason);

public record ValueInfo(List<double> AllValues, List<ValueDetail> AllValueDetails, bool IsInconsistent)
{
    public double DisplayValue => IsInconsistent ? double.NaN : (AllValues.FirstOrDefault());
    public string DisplayText => IsInconsistent ? "??" : $"{DisplayValue:F1}";
}

public record ValuePopupData(
    string Id,
    List<double> Values,
    List<ValueDetail> AllValueDetails,
    bool IsInconsistent,
    bool IsAngle);