# Seaside Audition Mansion — concept v1

海辺のリアリティーショー用・白いアールデコ邸宅の初期モデル。Unityの既存素材・シーンは変更していません。

## 成果物

- `concept.png`：画像生成ツールで作成した建築コンセプト。3Dレンダーではありません。
- `SeasideMansion.blend`：編集可能なBlender 4.2モデル。単位はメートル。
- `SeasideMansion.glb`：建築・庭・家具のポータブルモデル。海・照明・カメラは含みません。
- `exterior.png` / `interior.png` / `courtyard.png`：実際の3Dモデルの確認レンダー。
- `build_mansion.py`：再生成用Python。新しいBlenderプロセスで実行してください。開いているシーン内のオブジェクトを消して構築するため、作業中のファイル上では実行しないでください。

## 構成

外周40×38m、中央庭園20×18m。北（Blender +Y）が海側です。
南入口のレッドカーペット→エントランス→噴水を回り込む庭園通路→北の海側ラウンジ。
左右に約3m幅の回遊廊下、幅2.4mの開いた出入口、各3室、海側ダイニングがあります。
寝室・会話用の部屋・ラウンジに家具を配置。出入口に扉板は置いていません。

`Roof - hide to edit interiors` コレクションを非表示にすると内装を編集しやすくなります。
`Camera_Exterior` / `Camera_Interior` / `Camera_Courtyard` を切り替えて確認できます。
BlenderのShift+バッククォートのWalk Navigationで室内を確認できます。
Unityで遊ぶためのコライダー、プレイヤー、NavMesh、LODはまだ含まない制作モデルです。
コンセプトの細部を完全再現した最終アセットではなく、構造・配色・動線の初期モデルです。

## 再生成

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python ArtSource/SeasideMansion/build_mansion.py
```

## Blender MCP

ahujasid/blender-mcpのアドオンをBlender 4.2にインストールし、ユーザー設定で有効化。
Codex設定に `blender` STDIOサーバーを追加済み。uvxは `/opt/homebrew/bin/uvx`。
ローカル接続は127.0.0.1:9876。テレメトリーは無効化しました。
Blender再起動後はNパネルのMCP for Blenderからサーバーを開始できます。
Codexで新しいMCPツール登録を読み込むにはアプリ再起動が必要な場合があります。

導入元: https://github.com/ahujasid/blender-mcp

## イラストの生成指示

Built-in image generation toolを使用。白い平屋のアールデコ海辺邸宅、四方の翼棟と回遊廊下、中央のカラフルな庭園と段式噴水、開いた出入口、南入口の赤いカーペット、北ラウンジの大きな海向き窓、夕焼け、シャンパンゴールドの幾何学装飾。入口側上空から庭園全体を見渡し、右下に室内イメージ。文字・人物なし。
