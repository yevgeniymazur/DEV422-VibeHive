// Yevgeniy Mazur
// Event API client.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeHive.Contracts;
namespace VibeHive.WinForms.Services;

public sealed class EventsClient : IDisposable
{
    private readonly HttpClient http = new() { BaseAddress = new Uri(ServiceAddresses.Api), Timeout = TimeSpan.FromSeconds(15) };
    public AuthResponse? Session { get; private set; }
    private static async Task Check(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
    public async Task Register(RegisterRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/auth/register", request);
        await Check(response);
    }
    public async Task Login(LoginRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/auth/login", request);
        await Check(response);
        Session = await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Empty login response.");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);
    }
    public void Logout() { Session = null; http.DefaultRequestHeaders.Authorization = null; }
    public async Task<EventDto[]> Events()
    {
        using var response = await http.GetAsync("api/events");
        await Check(response);
        return await response.Content.ReadFromJsonAsync<EventDto[]>() ?? [];
    }
    public async Task<TicketDto[]> Tickets()
    {
        using var response = await http.GetAsync("api/tickets");
        await Check(response);
        return await response.Content.ReadFromJsonAsync<TicketDto[]>() ?? [];
    }
    public async Task Create(CreateEventRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/events", request);
        await Check(response);
    }
    public async Task Book(BookTicketRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/tickets", request);
        await Check(response);
    }
    public async Task Cancel(Guid id)
    {
        using var response = await http.DeleteAsync($"api/tickets/{id}");
        await Check(response);
    }
    public void Dispose() => http.Dispose();
}
