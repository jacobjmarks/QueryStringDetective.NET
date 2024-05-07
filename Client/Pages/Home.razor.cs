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
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private AppConfig AppConfig { get; set; } = null!;
    [Inject] private ClipboardService ClipboardService { get; set; } = null!;
    [Inject] private LocalStorageService LocalStorageService { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "qs")]
    public string? InitialInputValue { get; set; }

    private enum NullableTypeDisplay { Always, WhenDiscrepant, Never }
    private NullableTypeDisplay _showNullableTypes = NullableTypeDisplay.WhenDiscrepant;
    private NullableTypeDisplay ShowNullableTypes
    {
        get => _showNullableTypes;
        set
        {
            _showNullableTypes = value;
            LocalStorageService.SetItemAsync("qsd:cfg:nullable-types", value).AndForget();
        }
    }

    private bool _showErroneous = false;
    private bool ShowErroneous
    {
        get => _showErroneous;
        set
        {
            _showErroneous = value;
            LocalStorageService.SetItemAsync("qsd:cfg:show-errors", value).AndForget();
        }
    }

    private bool _showDetailedErrorMessages = false;
    private bool ShowDetailedErrorMessages
    {
        get => _showDetailedErrorMessages;
        set
        {
            _showDetailedErrorMessages = value;
            LocalStorageService.SetItemAsync("qsd:cfg:detailed-errors", value).AndForget();
        }
    }

    private bool IsConfigurationDefault
    {
        get => ShowNullableTypes == NullableTypeDisplay.WhenDiscrepant
            && !ShowErroneous
            && !ShowDetailedErrorMessages;
    }

    private readonly HttpClient _httpClient = new();
    private MudTextField<string> _input = null!;
    private IEnumerable<BindingResults> _bindingResults = [];
    private bool _loadingIndicator = false;
    private bool _showErrorAlert = false;
    private bool _isFetchingResults = false;
    private System.Timers.Timer _debounceTimer = null!;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    protected override void OnInitialized()
    {
        _debounceTimer = new(_debounceInterval);
        _debounceTimer.Elapsed += OnDebounceIntervalElapsed;
        _debounceTimer.AutoReset = false;
    }

    protected override async Task OnInitializedAsync()
    {
        await InitializeConfigurationAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            return OnFirstRenderAsync();

        return Task.CompletedTask;
    }

    private async Task InitializeConfigurationAsync()
    {
        try
        {
            var showNullableTypes = await LocalStorageService.GetItemAsync<int?>("qsd:cfg:nullable-types");
            _showNullableTypes = (NullableTypeDisplay)showNullableTypes;
        }
        catch { /* ignore */ }

        try
        {
            var showErroneous = await LocalStorageService.GetItemAsync<bool?>("qsd:cfg:show-errors");
            if (showErroneous.HasValue)
                _showErroneous = showErroneous.Value;
        }
        catch { /* ignore */ }

        try
        {
            var showDetailedErrorMessages = await LocalStorageService.GetItemAsync<bool?>("qsd:cfg:detailed-errors");
            if (showDetailedErrorMessages.HasValue)
                _showDetailedErrorMessages = showDetailedErrorMessages.Value;
        }
        catch { /* ignore */ }
    }

    private async Task OnFirstRenderAsync()
    {
        if (InitialInputValue != null)
        {
            await _input.SetText(InitialInputValue);
            await _input.ForceUpdate();
        }
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

    private void InputOnChange(string? value)
    {
        if (value == null)
            return;

        if (!_loadingIndicator)
            _loadingIndicator = true;

        if (_showErrorAlert)
            _showErrorAlert = false;

        RestartDebounceTimer();
    }

    private void RestartDebounceTimer()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void OnDebounceIntervalElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_isFetchingResults)
        {
            RestartDebounceTimer();
            return;
        }

        InvokeAsync(async () =>
        {
            if (!_loadingIndicator)
            {
                _loadingIndicator = true;
                StateHasChanged();
            }

            try
            {
                await GetBindingResultsAsync(_input.Value ?? "");
            }
            finally
            {
                if (!_debounceTimer.Enabled)
                    _loadingIndicator = false;
                StateHasChanged();
            }
        }).AndForget();
    }

    private async Task GetBindingResultsAsync(string inputValue)
    {
        ArgumentNullException.ThrowIfNull(inputValue);

        try
        {
            _isFetchingResults = true;

            var qs = QueryString.Create("qs", "?" + inputValue);
            using var response = await _httpClient.GetAsync(AppConfig.AzureFunctionUrl + qs);
            response.EnsureSuccessStatusCode();
            var results = await response.Content.ReadFromJsonAsync<IEnumerable<BindingResults>>() ?? [];
            _bindingResults = results.OrderBy(r => r.AllErroneous);

            NavigationManager.NavigateTo("?qs=" + Uri.EscapeDataString(_input.Value ?? ""), replace: true);
        }
        catch
        {
            _bindingResults = [];
            _showErrorAlert = true;
            throw;
        }
        finally
        {
            _isFetchingResults = false;
        }
    }

    private bool ResultTableFilter(BindingResults results)
    {
        if (!_showErroneous && results.AllErroneous)
            return false;

        if (results.Type.EndsWith('?') && _showNullableTypes != NullableTypeDisplay.Always)
        {
            if (_showNullableTypes == NullableTypeDisplay.Never)
                return false;

            if (_showNullableTypes == NullableTypeDisplay.WhenDiscrepant)
            {
                var nonNullableResults = _bindingResults.First(r => r.Type == results.Type[..^1]);
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
        _bindingResults = [];
        _loadingIndicator = false;
        _debounceTimer.Stop();
        await _input.Clear();
        await _input.FocusAsync();

        NavigationManager.NavigateTo(".", replace: true);
    }

    private void UseDefaultConfiguration()
    {
        ShowNullableTypes = NullableTypeDisplay.WhenDiscrepant;
        ShowErroneous = false;
        ShowDetailedErrorMessages = false;
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
