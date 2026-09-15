# SeasideMansion 詳細版 V2

元版 `../SeasideMansion.blend` を保持し、`SeasideMansion-DetailV2.blend` に詳細版を制作しました。建築の間取り、床、入口、家具は元モデルを使用しています。

## 参考画像と実メッシュ

先にimagegenで4枚の参考イメージを生成し、それぞれを見ながらBlender Pythonでメッシュを作成しました。画像から自動変換したモデルではありません。

|素材|生成参考画像|実際のモデルのレンダー|
|---|---|---|
|噴水|Fountain-concept.png|Fountain-model.png|
|柱|Column-concept.png|Column-model.png|
|ヤシ|Palm-concept.png|Palm-model.png|
|植栽|Plants-concept.png|PlantBed-model.png|

参考画像は目標の形・素材感であり、完成モデルのスクリーンショットではありません。モデルはゲーム用に細部を整理したスタイライズ表現です。

- 噴水: 深さのある底池、上下の受け鉢、水面、曲面の縁、縦溝の支柱、金属帯、鉢のリブ、池のアーチ彫刻。落水と8本のアーチ状の噴流。Unity側で透明な流れと最大256粒の飛沫を追加。
- 柱: エンタシスのある細い軸、18本の縦溝、段差を丸めた柱脚と柱頭、真鍮の扇形飾り。
- ヤシ: 曲がって先細りする幹、控えめな葉痕、15本の弓形の葉軸、各25対の湾曲した小葉、実。12本は共通メッシュを使用。
- 花壇: 常緑低木、細葉と花穂の紫花、三枚の苞を持つコーラル色の花。葉・枝・花弁を個別形状で作成し、花壇単位で結合。Unityでは両面描画・弱い風揺れ。

`Detail V2 - reusable masters` コレクションに再利用元メッシュ、`Detail V2 - crafted assets` に配置、`Original details - archived` に旧装飾を保持。旧版とマスターのコレクションは通常非表示です。

## Ocean実験

Oceanモディファイアの解像度7 / 9 / 12を評価しました。三角形数は4,802 / 13,122 / 41,472。採用は9、320m四方。計測は `mesh_budget.json` に記録しています。評価時間は単発測定で、ゲームのFPSではありません。

.blendには編集可能なOceanモディファイアを残しています。Unityへは評価済みの波形をFBX出力し、細かな動きはGPUの頂点変形と水面シェーダーで付けています。BlenderのOcean計算自体をUnityで実行する方式ではありません。流体シミュレーション、海面衝突、浮力は追加していません。

近くの波メッシュと遠景の水平面は重複領域を除外し、端で高さを揃えます。海への室内用ReflectionProbeの混入を防ぎ、同じ環境反射に統一しています。岸は既存の砂地を調整し、邸宅の外に海が見える幅にしました。

## 再生成

`build_details.py` は別のBlenderプロセスで元ファイルを読み込み、詳細版.blendと `Assets/Environments/SeasideMansion/DetailV2/*.fbx` を更新します。元の.blendは上書きしません。詳細版を手作業で編集した場合は再生成前に別名で保存してください。

```
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --python ArtSource/SeasideMansion/DetailV2/build_details.py
```

`render_details.py` は4素材を個別レンダーします。Unityの初回セットアップは適用済みです。`MansionDetailSetup.Apply` を再実行しないでください。微調整と保存は `MansionWaterPolish.Apply`、シーン変更前のバックアップは `Backups/MansionDetailV2/BeforeDetails.unity`。

Unity実機画像・検証: `Docs/MansionDetailV2`。
参考仕様: https://docs.blender.org/manual/en/latest/modeling/modifiers/physics/ocean.html
