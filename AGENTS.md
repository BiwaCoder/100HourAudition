# 100HourAIGame 作業ガイド

- 会話・報告は基本的に日本語。
- セッションをまたぐ現在の状況は [引き継ぎ文書](Docs/Handoff/2026-09-12-current-work.md) を読む。これは2026-09-12時点の記録であり、現在のコード・ユーザーの最新指示を優先する。
- Unity操作は `unity-cli` スキルを確認し、CLIを `/Users/matsumurakatsuhiro/.unity/bin/unity` のフルパスで実行する。
- 接続中のEditorのシーン・Prefab・UnityアセットはPipelineのUnity APIで編集する。まず対象プロジェクトとPlay状態を確認する。
- コードは原則 `Assets/Scripts` 以下に機能別配置。既存のUI、Cinemachine、Joystick、ポストエフェクトを活用する。
- 未コミットのユーザー作業・前セッションの変更を保持する。git statusと差分を確認し、依頼されていないコミットや一括復元をしない。
- 機能別の説明は `Assets/Scripts/Environments/SeasideMansion/README.md`、`Assets/Scripts/RealityShow/README.md`、`Assets/Scripts/Cutscenes/README.md` を参照する。
