# VibeHive

DEV422 team midterm: one Windows Forms desktop app with REST and gRPC services.
Events & Ticketing now includes registration, JWT login, organizer event creation,
and customer ticket bookings with a matching WinForms screen. Music Rental is
merged. Playlist and inventory integration remain separate work.

## Module ownership

| Area | Owner | Starting files |
| --- | --- | --- |
| Events & Ticketing, registration, login, JWT, roles | Eugene Mazur | API Auth/Users/Events/Tickets controllers; EventsTicketsControl.cs |
| Collaborative Playlist Builder | TheKeybladeofSolace | PlaylistsController.cs; PlaylistsControl.cs |
| Music Rental Management | Wilber Amaya-Maurisio | MusicAlbum.cs; MusicRental.cs; MusicStore.cs: MusicController.cs; RentalController.cs |
| Album Inventory gRPC | Eugene Mazur, Wilber Amaya-Maurisio, TheKeybladeofSolace | VibeHive.Inventory; InventoryControl.cs |


Ownership reflects the current plan, All three members must contribute and appear in the video.


## Open and run on Windows

1. Install Visual Studio 2022 with **.NET desktop development**, **ASP.NET and web development**, and the **.NET 8 SDK**.
2. Clone this repository and open **VibeHive.sln**.
3. Restore NuGet packages and build the solution.
4. For the gRPC HTTPS endpoint, run `dotnet dev-certs https --trust` once.
5. In solution properties, set multiple startup projects: **VibeHive.Api**, **VibeHive.Inventory**, and **VibeHive.WinForms** to Start.
6. Press F5. The desktop app shows four module tabs.

Alternatively, start each project in a separate terminal:

```powershell
dotnet run --project src/VibeHive.Api
dotnet run --project src/VibeHive.Inventory
dotnet run --project src/VibeHive.WinForms
```

The REST health endpoint is http://localhost:5080/health.
The gRPC service uses https://localhost:7081; its CRUD methods currently return
Unimplemented. The Events & Tickets and Music Rental screens call the REST API.

## Project layout

- `src/VibeHive.WinForms`: shared desktop shell and independent module controls.
- `src/VibeHive.Api`: REST API controller placeholders for authentication, events, tickets, and playlists.
- `src/VibeHive.Inventory`: gRPC host, album proto contract, and service placeholder.
- `src/VibeHive.Contracts`: shared REST DTO/client configuration location. Add agreed DTOs here; gRPC messages are generated from the proto.

## Working together

- Pull the latest main before beginning work.
- Create a branch for your changes, such as `eugene/events`, `key/playlists`, or `key/inventory`.
- Work in your module files, commit, push, and open a pull request into main.
- Coordinate changes to MainForm, shared contracts, and albums.proto before editing them.
- Add API clients in your module or a shared Services folder; avoid putting HTTP/gRPC calls in MainForm.
- Keep passwords, signing keys, tokens, and local databases out of Git. Use .NET user secrets for API secrets.
- Coordinate shared user IDs and authentication before integrating playlist invitations and voting.

## First implementation tasks

- Eugene: user models, registration, password hashing, login/JWT, authorization, events/tickets, and matching WinForms screens.
- TheKeybladeofSolace: playlist models and Command Pattern, REST endpoints, voting refresh, and matching screen.
- TheKeybladeofSolace: gRPC album CRUD, validation/errors, inventory client, and matching screen. ListAlbums is server streaming; add bidirectional updates if the team chooses that enhancement.
- Wilber Amaya-Maurisio: music rental module, with album and rental models, REST endpoints (`/api/music`, `/api/rentals`) with input validation, and a matching WinForms screen built as a UserControl so it can be hosted in the shared main window.
- Team: shared data contracts (such as passing the logged-in user's ID and name from login into the music rental screen), API ports, persistence, and integration. Shared responsibility over album inventory microservices.

## Build verification

A Windows GitHub Actions workflow restores and builds all four projects on pushes
and pull requests. It does not exercise desktop interactions or implemented features.

## Events & Ticketing setup

Accounts, events, and tickets are stored in memory and reset when the API restarts.
Public registration creates Customers. An Organizer can create events; signed-in
users can book tickets, view their own bookings, and cancel before the event starts.
Cancellation returns seats to availability. Booking checks prevent overselling.

Before starting the API, configure your organizer account locally:

```powershell
dotnet user-secrets set "Organizer:Email" "your-email@example.com" --project src/VibeHive.Api
dotnet user-secrets set "Organizer:Password" "YOUR_PRIVATE_PASSWORD" --project src/VibeHive.Api
```

Use a password with at least eight characters. Sign in with those credentials in
Events & Tickets to show the event creation controls. Register another account to
try the Customer flow. Dates entered in the desktop app use the computer's local
time. Tokens expire after one hour; sign out and sign in again to renew the session.

The API generates a temporary JWT signing key on startup. Optionally configure
`Jwt:Key` through user secrets with at least 32 UTF-8 bytes. Tokens are kept only in
memory by the desktop client. Login is currently local to the Events & Tickets
screen; passing the session to other modules remains team integration work.

### Event API verification

Build with `dotnet build VibeHive.sln`. Run a fresh API with organizer credentials
configured, then run `python tests/events_smoke.py`. The test process needs
`Organizer__Email` and `Organizer__Password` environment variables matching that
organizer. The script creates test accounts and an event, so use a fresh demo API.
It checks registration, login, token rejection, organizer permissions, quantity
validation, concurrent booking capacity, booking ownership, and cancellation.
WinForms compiles on Linux with Windows targeting but needs a Windows UI check.
