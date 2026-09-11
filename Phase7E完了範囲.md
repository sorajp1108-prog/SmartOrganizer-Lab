# Phase 7E 差分監視基盤

- NTFSかつ通常権限でボリュームを読み取りオープンできる場合、FSCTL_QUERY_USN_JOURNALで対応可否を検査する。ジャーナルの作成・削除・拡張は行わない。
- VolumeRootごとにJournal IDとNext USNを`change-tracking-v1.json`へ原子的に保存する。
- FSCTL_READ_USN_JOURNALから前回Next USN以降のV2レコードを取得する。
- Journal ID変更、保存USNがFirstUsn/LowestValidUsnより古い、NextUsnの巻き戻りを不連続として検出し、全件再走査へ切り替える。
- 非NTFS、アクセス拒否、ジャーナル未作成、読み取り失敗ではFileSystemWatcherを継続し、30分ごとの定期再走査を行う。
- 管理者昇格は要求しない。利用できる環境だけUSNを使い、利用不能でも標準機能を止めない。
- 処理時間、変更件数、結果を`change-tracking-metrics.jsonl`へ記録する。
- 10万件の合成レコード性能試験と5種の障害シナリオ検査を同梱する。

## 実装上の境界

USN差分は変更有無と再走査要否の判定に使う。USNレコードは完全なフルパスを直接持たないため、本版では差分がある場合に現在の監視ビューを再構築する。全ボリューム走査は避けるが、監視ルート内のカタログ再評価は行う。File IDから親階層を復元する完全なパス解決キャッシュは次の最適化対象とする。
