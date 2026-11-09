# VibeHive

DEV422 team midterm: one Windows Forms desktop app with REST and gRPC services.
This is a starter skeleton, not a completed assignment. No login, JWT, playlist,
event, ticket, or inventory operations are implemented yet.

## Module ownership

| Area | Owner | Starting files |
| --- | --- | --- |
| Events & Ticketing, registration, login, JWT, roles | Eugene Mazur | API Auth/Users/Events/Tickets controllers; EventsTicketsControl.cs |
| Collaborative Playlist Builder | TheKeybladeofSolace | PlaylistsController.cs; PlaylistsControl.cs |
| Album Inventory gRPC | TheKeybladeofSolace | VibeHive.Inventory; InventoryControl.cs |
| Third teammate's specific contribution | To be agreed by the team | Divide one of the modules or integration work together |

Ownership reflects the current plan; the team must agree on the third member's
active contribution. All three members must contribute and appear in the video.
These are the three planned components; Music Rental is not included.

## Open and run on Windows

1. Install Visual Studio 2022 with **.NET desktop development**, **ASP.NET and web development**, and the **.NET 8 SDK**.
2. Clone this repository and open **VibeHive.sln**.
3. Restore NuGet packages and build the solution.
4. For the gRPC HTTPS endpoint, run `dotnet dev-certs https --trust` once.
5. In solution properties, set multiple startup projects: **VibeHive.Api**, **VibeHive.Inventory**, and **VibeHive.WinForms** to Start.
6. Press F5. The desktop app currently shows three placeholder tabs.

Alternatively, start each project in a separate terminal:

```powershell
dotnet run --project src/VibeHive.Api
dotnet run --project src/VibeHive.Inventory
dotnet run --project src/VibeHive.WinForms
```

The REST health endpoint is http://localhost:5080/health.
The gRPC service uses https://localhost:7081; its CRUD methods currently return
Unimplemented. WinForms does not call the services yet.

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
- Team: agree on the third teammate's work, shared data contracts, persistence, and integration.

## Build verification

A Windows GitHub Actions workflow restores and builds all four projects on pushes
and pull requests. It does not exercise desktop interactions or implemented features.
