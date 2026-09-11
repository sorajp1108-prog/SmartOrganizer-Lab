using Microsoft.UI.Xaml;
using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class ColumnLayoutService
{
    public const double NameMin=260, KindMin=100, SourceMin=150, ModifiedMin=140, SizeMin=90;
    private readonly SettingsService settings;
    public ColumnWidths Widths { get; private set; }
    public ColumnLayoutService(SettingsService settings)
    {
        this.settings=settings;
        Widths=new(settings.Get("columns.name",420d),settings.Get("columns.kind",130d),settings.Get("columns.source",190d),settings.Get("columns.modified",170d),settings.Get("columns.size",100d));
        Normalize();
    }
    public void Set(int index,double value)
    {
        Widths=index switch {0=>Widths with{Name=Math.Max(NameMin,value)},1=>Widths with{Kind=Math.Max(KindMin,value)},2=>Widths with{Source=Math.Max(SourceMin,value)},3=>Widths with{Modified=Math.Max(ModifiedMin,value)},4=>Widths with{Size=Math.Max(SizeMin,value)},_=>Widths};
    }
    public void Reset()=>Widths=new(420,130,190,170,100);
    public void Save(){settings.Set("columns.name",Widths.Name);settings.Set("columns.kind",Widths.Kind);settings.Set("columns.source",Widths.Source);settings.Set("columns.modified",Widths.Modified);settings.Set("columns.size",Widths.Size);}
    private void Normalize()=>Widths=new(Math.Max(NameMin,Widths.Name),Math.Max(KindMin,Widths.Kind),Math.Max(SourceMin,Widths.Source),Math.Max(ModifiedMin,Widths.Modified),Math.Max(SizeMin,Widths.Size));
}
