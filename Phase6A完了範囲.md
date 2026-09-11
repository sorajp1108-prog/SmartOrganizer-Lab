# Phase 6A 基礎堅牢化

1. FileSystemWatcherからの通知と非同期クエリ結果をDispatcherQueue経由でUIスレッドへ反映。
2. 問い合わせ世代番号を採用し、遅れて完了した古い結果を破棄。
3. 監視Errorをオーバーフロー相当として全件再同期し、監視を再生成。
4. 操作履歴本体が破損した場合は`.bak`を検証して復元。壊れた本体は時刻付きで隔離し、勝手に削除しない。
5. 起動時にRunning、Committing、RecoveryRequiredを検査し、元・先・partial・ハッシュ状況と推奨を表示。
6. Committing、CompletedWithSourceRemaining、RecoveryRequiredを追加。移動元削除だけが失敗した状態を通常失敗と区別。
7. 操作中だけキャンセルボタンを表示。正式公開前はキャンセル可能、コミット開始後は「確定中」として無効化。
