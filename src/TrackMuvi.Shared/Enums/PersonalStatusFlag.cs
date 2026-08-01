namespace TrackMuvi.Shared.Enums;

/// <summary>
/// Estados personales del mockup (state-panel de Mi Lista / ficha de detalle).
/// No son mutuamente excluyentes: un título puede estar "Siguiendo" y "Favorita" a la vez.
/// </summary>
public enum PersonalStatusFlag
{
    Want,
    Watched,
    Favorite,
    Following,
    Pending,
    Abandoned
}
