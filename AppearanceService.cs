using Microsoft.UI.Xaml;
using Windows.Storage;
namespace SmartOrganizer.WinUI.Services;
public enum AppTheme { System,Light,Dark }
public enum UiDensity { Standard,Compact }
public sealed class AppearanceService
{
    private readonly SettingsService settings;
    public AppTheme Theme {get;private set;}
    public UiDensity Density {get;private set;}
    public AppearanceService(SettingsService settings)
    {
        Theme=Enum.TryParse(settings.Get("appearance.theme","System"),out AppTheme t)?t:AppTheme.System;
        Density=Enum.TryParse(settings.Get("appearance.density","Standard"),out UiDensity d)?d:UiDensity.Standard;
    }
    public void Apply(FrameworkElement root,AppTheme theme){Theme=theme;settings.Set("appearance.theme",theme.ToString());root.RequestedTheme=theme switch{AppTheme.Light=>ElementTheme.Light,AppTheme.Dark=>ElementTheme.Dark,_=>ElementTheme.Default};}
    public void SetDensity(UiDensity density){Density=density;settings.Set("appearance.density",density.ToString());}
}
