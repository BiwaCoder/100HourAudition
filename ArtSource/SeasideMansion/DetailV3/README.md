# Seaside Mansion Detail V3

Blender 4.2 の編集可能な元データは `SeasideMansion-DetailV3.blend`。V2を読み込んで追加した独立版で、元データを上書きしない。

- `Lantern-concept.png`, `Sofa-concept.png`, `Table-concept.png`: imagegenで先に作成した設計参考画像。
- `*-model.png`: Blenderで実際のメッシュをレンダリングした確認画像。参考画像そのものではない。
- `build_furnishings.py`: 六角ガラス灯具、3座面ソファ、縫製クッション、旋盤加工の丸机、壁の納まりを再生成。
- `render_furnishings.py`: スタジオ確認画像を出力。
- `mesh_budget.json`: モデルごとのLOD0三角形数。

実行例（プロジェクトルート）:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --python ArtSource/SeasideMansion/DetailV3/build_furnishings.py
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --python ArtSource/SeasideMansion/DetailV3/render_furnishings.py
```

Unityでは `MansionFurnishingSetup.Apply()` が元の家具を無効化して新モデルを配置する。一度だけ実行する初期セットアップ。追加の細部は `Mansion Detail V3` にまとめる。モデルはLOD0/LOD1、壁の部品は壁ごとに結合。旧配置と衝突用壁は保持する。変更前のシーンは `Backups/MansionDetailV3/BeforeFurnishings.unity` に保存。

## 実建築の写真参考

Villa Cavroisの写真を実際に確認し、石張りのパネル割り、開口部の見込み、床と壁の境界、細い金属枠、横方向の段差を参考にした。建物を再現するのではなく、既存の海辺の邸宅に納まりを追加した。写真の画素をゲーム素材として転用していない。

- [WikiArquitectura / Villa Cavrois](https://en.wikiarquitectura.com/building/villa-cavrois/) — 石張りと金属の開口部。確認写真: `Villa-Cavrois_020.jpg`。
- [The Gaze of a Parisienne / En perspective à la Villa Cavrois](https://thegazeofaparisienne.com/2023/06/15/en-perspective-a-la-villa-cavrois/) — 大開口、木床、壁と窓の取り合い。確認写真: `IMG_4717.jpeg`。

## 表面と光

`CoastalFurnishing.shader` は布の織り、木目、石の粒子をワールド座標ベースで描くBuilt-in Standard Surface Shader。布の細線は画面上の微分値で弱め、遠景のちらつきを抑える。Blenderのノード材質とUnityのシェーダーは別実装のため、最終判断にはUnityの確認画像を使う。

6灯の影なし近距離Point Light、乳白ガラスのHDR発光、カメラの `MansionSoftGlow` を使用。ブルーム用一時RTは縦横1/4の2枚、縦横2往復のぼかし。コンポーネント無効化時に材質、描画完了時に一時RTを解放する。既存MistFilterは保持。他シーンに自動追加しない。
