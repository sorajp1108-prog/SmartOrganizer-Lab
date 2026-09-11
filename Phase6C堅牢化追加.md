# Phase 6C 追加堅牢化

- 再帰列挙をフォルダー単位の明示スタックへ変更し、アクセス不能箇所を個別にスキップ。
- ReparsePointを辿らず、訪問済みパスと最大64階層で循環・暴走を防止。
- FileSystemWatcher.IncludeSubdirectoriesを画面設定と同期。
- FolderPickerで選んだStorageFolderをFutureAccessListへ固定トークンで保存し、起動時に復元。
- 重複検査の簡易・全体ハッシュ各段階で、前後のサイズとLastWriteTimeUtcが同一か再検証。
- OfflineおよびFILE_ATTRIBUTE_RECALL_ON_DATA_ACCESSのファイルは列挙・重複ハッシュ対象から除外し、意図しないクラウド取得を避ける。
- 適用中のスマートビュー、検索、種類、入手元、拡張子、サブフォルダーを画面上へ表示し、すべて解除を追加。
