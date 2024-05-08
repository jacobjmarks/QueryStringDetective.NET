using Microsoft.JSInterop;

namespace Client;

public sealed class ClipboardService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;

    public async Task CopyToClipboardAsync(string content)
    {
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", content);
    }
}
