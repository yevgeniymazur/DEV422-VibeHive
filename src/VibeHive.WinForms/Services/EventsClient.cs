// Yevgeniy Mazur
// Event API client.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeHive.Contracts;

namespace VibeHive.WinForms.Services;

public sealed class EventsClient : IDisposable
{
    private readonly HttpClient http = new()
    {
        BaseAddress = new Uri(ServiceAddresses.Api),
        Timeout = TimeSpan.FromSeconds(15)
    };

    public AuthResponse? Session { get; private set; }

    private static async Task Check(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = await response.Content.ReadAsStringAsync();

        throw new InvalidOperationException(
            $"{(int)response.StatusCode}: {message}");
    }

    private async Task<T> Get<T>(string path)
    {
        using var response = await http.GetAsync(path);

        await Check(response);

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException(
                "The API returned an empty response.");
    }

    private async Task Send(
        HttpMethod method,
        string path,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request);

        await Check(response);
    }

    public Task Register(RegisterRequest request)
    {
        return Send(
            HttpMethod.Post,
            "api/users/register",
            request);
    }

    public async Task Login(LoginRequest request)
    {
        using var response = await http.PostAsJsonAsync(
            "api/auth/login",
            request);

        await Check(response);

        Session = await response.Content
            .ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException(
                "The API returned an empty login response.");

        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                Session.Token);
    }

    public void Logout()
    {
        Session = null;
        http.DefaultRequestHeaders.Authorization = null;
    }

    public Task<VenueDto[]> Venues()
    {
        return Get<VenueDto[]>("api/events/venues");
    }

    public Task<EventDto[]> Events()
    {
        return Get<EventDto[]>("api/events");
    }

    public Task<EventDto[]> MyEvents()
    {
        return Get<EventDto[]>("api/events/mine");
    }

    public Task<EventDto[]> ReviewEvents()
    {
        return Get<EventDto[]>("api/events/review");
    }

    public Task Create(CreateEventRequest request)
    {
        return Send(
            HttpMethod.Post,
            "api/events",
            request);
    }

    public Task Approve(Guid eventId)
    {
        return Send(
            HttpMethod.Post,
            $"api/events/{eventId}/approve");
    }

    public Task Reject(Guid eventId)
    {
        return Send(
            HttpMethod.Post,
            $"api/events/{eventId}/reject");
    }

    public Task Propose(
        Guid eventId,
        ReviseEventRequest request)
    {
        return Send(
            HttpMethod.Put,
            $"api/events/{eventId}/proposal",
            request);
    }

    public Task AcceptProposal(Guid eventId)
    {
        return Send(
            HttpMethod.Post,
            $"api/events/{eventId}/proposal/accept");
    }

    public Task DeclineProposal(Guid eventId)
    {
        return Send(
            HttpMethod.Post,
            $"api/events/{eventId}/proposal/decline");
    }

    public Task<TicketDto[]> Tickets()
    {
        return Get<TicketDto[]>("api/tickets");
    }

    public Task<TicketDto[]> UserTickets(Guid userId)
    {
        return Get<TicketDto[]>($"api/tickets/{userId}");
    }

    public Task<TicketDto[]> AllTickets()
    {
        return Get<TicketDto[]>("api/tickets/all");
    }

    public Task<TicketDto[]> EventTickets(Guid eventId)
    {
        return Get<TicketDto[]>($"api/tickets/event/{eventId}");
    }

    public Task Book(BookTicketRequest request)
    {
        return Send(
            HttpMethod.Post,
            "api/tickets",
            request);
    }

    public Task Cancel(Guid ticketId)
    {
        return Send(
            HttpMethod.Delete,
            $"api/tickets/{ticketId}");
    }

    public Task Adjust(
        Guid ticketId,
        AdjustTicketRequest request)
    {
        return Send(
            HttpMethod.Put,
            $"api/tickets/{ticketId}",
            request);
    }

    public void Dispose()
    {
        http.Dispose();
    }
}
