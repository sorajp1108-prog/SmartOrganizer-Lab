using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class StructureIndex
{
 readonly object gate=new();IReadOnlyList<StructureBlock> blocks=[];readonly SystemProtectionPolicy system=new();public static StructureIndex Current{get;}=new();
 public void Replace(IEnumerable<StructureBlock> v){lock(gate)blocks=v.OrderByDescending(x=>x.RootPath.Length).ToArray();}
 public StructureBlock? FindBlockForPath(string path){var p=Path.GetFullPath(path);lock(gate)return blocks.FirstOrDefault(x=>Child(p,x.RootPath));}
 public ProtectionDecision DecideOperation(string path){if(system.IsProtected(path,out var root))return new(false,ProtectionState.SystemProtected,"Windowsの重要領域は変更できません",root,null,StructureBlockKind.SystemArea);var b=FindBlockForPath(path);return b is null?new(true,ProtectionState.None,"単体ファイル",null,null,null):new(false,b.Protection,b.Kind==StructureBlockKind.UnknownBundle?"用途を確認できない一式です":"アプリまたはプロジェクトの構成内です",b.RootPath,b.Id,b.Kind);}
 static bool Child(string p,string r){r=Path.GetFullPath(r).TrimEnd(Path.DirectorySeparatorChar);return p.Equals(r,StringComparison.OrdinalIgnoreCase)||p.StartsWith(r+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);}
}
