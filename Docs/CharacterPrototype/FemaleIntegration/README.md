# 女性主人公のUnity組み込み（2026-09-12）

対象: Assets/Scenes/GameScene/SeasideMansionCharacterTest.unity

- Himari: 黒髪ロング・白い衣装・スカートの3 FBX。ワイヤー、ストリート・セル、冒険アニメ。
- Yuto: 従来の3試作モデルを配置。元アセットのファイル名は保持。
- 画面右上の「スタイル」ボタン / Vキーで両者を連動切り替え。キューブ、シルエット、光のワイヤー、光の面＋線、ストリート・セル、冒険アニメの6表示。初期値はストリート・セル。
- 女性モデルは25ボーンのLegacy Animation。Idle/Walkをクロスフェードし、速度を移動に合わせる。髪・スカートの簡易揺れはFBXクリップ内。
- 女性FBXは描画子オブジェクトのみ。既存Rigidbody、Collider、actorId、カメラターゲットは保持。
- FBX頂点座標とTransformの100倍スケールに合わせて、女性用輪郭幅とワイヤー座標倍率を調整。
- 元SeasideMansion / RealityShowFilmのシーンには変更なし。接地IK・布物理・表情・口パク・実機性能は今回未検証／未実装。

検証: PlayVerification.txt（導入、UI raycast、6表示、連動、移動停止、5非キューブ表示の歩行、Collider/肖像表示）とWireVerification.txt。
画像: StreetToon.png、StorybookToon.png、SilhouetteAndWire.png、およびGame*.png。
統合直前のコピー: Backups/CharacterPrototype/BeforeFemaleIntegration.unity。

再読み込み検証: ReloadVerification.txt。保存→シーン再オープン→Play後、3体すべて待機/歩行クリップ参照と切り替え連動がTrue。検証後にEditorの終了を検出したため、最終Console再取得は未実施。シーン保存は終了前に完了。
