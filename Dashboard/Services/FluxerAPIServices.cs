using System.Net.Http.Headers;
using System.Text.Json;
using Dashboard.Models;
using Microsoft.AspNetCore.Authentication;

public class FluxerApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FluxerApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<Guild>> GetUserGuildsAsync(string userId)
{
    var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
    if (token is null) return [];

    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fluxer.app/v1/admin/users/list-guilds");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Content = JsonContent.Create(new
    {
        user_id = userId,
        before = (string?)null,
        after = (string?)null,
        limit = 100,
        with_counts = true
    });

    var response = await _http.SendAsync(request);
    var body = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        throw new HttpRequestException($"list-guilds failed: {response.StatusCode}");

    return JsonSerializer.Deserialize<List<Guild>>(body) ?? [];
}
}