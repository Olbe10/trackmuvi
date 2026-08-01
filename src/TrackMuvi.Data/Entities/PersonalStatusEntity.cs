namespace TrackMuvi.Data.Entities;

/// <summary>
/// Estado personal de un título en el dispositivo. Los 6 flags no son excluyentes
/// (una serie puede estar Following y Favorite a la vez), igual que en el mockup.
/// </summary>
public class PersonalStatusEntity
{
    public string TitleKey { get; set; } = string.Empty;
    public bool Want { get; set; }
    public bool Watched { get; set; }
    public bool Favorite { get; set; }
    public bool Following { get; set; }
    public bool Pending { get; set; }
    public bool Abandoned { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
