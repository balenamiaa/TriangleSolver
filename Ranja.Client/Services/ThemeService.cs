using Microsoft.JSInterop;

namespace Ranja.Client.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string CurrentTheme => _currentTheme;

    public event Action<string>? ThemeChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "theme");
            _currentTheme = savedTheme ?? "light";
            await ApplyThemeAsync(_currentTheme);
        }
        catch
        {
            // Fallback to light theme if localStorage is not available
            _currentTheme = "light";
            await ApplyThemeAsync(_currentTheme);
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        if (_currentTheme != theme)
        {
            _currentTheme = theme;
            await ApplyThemeAsync(theme);
            await SaveThemeToStorageAsync(theme);
            ThemeChanged?.Invoke(theme);
        }
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == "light" ? "dark" : "light";
        await SetThemeAsync(newTheme);
    }

    private async Task ApplyThemeAsync(string theme)
    {
        try
        {
            if (theme == "dark")
            {
                await _jsRuntime.InvokeVoidAsync("eval", "document.documentElement.setAttribute('data-theme', 'dark')");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval", "document.documentElement.removeAttribute('data-theme')");
            }
        }
        catch
        {
            // TODO: Handle failure.
        }
    }

    private async Task SaveThemeToStorageAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme);
        }
        catch
        {
            // TODO: Handle failure
        }
    }
}