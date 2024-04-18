using MudBlazor;

namespace MyWebApp.Layout;

public partial class MainLayout
{
    private string _darkLightModeText = null!;
    private string _darkLightModeIcon = null!;
    private bool _isDarkMode;

    private MudThemeProvider _mudThemeProvider = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            UpdateDarkLightModeIcon();
            StateHasChanged();
        }
    }

    private void ToggleDarkLightMode()
    {
        _isDarkMode = !_isDarkMode;
        UpdateDarkLightModeIcon();
    }

    private void UpdateDarkLightModeIcon()
    {
        if (_isDarkMode)
        {
            _darkLightModeText = "Switch to Light Mode";
            _darkLightModeIcon = Icons.Material.Filled.LightMode;
        }
        else
        {
            _darkLightModeText = "Switch to Dark Mode";
            _darkLightModeIcon = Icons.Material.Filled.DarkMode;
        }
    }
}
