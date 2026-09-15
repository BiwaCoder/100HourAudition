# Seaside SF Reality Show — 検証記録

2026-09-12。採用画像は祈りの姿・露出を抑えた衣装・赤面しないクールな顔のバージョン。

- 既存ShowRulesChecks: 30 seeds / 30 wins / 30 distinct outcomes。記憶、合成、リスクカード、資源、周回上限、保存と乱数連続性がPASS。
- 統合Play: `PlayVerification.txt` の23項目がPASS、Ending=Victory。
- Console: ゲーム実装・シェーダー由来のエラーなし。
- 目の合成を画面比較: 半開眼と開眼のイラスト領域差分は49×18ピクセルの目の周辺のみ。身体・衣装・髪・背景は固定。
- 最後の調整は実習説明の話者表示を沙織に統一（ゲームルール変更なし）。

01: 自己紹介／02: 脱落／03: 中央の再接続／04: 祈りのシルエット／05a: 閉眼／05b: 開眼途中／05: 開眼完了／06: 観測実習／07: 候補表示／08: カード取得／09: 勝利。

統合Play確認は `Assets/Scripts/Environments/SeasideMansion/Checks~/VerifySignalPlay.cs` を新規Play中にPipeline evalで実行。位置移動は検証用にテレポートし、通常の距離判定で遭遇を起動。テストはTemp/MansionSignalCheck.jsonだけに保存。既存のユーザーセーブは更新しない。
