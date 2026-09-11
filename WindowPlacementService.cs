using Microsoft.UI.Windowing;
using Windows.Graphics;
namespace SmartOrganizer.WinUI.Services;
public sealed class WindowPlacementService
{
    private bool enforcingMinimum;
    private readonly SettingsService settings;
    public WindowPlacementService(SettingsService settings)=>this.settings=settings;
    public void Restore(AppWindow window)
    {
        window.Changed += (_,args) => EnforceMinimum(window,args);
        var w=Math.Max(900,settings.Get("window.width",1200));var h=Math.Max(600,settings.Get("window.height",760));
        var x=settings.Get("window.x",80);var y=settings.Get("window.y",60);
        window.MoveAndResize(new RectInt32(x,y,w,h));
        if(settings.Get("window.maximized",false) && window.Presenter is OverlappedPresenter p)p.Maximize();
    }
    private void EnforceMinimum(AppWindow window, AppWindowChangedEventArgs args)
    {
        if (enforcingMinimum || !args.DidSizeChange) return;
        var width=Math.Max(900,window.Size.Width);var height=Math.Max(600,window.Size.Height);
        if(width==window.Size.Width && height==window.Size.Height)return;
        enforcingMinimum=true;window.Resize(new SizeInt32(width,height));enforcingMinimum=false;
    }
    public void Save(AppWindow window)
    {
        if(window.Presenter is not OverlappedPresenter p || p.State==OverlappedPresenterState.Minimized)return;
        settings.Set("window.maximized",p.State==OverlappedPresenterState.Maximized);
        if(p.State!=OverlappedPresenterState.Restored)return;
        settings.Set("window.x",window.Position.X);settings.Set("window.y",window.Position.Y);settings.Set("window.width",window.Size.Width);settings.Set("window.height",window.Size.Height);
    }
}
