namespace VibeHive.WinForms.Modules;

// Owner: TheKeybladeofSolace — Album CRUD through the gRPC inventory service.
public class InventoryControl : UserControl
{
    public InventoryControl()
    {
        Controls.Add(new Label
        {
            Text = "Album Inventory — module placeholder\nOwner: TheKeybladeofSolace\nAlbum CRUD through the gRPC inventory service.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
        // TODO: Build this module's screen and connect its service client.
    }
}
