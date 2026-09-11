# SmartOrganizer v0.26 Lab 仮想高負荷検証報告

## 結論

Linux隔離環境で実行可能な静的検査と仮想負荷試験は合格した。ただし、WinUI 3、DispatcherQueue、COM、P/Invoke、NTFS USN、MSIXはWindows固有であるため、本報告だけで実用品と断定しない。

## 実行結果

- 復元ソース: 102ファイル
- 同梱Python検査: 16スクリプト、終了コードはすべて0
- 仮想シナリオ: 合計10,000反復
- 仮想シナリオ失敗: 0
- 2,000回のコピー、ストリームハッシュ、確定、削除: 失敗0
- `.partial`残存: 0
- 2,000ファイルの重複ハッシュ: 100グループを正しく識別
- 1GiB疎ファイルのストリームSHA-256: Python追跡メモリ増分 約1.41MiB
- USN連続性判定: 2,000反復合格
- JSONジャーナル原子的置換: 2,000反復合格

## 5つの地雷の暫定判定

### 1. UIスレッドと非同期

`.Result`、`.Wait()`、`GetAwaiter().GetResult()`、`Parallel.ForEach`は製品ソースに見つからなかった。MainViewModelにはDispatcherQueue、世代番号、CancellationTokenSourceがある。

ただし、MainWindowには複数の`async void`イベントがあり、イベント内部から漏れた例外はアプリ終了につながり得る。Windows実機で例外捕捉と画面応答性を検査する必要がある。

また検索タイマーは`ViewModel.Search=...`でRefreshAsyncを起動した直後に、もう一度`await ViewModel.RefreshAsync()`を呼ぶため、検索1回につき問い合わせが二重起動する。世代番号で古い結果は捨てられるが、余計なI/Oとキャンセル競合を生む。

### 2. SafeOperationService

FileStream、IncrementalHashにはusing/await usingがあり、単純なハンドル解放忘れは確認されなかった。仮想コピー2,000回でもpartial残存はなかった。

ただし、正式ファイルへ`File.Move`した後のハッシュ検証にキャンセルトークンを渡している。コミット直前にキャンセルが競合すると、正式ファイルが作成済みなのにFailedまたはCancelledとして記録される可能性がある。コミット開始後は検証とジャーナル確定をCancellationToken.Noneで完遂する設計が必要。

catch節内の`File.Delete(stage)`自体がIOExceptionを出した場合、元の例外処理とジャーナル記録を妨げる可能性もある。削除は個別に保護すべき。

### 3. DuplicateCandidateService

巨大ファイルをFile.ReadAllBytesで一括読込する実装ではない。FileStreamとComputeHashAsyncを用いるため、指摘された直接的なメモリ無限消費は該当しない。1GiB仮想ストリーム試験も低メモリで完了した。

ただし、候補をToArrayで保持し、カタログ側も全行を複数配列へ展開するため、ファイル件数が数十万以上になると行オブジェクト由来のメモリ増加は残る。Windows/.NET実測が必要。

### 4. UsnJournalReader

非Windows、非NTFS、API失敗をSupported=falseへ変換し、ChangeTrackingCoordinatorは全件再走査へフォールバックする。0件で黙る設計ではない。

ただしP/Invoke、ボリュームハンドル、USN V2レコード、アクセス拒否、ジャーナル切り詰めはWindows NTFS実機で未検証。現時点では既定有効にすべきではない。

### 5. OperationJournalとUndo

ジャーナル書込みは一時ファイルとFile.Replaceを使い、Undoはハッシュと同名競合を再検証する。仮想原子的置換2,000回は合格した。

ただしSafeOperationService全体に操作単位または保存先単位の排他がない。同じファイルに対するExecuteAsyncとUndoAsyncの同時実行では、検査後から変更までの間に状態が変わるTOCTOU競合が残る。Windows実機の並行障害注入が必要。

## 現時点の判定

- 重複ハッシュの一括メモリ読込: 指摘は該当しない
- partialの通常経路ハンドル解放: 対策あり
- UI世代管理とDispatcherQueue: 対策あり
- 検索二重Refresh: 修正必要
- コミット後キャンセル競合: 修正必要
- catch内partial削除失敗: 修正必要
- 操作とUndoの排他不足: 修正必要
- USN実機適合性: 未判定、既定無効推奨

## この環境では検証できない項目

- WinUI 3の画面応答性と白画面
- RPC_E_WRONG_THREAD、COMException
- WindowsのDispatcherQueue実動作
- NTFS USN Journalの実読取り
- 管理者権限あり・なし
- Windows Defenderなどによる一時ロック
- MSIXインストール、起動、終了
- .NET 10 WindowsでのC#・XAML実コンパイル

以上の理由から、本版は「仮想負荷試験合格、Windows実機承認前」とする。
