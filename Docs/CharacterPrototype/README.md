# 主人公の抽象3Dモデル試作

**最新: [Style V2](StyleV2/README.md)** で発光ワイヤーとストリート・セル／冒険アニメを追加。現在は6種類切り替え、初期は光のワイヤー。以下は初版の記録。

対象シーン: `Assets/Scenes/GameScene/SeasideMansionCharacterTest.unity`。
`SeasideMansion.unity` のコピーに主人公だけを追加。RealityShowFilmと元のSeasideMansionは変更していません。

## 試す

Play → 言語画面の開始 → 従来の導入 → ジョイスティックで移動、Spaceでジャンプ。
画面右上の「主人公」ボタン、またはVキーで、キューブ → シルエット → ワイヤー → 面＋線を循環。初期表示は面＋線。
Inspectorの `MansionPlayerCube / ShowCharacterVisual / Mode` もPlay中に変更できます。
人型の間は主人公の顔写真窓を隠し、キューブでは戻します。相手のキューブ・顔窓とシナリオは従来通り。

顔・髪・模様を持たない長い上着の人型。暗い単色の面と生成した細い立体線を組み合わせています。線は全三角形のエッジではなく、形を示す稜線のみ。固有キャラクターや既存ゲームのアセットは使用していません。

## 構成と編集

- `Assets/Scripts/RealityShow/Cinema/Runtime/ShowCharacterVisual.cs`: 表示切替、UI、移動方向への視覚モデルの回転、待機/歩行の簡易ポーズ。
- `Assets/Scripts/RealityShow/Cinema/Editor/ShowCharacterPrototypeSetup.cs`: Unity APIでコピーとモデルを生成。既存テストシーンの上書きは拒否。
- `Assets/RealityShow/CharacterPrototype`: 面232三角形、線1,280三角形、2素材、テクスチャなし、7ボーン。面＋線時にSkinnedMeshRenderer 2個。
- キューブのRendererだけを切替。元のRigidbody・BoxCollider・JoystickCubeMover・CutsceneActor・カメラ対象の位置や設定は保持。身長約1.82m、キューブの底に足を合わせています。
- 大きなメッシュや画像生成、追加パッケージ、外部通信は不要。

試作の制約: 手足は関節ごとの簡易振り付けで、接地IK、肘膝の屈伸、口パクは未実装。Colliderは従来のキューブのまま。輪郭線の細さは遠景で見えにくくなります。実機性能は未測定。コピー内の会話システムは元と同じセーブ先を継承しています（今回の検証は遭遇前で終了しセーブは操作していません）。

## 検証

`PlayVerification.txt`: 導入→Walking、UIレイキャスト/4表示循環、顔写真切替、ジョイスティック移動、歩行ポーズ、停止。
`WalkingGame.png`: 実Game View。
`Cube.png`, `Silhouette.png`, `Wire.png`, `SilhouetteAndWire.png`: 同位置・同カメラのUnity描画比較。
検証コード `Assets/Scripts/RealityShow/Cinema/Checks~/VerifyCharacterPrototype.cs` は言語開始後にPlay中のPipeline eval_fileから実行。遭遇前の短い横移動を行い、位置を戻します。
