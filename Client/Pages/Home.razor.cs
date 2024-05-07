using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;

namespace Client.Pages;

public sealed partial class Home : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private AppConfig AppConfig { get; set; } = null!;

    [Inject]
    private ClipboardService ClipboardService { get; set; } = null!;

    [Inject]
    private LocalStorageService LocalStorageService { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "qs")]
    [Parameter]
    public string? InitialInputValue { get; set; }

    private readonly HttpClient httpClient = new();

    private MudTextField<string> _input = null!;
    private bool loadingIndicator = false;
    private bool _showErrorAlert;

    private IEnumerable<BindingResults> bindingResults = [];

    private string showNullableTypes = "When Discrepant";
    private string ShowNullableTypes
    {
        get => showNullableTypes;
        set
        {
            showNullableTypes = value;
            LocalStorageService.SetItemAsync("qsd:cfg:nullable-types", value).AndForget();
        }
    }

    private bool showErroneous = false;
    private bool ShowErroneous
    {
        get => showErroneous;
        set
        {
            showErroneous = value;
            LocalStorageService.SetItemAsync("qsd:cfg:show-errors", value).AndForget();
        }
    }

    private bool showDetailedErrorMessages = false;
    private bool ShowDetailedErrorMessages
    {
        get => showDetailedErrorMessages;
        set
        {
            showDetailedErrorMessages = value;
            LocalStorageService.SetItemAsync("qsd:cfg:detailed-errors", value).AndForget();
        }
    }

    private void InputOnChange(string? value)
    {
        if (value == null)
            return;

        if (!loadingIndicator)
            loadingIndicator = true;

        if (_showErrorAlert)
            _showErrorAlert = false;

        RestartDebounceTimer();
    }

    private void RestartDebounceTimer()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private bool isFetchingResults = false;

    private void OnDebounceIntervalElapsed(object? sender, ElapsedEventArgs e)
    {
        if (isFetchingResults)
        {
            RestartDebounceTimer();
            return;
        }

        InvokeAsync(async () =>
        {
            if (!loadingIndicator)
            {
                loadingIndicator = true;
                StateHasChanged();
            }

            try
            {
                await GetBindingResultsAsync(_input.Value ?? "");
            }
            finally
            {
                if (!_debounceTimer.Enabled)
                    loadingIndicator = false;
                StateHasChanged();
            }
        }).AndForget();
    }

    private void InputOnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter")
        {
            if (_input.Value == null)
                _input.SetText("");
            InputOnChange(_input.Value);
        }
    }

    private System.Timers.Timer _debounceTimer = null!;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    protected override void OnInitialized()
    {
        _debounceTimer = new(_debounceInterval);
        _debounceTimer.Elapsed += OnDebounceIntervalElapsed;
        _debounceTimer.AutoReset = false;
    }

    private async Task GetBindingResultsAsync(string inputValue)
    {
        ArgumentNullException.ThrowIfNull(inputValue);

        try
        {
            isFetchingResults = true;

            var qs = QueryString.Create("qs", "?" + inputValue);
            using var response = await httpClient.GetAsync(AppConfig.AzureFunctionUrl + qs);
            response.EnsureSuccessStatusCode();
            var results = await response.Content.ReadFromJsonAsync<IEnumerable<BindingResults>>() ?? [];
            bindingResults = results.OrderBy(r => r.AllErroneous);

            NavigationManager.NavigateTo("?qs=" + Uri.EscapeDataString(_input.Value ?? ""), replace: true);
        }
        catch
        {
            bindingResults = [];
            _showErrorAlert = true;
            throw;
        }
        finally
        {
            isFetchingResults = false;
        }
    }

    private bool ResultTableFilter(BindingResults results)
    {
        if (!showErroneous && results.AllErroneous)
            return false;

        if (results.Type.EndsWith('?') && showNullableTypes != "Always")
        {
            if (showNullableTypes == "Never")
                return false;

            if (showNullableTypes == "When Discrepant")
            {
                var nonNullableResults = bindingResults.First(r => r.Type == results.Type[..^1]);
                if (nonNullableResults.AreEquivalentTo(results))
                    return false;
            }
        }

        return true;
    }

    private async Task GetSharableLink()
    {
        var shareLink = NavigationManager.BaseUri + "?qs=" + Uri.EscapeDataString(_input.Value ?? "");
        await ClipboardService.CopyToClipboardAsync(shareLink);

        Snackbar.Add(
            message: "Link copied to clipboard",
            severity: Severity.Normal,
            configure: o =>
            {
                o.DuplicatesBehavior = SnackbarDuplicatesBehavior.Allow;
                o.Icon = Icons.Material.Filled.ContentCopy;
                o.IconSize = Size.Small;
                o.ShowCloseIcon = false;
                o.ShowTransitionDuration = 150;
                o.VisibleStateDuration = 1500;
                o.HideTransitionDuration = 150;
            });
    }

    private async Task Clear()
    {
        bindingResults = [];
        loadingIndicator = false;
        _debounceTimer.Stop();
        await _input.Clear();
        await _input.FocusAsync();

        NavigationManager.NavigateTo(".", replace: true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeConfigurationAsync();

            if (InitialInputValue != null)
            {
                await _input.SetText(InitialInputValue);
                await _input.ForceUpdate();
            }
        }
    }

    private async Task InitializeConfigurationAsync()
    {
        try
        {
            var _showNullableTypes = await LocalStorageService.GetItemAsync<string?>("qsd:cfg:nullable-types");
            if (_showNullableTypes is "Always" or "When Discrepant" or "Never")
                showNullableTypes = _showNullableTypes;
        }
        catch { /* ignore */ }

        try
        {
            var _showErroneous = await LocalStorageService.GetItemAsync<bool?>("qsd:cfg:show-errors");
            if (_showErroneous.HasValue)
                showErroneous = _showErroneous.Value;
        }
        catch { /* ignore */ }

        try
        {
            var _showDetailedErrorMessages = await LocalStorageService.GetItemAsync<bool?>("qsd:cfg:detailed-errors");
            if (_showDetailedErrorMessages.HasValue)
                showDetailedErrorMessages = _showDetailedErrorMessages.Value;
        }
        catch { /* ignore */ }
    }

    private void UseDefaultConfiguration()
    {
        ShowNullableTypes = "When Discrepant";
        ShowErroneous = false;
        ShowDetailedErrorMessages = false;
    }

    private bool ConfigurationIsDefault()
    {
        return ShowNullableTypes == "When Discrepant"
            && !ShowErroneous
            && !ShowDetailedErrorMessages;
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}