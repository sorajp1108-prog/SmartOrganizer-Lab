using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public static class JournalContinuityEvaluator
{
 public static DeltaRefreshDisposition Evaluate(ChangeTrackingCheckpoint? saved,UsnJournalSnapshot current,out string reason){if(saved is null){reason="初回";return DeltaRefreshDisposition.FullRescan;}if(saved.JournalId!=current.JournalId){reason="Journal ID変更";return DeltaRefreshDisposition.FullRescan;}if(saved.NextUsn<current.LowestValidUsn||saved.NextUsn<current.FirstUsn){reason="切り詰め";return DeltaRefreshDisposition.FullRescan;}if(saved.NextUsn>current.NextUsn){reason="USN巻き戻り";return DeltaRefreshDisposition.FullRescan;}if(saved.NextUsn==current.NextUsn){reason="変更なし";return DeltaRefreshDisposition.NoChanges;}reason="差分あり";return DeltaRefreshDisposition.IncrementalRefresh;}
}
