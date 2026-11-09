namespace VibeHive.WinForms.Modules;

// Owner: Eugene Mazur — Registration, login, JWT, roles, events, and ticket bookings.
public class EventsTicketsControl : UserControl
{
    public EventsTicketsControl()
    {
        Controls.Add(new Label
        {
            Text = "Events & Ticketing — module placeholder\nOwner: Eugene Mazur\nRegistration, login, JWT, roles, events, and ticket bookings.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
        // TODO: Build this module's screen and connect its service client.
    }
}
