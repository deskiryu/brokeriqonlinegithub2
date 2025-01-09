using Microsoft.JSInterop;
using System.Threading.Tasks;

public class CookieService
{
    public const string ACCESS_TOKEN_KEY = "accessToken";
    public const string ACCESS_EXPIRATION_KEY = "accessExpiration";
    public const string REFRESH_TOKEN_KEY = "refreshToken";

    private readonly IJSRuntime _jsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetCookieAsync(string name, string value, int days)
    {
        await _jsRuntime.InvokeVoidAsync("cookieManager.setCookie", name, value, days);
    }

    public async Task<string> GetCookieAsync(string name)
    {
        return await _jsRuntime.InvokeAsync<string>("cookieManager.getCookie", name);
    }

    public async Task DeleteCookieAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("cookieManager.deleteCookie", name);
    }
}