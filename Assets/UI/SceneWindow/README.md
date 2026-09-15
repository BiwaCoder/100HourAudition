# Scene Window UI

`SceneWindow.prefab` を既存Canvasの下へ配置します。UIだけのパーツで、シーンロード、映像再生、画面への進入、LIKE数更新は行いません。確認用シーンは `Assets/Scenes/Library/SceneWindowDemo.unity` です。

## 編集する場所

- `Viewport/RecordingBadge`：REC表示。不要なら非アクティブにします。
- `Viewport/SpeakerAndTime`：人物名と時刻。
- `Viewport/Dialogue`：セリフ。長文は自動縮小（14–19）するので、それでも収まらない場合は要約するか枠を広げます。
- `Footer/ClipTitle`：エピソード・クリップ名。
- `Footer/StarsAndTime`：Star数と経過時間。
- `Viewport/OpenButton` / `FavoriteButton`：標準Button。On Clickは未接続です。後でアプリ側の処理をInspectorから登録できます。

## 後で映像をつなぐ

`Viewport/PreviewImage` は無効状態のRawImageです。TextureにRenderTextureなどを指定して有効化し、`Placeholder` を非アクティブにします。現時点ではカメラ・RenderTextureを生成せず、所有・解放もしません。枠と同じ比率の映像を用意するか、接続側でアスペクト比を調整してください（初期枠600×342）。

`Viewport/ContentRoot` は追加UIの配置先です。Viewportの丸角Mask内に表示され、REC・字幕・ボタンはその上に重なります。装飾のRaycast Targetは無効です。追加する表示用Graphicも同様に設定してください。

ルートのCanvasGroupは後でウィンドウ全体をフェードさせる際に利用できます。透明度0で操作も止める場合は、接続側でinteractableとblocksRaycastsも無効化してください。

## サイズと依存

標準640×440。映像枠は親に追従し、フッターは下端に固定しています。横幅を極端に縮める場合は文字とボタンの配置を調整してください。縦横比の固定や全画面への拡大処理は実装していません。

既存の `Assets/Scripts/UI/Common/Runtime/GlassSurfaceGraphic.cs` と `Assets/UI/AuditionHeader/Fonts` の日本語・セリフ体フォント、TMPのLiberationSansを再利用しています。透明パネルは半透明グラデーションで、背景のリアルタイムぼかしではありません。別プロジェクトへ持ち出す場合は依存も含めてExport Packageしてください。

配置先のCanvasにはGraphicRaycaster、シーンにはEventSystemが1つ必要です。Prefab自身にはCanvasやEventSystemは含めていません。
