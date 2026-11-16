# VibeHive

Updated by Yevgeniy Mazur
November 15, 2026, at 4:21 PM PST

VibeHive is our DEV422 midterm project built with C# and .NET 8. It combines Music Rental, Collaborative Playlists, and Events & Ticketing in one Windows Forms application using an ASP.NET Core REST API.

Project scope:

The team chose three of the four components and decided to leave out Album Inventory gRPC. The gRPC project, protocol definition, and service scaffold are included in the solution, but the service methods are not implemented. Its tab is commented out and hidden from the application.

Events & Ticketing and Music Rental are integrated and working. Playlists has an initial interface and API implementation, but still needs work before it can be demonstrated as a complete module.

Login and registration:

Users register and sign in through the account window at the top of the application. Registration includes User, Event Manager, and Admin roles for demonstration purposes.

The signed-in account stays active when moving between all three tabs. Events & Ticketing and Music Rental use the same login session. The Playlist tab does not sign the user out, but it still needs to be connected to the shared account.

The account’s role stays the same across tabs. Each module shows the controls and allows the actions available to that role.
- Users can book tickets, manage their own bookings, and rent or return albums.
- Event Managers can also submit event requests, respond to proposed changes, and view bookings for their events.
- Admins have management access throughout Events & Ticketing and Music Rental. Playlist permissions still need to be implemented.
Signing out clears the session and refreshes the connected modules. Passwords are hashed, and login tokens expire after one hour. Role selection during registration is included for the classroom demonstration.

Events & Ticketing:

The module is designed to scale to additional venues and events. For this proof of concept, we use five venues in the Seattle and Tacoma area. This keeps venue selection simple and makes the workflow easier to demonstrate.
Venue	Maximum tickets per session
The Crocodile	750
The Showbox	1,100
Showbox SoDo	1,800
WAMU Theater	9,000
Tacoma Dome	22,000


These are the limits configured in the application. Each session can use a lower capacity.

Event Managers submit an event name, genre, venue, and schedule. An event can have multiple sessions, each with its own date, start time, end time, and capacity.

Admins review pending requests and can approve, reject, or propose changes. If an Admin proposes different dates, times, or capacities, the requesting manager must accept those changes before the event is approved. Declining a proposal returns the request to pending review.

Multiple pending requests can use the same venue and time. The application checks for conflicts with approved events before granting final approval, including when a manager accepts a proposal.

Guests can browse approved upcoming events. Signed-in users can book 1 to 10 tickets at a time for a selected session, depending on availability. Cancelling a booking before the session starts returns those tickets to availability.

Users see their own bookings. Event Managers can view bookings for their events. Admins can view all bookings, cancel them, and adjust ticket quantities without exceeding capacity.

The schedule editor lets managers and Admins add dates, update one or all sessions, and remove sessions. Changes to time or capacity fields must be applied to the session table before submitting.

Music Rental:

Anyone can browse the album catalog. Admins can add albums with a title, artist, genre, release year, and availability setting. Albums with active rentals cannot be deleted.

Signed-in users can rent an available album under their account. The album becomes unavailable until it is returned. Users can view and return only their own active rentals.

Admins can view all active rentals, select a registered customer when renting an album, and process returns for any customer.

Music Rental uses the shared login. Switching accounts refreshes the rental list and controls for the new user.

Collaborative Playlists:

The current Playlist tab contains the initial layout, including playlist and song lists, text fields, a collaborative option, and creation and voting buttons.

The API contains initial endpoints for creating and retrieving playlists, adding songs, inviting collaborators, voting, and viewing rankings. The module still needs:
- Button actions and API connections in the desktop interface.
- Playlist and song loading.
- A working song source.
- Fixes to collaborator creation and voting logic.
- Shared login and permission integration.
- Testing of the complete workflow.
The Playlist module is included, but it is not currently finished.

Data and current limitations:

Application data is stored in memory and resets when the API restarts. Keep the API running while switching accounts during the demonstration.

Ticket bookings track general admission quantities. Payment processing and individual seat selection are not included. Changes to approved event schedules are not currently supported.

Album Inventory gRPC remains scaffolded but unimplemented, and Collaborative Playlists still needs the work listed above.
DEV422 team midterm: one Windows Forms desktop app with REST and gRPC services.
This is a starter skeleton, not a completed assignment. No login, JWT, playlist,
event, ticket, or inventory operations are implemented yet.

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
- Wilber Amaya-Maurisio: music rental module, with album and rental models, REST endpoints (`/api/music`, `/api/rentals`) with input validation, and a matching WinForms screen built as a UserControl so it can be hosted in the shared main window.
- Team: shared data contracts (such as passing the logged-in user's ID and name from login into the music rental screen), API ports, persistence, and integration. Shared responsibility over album inventory microservices.

## Build verification

A Windows GitHub Actions workflow restores and builds all four projects on pushes
and pull requests. It does not exercise desktop interactions or implemented features.
