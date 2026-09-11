# Phase 5 Store提出準備

## 完了

- Store経由MSIXを本番配布経路として固定
- Partner Center Identityを安全に反映するテンプレートと検証
- x64 StoreUpload候補のWindowsビルドスクリプト
- 秘密鍵をリポジトリへ保存しないCI構成
- 日本語Store掲載文案
- プライバシーポリシー草案
- サポートページ草案
- Partner Center提出チェックリスト
- 正式アイコンの必須チェック
- Store候補パッケージとログのSHA-256固定

## 外部依存で未完了

- Partner Center開発者アカウント
- 製品名予約
- Identity Name、Publisher ID、Publisher display name
- 正式アイコン
- 公開URLとなるプライバシーポリシーとサポートページ
- Windows Phase 4実行結果
- Store提出、認定、Microsoftによる再署名

## 単一起動入口

本番完成時の入口はMicrosoft Storeのインストールと、インストール後のスタートメニュー「SmartOrganizer」のみ。CMD、PowerShell、Visual Studioを利用者へ要求しない。
