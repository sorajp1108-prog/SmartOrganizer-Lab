# Phase 7C ファイル同一性基盤

- WindowsではGetFileInformationByHandleEx(FileIdInfo)から128-bit File IDとVolumeSerialNumberを取得する。
- ファイルの安定キーは `FID:<VolumeSerial>:<FileId>`。同一ボリューム内のリネーム・移動後もFirstSeenAtを維持する。
- File IDが取れないファイルシステム、共有、権限不足、API失敗ではパスキーへフォールバックし、一覧全体を停止しない。
- 台帳をfirst-seen.jsonからidentity-ledger-v2.jsonへ移行し、旧パスに一致するFirstSeenAtを初回登録時に引き継ぐ。
- 台帳には現在パス、直前パス、Volume ID、File ID、ファイルシステム名、サイズ、更新日時、内容ハッシュを記録する。
- 別ボリューム関連付けは、64MB以下の安定読み取り可能な単体ファイルのみSHA-256一致とサイズ一致を必須とする。大容量ファイルを全件ハッシュして通常利用を妨げない。
- 内容ハッシュ一致は「同じ内容の関連付け」であり、物理的に同一のファイルであることを断定しない。
- 台帳は一時ファイルから原子的に置換する。

## 制約

- File IDはVolume IDとの組み合わせで使用する。
- パスフォールバックではリネーム追跡できない。
- 64MBを超える別ボリューム移動は自動関連付けせず、新規として扱う。
- USN JournalはPhase 7Cの必須要件には含めず、将来の差分監視最適化に分離する。
