VibeHive:

Updated by Yevgeniy Mazur
November 15, 2026

Project overview:

- VibeHive is our DEV422 midterm project built with C# and .NET 8.
- One Windows Forms application brings together Music Rental, Collaborative Playlists, and Events & Ticketing.
- The desktop application connects to an ASP.NET Core REST API.
- The team selected three of the four components.
- Album Inventory gRPC remains in the solution, but its tab is hidden and it is not part of the current demonstration.

Team contributions:

- Yevgeniy Mazur: Events & Ticketing, registration, JWT authentication, shared login, role permissions, application integration, and fixes connecting the Playlist interface to its API.
- Wilber Amaya-Maurisio: Music Rental interface and REST implementation, plus integration testing.
- Brooks Halliday: Initial Playlist interface and API implementation, plus Album Inventory scaffold and development.

Requirements:

- Windows.
- Visual Studio 2022.
- .NET 8 SDK.
- .NET desktop development workload.
- ASP.NET and web development workload.

Running the application:

- Clone the repository and open VibeHive.sln.
- Restore NuGet packages.
- Build the solution.
- Open the solution's startup project settings.
- Set VibeHive.Api and VibeHive.WinForms to Start.
- Press F5.
- The REST API runs at http://localhost:5080.
- The health endpoint is http://localhost:5080/health.
- The Inventory service is not required for the three visible modules.

Registration and shared login:

- Use Sign in / Register at the top of the application.
- Registration supports User, Event Manager, and Admin accounts.
- Role selection is included for classroom demonstrations. A production application would restrict privileged account creation.
- Events & Ticketing, Music Rental, and Playlists share the same signed-in account.
- Users can switch tabs without signing in again.
- The account's role stays the same across tabs. Each module enables the actions permitted for that account.
- The API also checks permissions before allowing protected actions.
- Signing out clears the shared session and account-specific information in the connected modules.
- Passwords are hashed.
- JWT sessions expire after one hour. Sign out and sign in again if a session expires.

User permissions:

- Browse approved upcoming events.
- Book tickets and view or cancel personal bookings.
- Browse albums, rent available albums, and return personal rentals.
- Create playlists and access playlists they own or have been invited to.
- Add songs to accessible playlists.
- Vote on songs in accessible collaborative playlists.
- Rename or delete their own playlists.
- Invite registered users to their own collaborative playlists.

Event Manager permissions:

- All regular User features.
- Submit event requests with one or more sessions.
- Review and accept or decline proposed schedule changes.
- View bookings for their events.
- Rent and return albums under their own account.
- Use Playlists with the same permissions as a regular User.

Admin permissions:

- Review all event requests.
- Approve or reject requests.
- Propose changes to dates, times, and capacities.
- View all ticket bookings, cancel bookings, and adjust quantities.
- Add and delete albums, subject to active rental restrictions.
- View all active rentals and manage rentals for registered customers.
- View and manage all playlists.
- Add songs, invite collaborators, and vote on collaborative playlists.
- Event schedule proposals must still be accepted by the account that submitted the request.

Events & Ticketing:

- Supports event requests, Admin review, schedule proposals, and ticket bookings.
- Uses five venues around Seattle and Tacoma to keep the interface simple for the demonstration and proof of concept.
- The venue catalog can be expanded later.
- Each session has its own schedule and ticket capacity.

Venues:

- The Crocodile: 750 tickets per session.
- The Showbox: 1,100 tickets per session.
- Showbox SoDo: 1,800 tickets per session.
- WAMU Theater: 9,000 tickets per session.
- Tacoma Dome: 22,000 tickets per session.
- These are the limits configured in the application. Each session can use a lower capacity.

Event requests:

- Event Managers enter an event name, genre, venue, and one or more sessions.
- Each session has its own date, start time, end time, and capacity.
- Sessions must start in the future.
- End times must be after start times.
- Capacity must stay within the selected venue's limit.
- An event supports up to 31 sessions.
- Sessions within the same event cannot overlap.

Approval and proposed changes:

- The manager submits a request with Pending status.
- The Admin selects All event requests to review it.
- The Admin can approve, reject, or propose changes.
- A proposal changes the status to Awaiting Manager Confirmation.
- The requesting manager reviews the proposed schedule and Admin note.
- Accepting applies the proposed schedule and approves the event.
- Declining returns the request to Pending for another review.
- Pending requests do not reserve a venue.
- Multiple pending requests can overlap.
- Final approval checks for conflicts with approved events at the same venue.
- Availability is checked again when a manager accepts a proposal.

Session editor:

- Add dates creates a session for each date in the selected range.
- Update selected applies the entered time and capacity to the selected session.
- Update all applies the entered time and capacity to every session.
- Remove selected removes a session.
- Clear dates removes all sessions from the editor.
- Changing an input field alone does not update sessions already listed.
- Apply the change and review the session table before submitting.

Ticket bookings:

- Guests can browse approved upcoming events.
- Signed-in accounts, including Managers and Admins, can book tickets.
- Bookings apply to a specific session.
- Each booking allows 1 to 10 tickets, subject to availability.
- Availability is tracked separately for each session.
- Capacity checks prevent overselling.
- Users can view and cancel their own bookings.
- Cancelling restores the tickets to availability.
- Event Managers can view bookings for their events.
- Admins can view all bookings, cancel bookings, and adjust quantities.
- Bookings cannot be cancelled or adjusted after the session starts.
- Booking statuses are Booked and Cancelled.

Music Rental:

- Anyone can browse the album catalog.
- Admins can add albums with a title, artist, genre, release year, and availability setting.
- Admins can delete albums that do not have active rentals.
- Signed-in users select an available album and rent it under their account.
- Renting makes the album unavailable until it is returned.
- Users see and return only their own active rentals.
- Admins can view all active rentals.
- Admins can select a registered customer when creating a rental.
- Admins can process returns for any customer.
- Switching accounts refreshes the rental information and controls for the new account.

Collaborative Playlists:

- The Playlist interface connects to the REST API and uses the shared login.
- Users see playlists they own or have been invited to.
- Admins can see all playlists.
- The signed-in account determines ownership.
- Users do not manually enter a creator's identity.

Creating and managing playlists:

- Sign in and open Playlists.
- Enter a playlist name.
- Select Collaborative if invitations and voting are needed.
- Click Create Playlist.
- Select the playlist in the list.
- To rename it, enter the new name in the playlist name field and click Rename.
- Only the owner or an Admin can rename or delete a playlist.

Adding songs:

- Select a playlist.
- Enter a song title and artist.
- Click Add song.
- Owners, invited collaborators, and Admins can add songs.
- The API rejects duplicate title and artist combinations within the same playlist.

Inviting collaborators:

- The collaborator must register an account while the same API instance is running.
- Select a collaborative playlist.
- Enter the collaborator's registered email.
- Click Invite user.
- Only the owner or an Admin can invite collaborators.
- Non-collaborative playlists do not accept invitations.

Voting and rankings:

- Select a song in a collaborative playlist.
- Click Vote for selected song.
- Each account can vote once per song in each playlist.
- Songs appear in descending vote order.
- Song titles are used to sort ties.
- Playlist data refreshes every five seconds while the tab is visible.
- Refresh also reloads the list manually.

Album Inventory gRPC:

- The solution includes an Inventory project, protocol definition, and service code.
- The team omitted this component from the current three-module demonstration.
- Its tab remains hidden.
- Music Rental manages its own album catalog through REST.
- Music Rental does not use the separate Inventory service.

Testing Events & Ticketing:

- Keep the API running while switching accounts.
- Register an Event Manager and submit an event request.
- Register an Admin and open All event requests.
- Propose a time or capacity change.
- Sign in as the requesting manager and accept the proposal.
- Book tickets and confirm that availability decreases.
- Cancel the booking and confirm that availability increases.
- Check that an overlapping request cannot receive approval at the same venue.

Testing Music Rental:

- As Admin, add an available album.
- Sign in as a User and rent it.
- Confirm that the album becomes unavailable.
- Return it and confirm that availability is restored.
- As Admin, verify access to customer selection and all active rentals.

Testing Playlists:

- Register two accounts.
- Create a collaborative playlist under the first account.
- Add songs and vote.
- Try voting for the same song again and confirm that it is rejected.
- Invite the second account by email.
- Sign in as the second account and confirm access.
- Add a song and vote as the collaborator.
- Verify that the collaborator cannot rename or delete the playlist.
- Sign in as Admin and confirm access to all playlists.

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

- src/VibeHive.WinForms: Desktop interface, shared login, theme, and module controls.
- src/VibeHive.Api: REST endpoints for accounts, events, tickets, music rentals, and playlists.
- src/VibeHive.Contracts: Shared models, roles, venue catalog, and service addresses.
- src/VibeHive.Inventory: Separate gRPC inventory project, excluded from the current demonstration.

Verification:

- The application builds and opens on Windows.
- The team has manually tested the integrated application.
- The GitHub Actions workflow restores and builds the solution.
- Build checks do not replace testing the application workflows.
- The existing tests/events_smoke.py targets the earlier Organizer-based implementation and does not validate the current role and approval workflow.
