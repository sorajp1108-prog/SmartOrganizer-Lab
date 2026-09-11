namespace SmartOrganizer.WinUI.Models;
public sealed record ColumnWidths(double Name,double Kind,double Source,double Modified,double Size)
{
    public double Extent => Name+Kind+Source+Modified+Size+64;
}
