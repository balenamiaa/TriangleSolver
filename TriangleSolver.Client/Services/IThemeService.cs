namespace TriangleSolver.Client.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    event Action<string>? ThemeChanged;
    Task SetThemeAsync(string theme);
    Task ToggleThemeAsync();
    Task InitializeAsync();
}

public enum Theme
{
    Light,
    Dark
}