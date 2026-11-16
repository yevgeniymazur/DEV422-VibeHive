# VibeHive

Updated by Yevgeniy Mazur  
November 15, 2026

## Project overview:

VibeHive is our DEV422 midterm project built with C# and .NET 8. It combines Music Rental, Collaborative Playlists, and Events & Ticketing in one Windows Forms application connected to an ASP.NET Core REST API.

The team selected three of the four project components. Album Inventory gRPC remains in the solution, but its tab is hidden and it is not part of the current demonstration.

## Team contributions:

| Contributor | Work |
| --- | --- |
| Yevgeniy Mazur | Events & Ticketing, registration, JWT authentication, shared login, role permissions, application integration, and fixes connecting the Playlist interface to its API |
| Wilber Amaya-Maurisio | Music Rental interface and REST implementation, integration testing |
| Brooks Halliday | Initial Playlist interface and API implementation, Album Inventory scaffold and development |

## Requirements:

- Windows
- Visual Studio 2022
- .NET 8 SDK
- .NET desktop development workload
- ASP.NET and web development workload

## Running the application:

1. Clone the repository and open `VibeHive.sln`.
2. Restore NuGet packages.
3. Build the solution.
4. Open the solution's startup project settings.
5. Set these projects to Start:
   - `VibeHive.Api`
   - `VibeHive.WinForms`
6. Press F5.

The REST API runs at `http://localhost:5080`.

The health endpoint is `http://localhost:5080/health`.

The Inventory service is not required for the three visible modules.

Alternatively, run these commands in separate terminals from the repository root:

```powershell
dotnet run --project src/VibeHive.Api

Registration and shared login:
Use Sign in / Register at the top of the application.

Registration supports User, Event Manager, and Admin accounts. Role selection is included for classroom demonstration purposes. A production application would restrict the creation of privileged accounts.

Events & Ticketing, Music Rental, and Playlists share the same signed-in account. Users can switch tabs without signing in again.

The account's role stays the same across tabs. Each module enables the actions permitted for that account. The API also checks permissions before allowing protected actions.

Signing out clears the shared session and account-specific information in the connected modules.

Passwords are hashed. JWT sessions expire after one hour. Sign out and sign in again if a session expires.
Roles and permissions:
Role	Events & Ticketing	Music Rental	Playlists
User	Browse approved events, book tickets, and manage personal bookings	Browse albums, rent available albums, and return personal rentals	Create playlists and access playlists they own or have been invited to
Event Manager	User features plus event requests, schedule proposal responses, and viewing bookings for their events	Rent and return albums under their own account	Same playlist permissions as a User
Admin	Review event requests, approve or reject requests, propose changes, and manage bookings	Manage albums and rentals for registered accounts	View and manage all playlists


Playlist owners and Admins can rename or delete playlists and invite collaborators. Invited collaborators can add songs and vote on collaborative playlists.

An event schedule proposal must be accepted by the account that submitted the request.
Events & Ticketing:
The module supports event requests, Admin review, schedule proposals, and ticket bookings.

We use five venues around Seattle and Tacoma to keep the interface simple for the demonstration and proof of concept. The venue catalog can be expanded later.
Venue	Application capacity limit per session
The Crocodile	750
The Showbox	1,100
Showbox SoDo	1,800
WAMU Theater	9,000
Tacoma Dome	22,000


These are the limits configured in the application. Each session can use a lower capacity.
Event requests:
Event Managers submit an event name, genre, venue, and one or more sessions. Each session has its own date, start time, end time, and capacity.

Sessions must start in the future, end after their start time, and stay within the venue's capacity. An event supports up to 31 sessions. Sessions within the same event cannot overlap.
Approval and proposed changes:
1. The manager submits a Pending request.
2. The Admin selects All event requests to review it.
3. The Admin approves, rejects, or proposes changes.
4. A proposal changes the status to Awaiting Manager Confirmation.
5. The requesting manager reviews the proposed schedule and Admin note.
6. Accepting applies the proposed schedule and approves the event.
7. Declining returns the request to Pending for another review.
Pending requests do not reserve a venue. Multiple requests can overlap, but final approval checks for conflicts with approved events at the same venue.

The application checks availability again when a manager accepts a proposal.
Session editor:
- Add dates creates a session for each date in the selected range.
- Update selected applies the entered time and capacity to the selected session.
- Update all applies the entered time and capacity to every session.
- Remove selected removes a session.
- Clear dates removes all sessions from the editor.
Changing an input field alone does not update sessions already listed. Apply the change and review the session table before submitting.
Ticket bookings:
Guests can browse approved upcoming events. Signed-in accounts, including Managers and Admins, can book tickets.
- Bookings apply to a specific session.
- Each booking allows 1 to 10 tickets, subject to availability.
- Availability is tracked separately for each session.
- Capacity checks prevent overselling.
- Users can view and cancel their own bookings.
- Cancelling restores the tickets to availability.
- Event Managers can view bookings for their events.
- Admins can view all bookings, cancel bookings, and adjust quantities.
- Bookings cannot be cancelled or adjusted after the session starts.
Booking statuses are Booked and Cancelled.
Music Rental:
Anyone can browse the album catalog.

Admins can add albums with a title, artist, genre, release year, and availability setting. Admins can delete albums that do not have active rentals.

Signed-in users select an available album and rent it under their account. Renting makes the album unavailable until it is returned.

Users see and return only their own active rentals.

Admins can view all active rentals, select a registered customer when creating a rental, and process returns for any customer.

Switching accounts refreshes the rental information and controls for the new account.
Collaborative Playlists:
The Playlist interface connects to the REST API and uses the shared login.
Creating and managing playlists:
1. Sign in and open Playlists.
2. Enter a playlist name.
3. Select Collaborative if invitations and voting are needed.
4. Click Create Playlist.
5. Select the playlist in the list.
Users see playlists they own or have been invited to. Admins can see all playlists.

To rename a selected playlist, enter the new name in the playlist name field and click Rename. Only the owner or an Admin can rename or delete it.

The signed-in account determines ownership. Users do not manually enter a creator's identity.
Adding songs:
Select a playlist, enter a song title and artist, and click Add song.

Owners, invited collaborators, and Admins can add songs. The API rejects duplicate title and artist combinations within the same playlist.
Inviting collaborators:
1. The collaborator must register an account while the same API instance is running.
2. Select a collaborative playlist.
3. Enter the collaborator's registered email.
4. Click Invite user.
Only the owner or an Admin can invite collaborators. Non-collaborative playlists do not accept invitations.
Voting and rankings:
Select a song in a collaborative playlist and click Vote for selected song.

Each account can vote once per song in each playlist. Songs appear in descending vote order, with titles used to sort ties.

Playlist data refreshes every five seconds while the tab is visible. Refresh also reloads the list manually.
Album Inventory gRPC:
The solution includes an Inventory project, protocol definition, and service code.

The team omitted this component from the current three-module demonstration. Its tab remains hidden.

Music Rental manages its own album catalog through REST. It does not use the separate Inventory service.
Suggested testing:
Keep the API running while switching accounts.
Events:
1. Register an Event Manager and submit an event request.
2. Register an Admin and open All event requests.
3. Propose a time or capacity change.
4. Sign in as the requesting manager and accept the proposal.
5. Book tickets and confirm that availability decreases.
6. Cancel the booking and confirm that availability increases.
7. Check that an overlapping request cannot receive approval at the same venue.
Music Rental:
1. As Admin, add an available album.
2. Sign in as a User and rent it.
3. Confirm that the album becomes unavailable.
4. Return it and confirm that availability is restored.
5. As Admin, verify access to customer selection and all active rentals.
Playlists:
1. Register two accounts.
2. Create a collaborative playlist under the first account.
3. Add songs and vote.
4. Try voting for the same song again and confirm that it is rejected.
5. Invite the second account by email.
6. Sign in as the second account and confirm access.
7. Add a song and vote as the collaborator.
8. Verify that the collaborator cannot rename or delete the playlist.
9. Sign in as Admin and confirm access to all playlists.
Data and limitations:
- Application data is stored in memory and resets when the API restarts.
- Accounts must be registered again after an API restart.
- JWT sessions expire after one hour.
- Registration allows role selection for classroom demonstrations.
- Ticket bookings use general admission quantities.
- Payment processing and assigned seating are not included.
- Schedule proposals apply to requests awaiting approval, not already approved events.
- Playlist refresh uses polling rather than an immediate server push.
- Album Inventory is separate from Music Rental and remains outside the current demonstration.
Project structure:
Project	Purpose
src/VibeHive.WinForms	Desktop interface, shared login, theme, and module controls
src/VibeHive.Api	REST endpoints for accounts, events, tickets, music rentals, and playlists
src/VibeHive.Contracts	Shared models, roles, venue catalog, and service addresses
src/VibeHive.Inventory	Separate gRPC inventory project, excluded from the current demonstration


Verification:
The application builds and opens on Windows. The team has manually tested the integrated application.

The GitHub Actions workflow restores and builds the solution. Build checks do not replace testing the application workflows.
