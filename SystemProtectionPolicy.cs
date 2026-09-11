namespace SmartOrganizer.WinUI.Services;
public sealed class SystemProtectionPolicy
{
 readonly string[] roots;public SystemProtectionPolicy(){roots=new[]{Environment.GetFolderPath(Environment.SpecialFolder.Windows),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"SysWOW64"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"WindowsApps")}.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(N).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();}
 public bool IsProtected(string path,out string root){var p=N(path);foreach(var r in roots)if(p.Equals(r,StringComparison.OrdinalIgnoreCase)||p.StartsWith(r+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)){root=r;return true;}root="";return false;}static string N(string p)=>Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
}
