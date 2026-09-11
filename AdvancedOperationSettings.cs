using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class AdvancedOperationSettings
{
 readonly SettingsService settings;public AdvancedOperationSettings(SettingsService settings)=>this.settings=settings;
 public FileOperationMode Mode{get=>Enum.TryParse(settings.Get("operations.mode","Standard"),out FileOperationMode v)?v:FileOperationMode.Standard;set=>settings.Set("operations.mode",value.ToString());}
 public ProtectedOperationConfirmation Confirmation{get=>Enum.TryParse(settings.Get("operations.protectedConfirmation","Always"),out ProtectedOperationConfirmation v)?v:ProtectedOperationConfirmation.Always;set=>settings.Set("operations.protectedConfirmation",value.ToString());}
 public HashSet<string> TrustedBlocks{get=>settings.Get("operations.trustedBlocks","").Split('|',StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);}
 public bool IsTrusted(string blockId)=>TrustedBlocks.Contains(blockId);
 public void Trust(string blockId){var ids=TrustedBlocks;if(ids.Add(blockId))settings.Set("operations.trustedBlocks",string.Join('|',ids));}
 public void ResetTrusted()=>settings.Set("operations.trustedBlocks","");
}
