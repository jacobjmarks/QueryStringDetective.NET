using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages;

public sealed partial class Home : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private AppConfig AppConfig { get; set; } = null!;

    [Inject]
    private ClipboardService ClipboardService { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "qs")]
    [Parameter]
    public string? InitialInputValue { get; set; }

    private readonly HttpClient httpClient = new();

    private MudTextField<string> _input = null!;
    private bool isLoading = false;

    private IEnumerable<BindingResults> bindingResults = [];
    private bool showErroneous = false;
    private bool showDetailedErrorMessages = false;

    private void InputOnChange(string? value)
    {
        if (value == null)
            return;

        if (!isLoading)
        isLoading = true;
    }

    private async Task InputOnDebounce(string? value)
    {
        if (value == null)
            return;

        if (!isLoading)
            isLoading = true;

        try
        {
            await GetBindingResultsAsync(value);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task GetBindingResultsAsync(string inputValue)
    {
        if (inputValue == null) return;
        var qs = QueryString.Create("qs", "?q=" + inputValue);
        using var response = await httpClient.GetAsync(AppConfig.AzureFunctionUrl + qs);
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<IEnumerable<BindingResults>>() ?? [];
        bindingResults = results.OrderBy(r => r.Results.Count(r => r.Value.IsErroneous));
    }

    private async Task GetSharableLink()
    {
        var shareLink = NavigationManager.BaseUri + "?qs=" + Uri.EscapeDataString(_input.Value);
        await ClipboardService.CopyToClipboardAsync(shareLink);
    }

    private async Task Clear()
    {
        bindingResults = [];
        await _input.Clear();
        await _input.FocusAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && InitialInputValue != null)
        {
            await _input.SetText(InitialInputValue);
            await _input.ForceUpdate();
        }
    }

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}