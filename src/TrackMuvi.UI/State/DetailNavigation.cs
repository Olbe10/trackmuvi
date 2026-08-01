using Microsoft.AspNetCore.Components;

namespace TrackMuvi.UI.State;

/// <summary>Equivalente a openDetail()/goBack() del mockup, usando rutas de Blazor Router.</summary>
public static class DetailNavigation
{
    public static void GoToDetail(NavigationManager nav, AppState state, string titleKey)
    {
        state.PreviousRoute = "/" + nav.ToBaseRelativePath(nav.Uri);
        nav.NavigateTo($"/title/{titleKey}");
    }

    public static void GoBack(NavigationManager nav, AppState state) =>
        nav.NavigateTo(string.IsNullOrEmpty(state.PreviousRoute) ? "/" : state.PreviousRoute);
}
