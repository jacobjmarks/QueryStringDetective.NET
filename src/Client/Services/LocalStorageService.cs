using System.Text.Json;
using Microsoft.JSInterop;

namespace Client;

public sealed class LocalStorageService(IJSRuntime js)
{
    public JsonSerializerOptions SerializerOptions { get; } = new();

    private readonly IJSRuntime _js = js;

    public async Task SetItemAsync<T>(string key, T value)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value, SerializerOptions));
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var value = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _js.InvokeVoidAsync("localStorage.clear");
    }
}
