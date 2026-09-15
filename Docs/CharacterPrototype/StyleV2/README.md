# 3スタイルの比較試作（Style V2）

シーンは既存の `Assets/Scenes/GameScene/SeasideMansionCharacterTest.unity` を更新。元のSeasideMansionとRealityShowFilmは変更なし。主人公のみ。

## 切り替え

Play → 言語の開始ボタン → 導入 → ジョイスティック。右上「主人公」またはVキーで次の6種類を循環します。

1. キューブ
2. シルエット
3. 光のワイヤー（初期表示）
4. 光の面＋線
5. ストリート・セル
6. 冒険アニメ

主人公の顔写真窓はキューブだけに表示。既存の移動・衝突判定・カメラ・シナリオは継承します。

## 美術

- 光のワイヤー: 元の立体線を使い、シアンから琥珀色の局所的な光が流れます。異なる周期と位置の波を重ね、全身が均一に点滅しないようにしています。頂点も小さく揺らぎ、既存MansionSoftGlowでHDRのにじみを表示。
- ストリート・セル: Jet Set Radioのエッジ・大胆な配色という方向性から作ったオリジナル試作。長い手足、角張った髪と肩、ヘッドホン、大きいスニーカー。ライム・濃紺・マゼンタの頂点色、段階的な陰影と輪郭線。
- 冒険アニメ: ダーククロニクルのかわいい冒険アニメという方向性から作ったオリジナル試作。低めの頭身、丸い袖とブーツ、大きな平面的な目、キャップ・スカーフ・肩掛け鞄。青緑・クリーム・茶の配色。既存作品のキャラクターやアセットを複製していません。

| モデル | 三角形数 | スキニング | 描画 |
|---|---:|---|---|
| 抽象シルエット | 232 | 7ボーン | 1 Renderer |
| ワイヤー | 1,280 | 同じ7ボーン | 1 Renderer + 既存ブルーム |
| ストリート・セル | 768 | 7ボーン | 1 Renderer / 輪郭・塗りの2パス |
| 冒険アニメ | 2,080 | 7ボーン | 1 Renderer / 輪郭・塗りの2パス |

全モデル画像テクスチャなし。テスト用途の簡易リグで、肘・膝の屈伸、接地IK、表情アニメーションは未実装。実機FPS・GPU時間の測定はしていません。参照作品そのものの画面再現ではなく、方向性を比較するための試作です。

## 調整箇所

- `Assets/Scripts/RealityShow/Cinema/Editor/ShowCharacterStyleSetup.cs`: 形状と配色の生成元。`Apply` は追加済みの場では中止。`RefreshStory` は冒険アニメの生成済みモデルとメッシュを再生成するため、手修正した後は実行しない。
- `Runtime/ShowCharacterRig.cs`: 2モデルの簡易ポーズ。
- `Runtime/ShowCharacterVisual.cs`: 6種類の切り替え。
- `Runtime/CharacterWireLight.shader`: 発光・不規則な流れ・線の揺れ。
- `Runtime/CharacterToon.shader`: 頂点色、3段階の面陰影、輪郭。
- `Assets/RealityShow/CharacterPrototype/StyleV2/LivingWire.mat`: Light intensity、Line drift、Flow speed、Base light、Traveling lightをInspectorで調整。

初版の素材とモデルは保持。更新前シーンの控え `Backups/CharacterPrototype/BeforeStyleV2.unity`。

## 検証・画像

`PlayVerification.txt`: 導入、6表示の排他的な切り替え、ボタンのレイキャスト、元のColliderと顔窓、各表示での移動と停止、新モデルの歩行ポーズ、姿勢固定でのワイヤー時間変化。
`Comparison.png`: 左から光のワイヤー／ストリート・セル／冒険アニメ。Unityでモデルを並べた比較描画。
各スタイル名のPNG: 邸宅入口の同一カメラ。
`Game*.png`: 実際のPlay画面。
`WireFlow0.png`〜`WireFlow2.png`: 同じカメラ・固定姿勢で0.8秒間隔の発光描画。画像差分も検証。

コピー内の会話システムは元と同じセーブ先を継承しています。今回も遭遇前で検証を終了し、セーブの保存・再開は操作していません。
