# Scripts — 機能別のコード配置

自作C#はこのフォルダへ配置します。Prefab、画像、Material、Shader、フォントは素材側（Assets/UI、Assets/JoystickCubeなど）に置きます。外部アセットのJoystick Packは更新しやすいよう元の構成を維持します。

```text
Scripts/
├── UI/
│   ├── Common/             共通のガラス・丸角描画
│   ├── ChoiceMenu/         選択肢・選択枠
│   ├── AuditionHeader/     タイトル・説明・時計
│   ├── ParticipantCards/   登場人物・脱落表示
│   ├── Timeline/           投稿・ページ切り替え
│   └── SceneWindow/        映像ウィンドウの生成
├── AIChat/                 PythonAPIチャット連携
└── Gameplay/
    └── Movement/           ジョイスティック移動
```

各機能には必要なものだけ作成します。
- Runtime：ゲーム中の処理。Model/View/Controllerも機能の中にまとめる。
- Editor：Unity Editor専用のBuilderなど。フォルダ名Editorを維持する。
- Demo：デモ用の実行コード。
- Checks~：Pipeline eval_file用の検証・構築コード。通常のC#ファイルではないため、Unityがインポートしない末尾の「~」を維持する。

既存の名前空間・クラス名とスクリプトGUIDは維持しています。素材生成先は従来のAssets/UIなどです。UIの確認用シーンはAssets/Scenes/Library、移動デモはAssets/Scenes/JoystickCubeDemo.unityにあります。

例：
```sh
/Users/matsumurakatsuhiro/.unity/bin/unity command eval_file --project-path . --file Assets/Scripts/UI/Timeline/Checks~/PaginationCheck.cs --format json
```

各UIの詳しい使い方は素材側のREADMEを参照してください。
