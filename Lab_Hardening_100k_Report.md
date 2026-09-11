# SmartOrganizer v0.26.1 Lab Hardening 10万回検証報告

## 結果

- 仮想反復: 100,000回
- 判定ケース: 15件
- 合格: 15件
- 失敗: 0件
- 実行時間: 105.89秒
- Python追跡最大メモリ: 13.78 MiB
- 同梱静的検査: 16スクリプトすべて合格

## 修正内容

1. 検索タイマーから重複していた2回目のRefreshAsync呼び出しを削除した。
2. 正式ファイルへのコミット開始後は公開後ハッシュ検証をCancellationToken.Noneで完遂する。
3. partial削除失敗を個別に捕捉し、本来の失敗・キャンセル記録を妨げないようにした。
4. SafeOperationServiceにSemaphoreSlimを追加し、ExecuteとUndoを直列化した。
5. コミット開始後の障害を通常FailedではなくRecoveryRequiredとして記録するようにした。

## 10万回の配分

- UI世代管理モデル: 20,000回
- コピー、ハッシュ、確定、削除: 20,000回
- 重複候補ストリームハッシュ: 20,000ファイル
- USN継続性判定: 20,000回
- ジャーナル原子的置換: 20,000回

## 追加負荷

- 64 MiB疎ファイルのストリームSHA-256を実行し、追跡メモリ増分は約0.17 MiBだった。
- 直前の検証では1 GiB疎ファイルのストリームSHA-256も合格している。
- partial残存は0件だった。
- 重複検査は1,000グループ、各20ファイルを正しく識別した。

## 制限

この検証環境にはdotnetとWindowsがない。したがって、WinUI 3、XAML実コンパイル、DispatcherQueue、RPC_E_WRONG_THREAD、COM、NTFS USN API、Windows Defenderによるロック、MSIXは未検証である。今回の結果はWindows実機承認の代わりではない。

## 判定

仮想高負荷試験と静的検査の範囲では不具合を再現しなかった。Windows実機試験へ進めるHardening候補とする。
