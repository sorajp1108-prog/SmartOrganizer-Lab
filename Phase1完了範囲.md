# Phase 1 完了範囲

1. v0.12.1配置を基準に、初期1200x760、最小想定900x600、左ナビ184、2段ヘッダー、下部ステータスへ固定。
2. ヘッダーと本文を同一ScrollViewerへ入れ、横スクロール位置を完全共有。
3. Thumbによる列幅変更、最小幅、保存、復元、リセットを実装。リサイズ時の強制再フィットは行わない。
4. System/Light/DarkとStandard/CompactをLocalSettingsへ保存し、次回起動時に復元。
5. 通常時のウィンドウ位置・サイズ、最大化状態をLocalSettingsへ保存。最小化状態は保存しない。
6. F5更新、Ctrl+F検索フォーカス、Ctrl+L場所変更入口、標準Tab移動を定義。

## Windows実機でのみ確認可能

- WinUI XAMLコンパイル
- Cursorプロパティ、ThemeResource、LocalSettingsの実動作
- 複数モニター復元時の画面内補正
- 100/125/150% DPI
