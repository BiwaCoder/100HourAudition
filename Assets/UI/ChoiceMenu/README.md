# Shiro選択UI（コードのみ）

Shiro/Assets/Scripts/Shiro/ShiroUI.cs の ChoiceSelector、BuildSelector、選択行・決定点滅を抽出。
本体は背景、ログ、音声、セーブ、ゲーム進行、Shiroのフォント素材に依存しない。
英語の動作確認用にChoiceMenuDemoシーンとUnity標準TMP Essential Resourcesを別途追加。
Unity 6 / uGUI（TextMeshProを含む）/ Input Systemを使用。

## 枠だけ付ける

既存の文字やボタンの RectTransform に **ChoiceSelectionFrame** を追加。
InspectorのSelected、または `frame.SetSelected(true/false)` で表示を切り替える。
コンポーネントを無効化すると枠が消え、再有効化すると復帰。削除すると生成した枠だけを破棄。
文字や対象自体の位置・色・拡縮は変更しない。

元と同じ細枠1.5、ブラケット18×3 / 3×14、余白(4,3)、拡縮±2%・速度5.5、
透明度0.5〜1・速度4。色・速度・余白はInspectorで変更できる。
Glitchは元の演出に合わせ既定で有効。不要ならEnable Glitchをオフ。
Time.timeScale=0でも動作する。グリッチ確率は60fps相当を時間換算し、ゲーム本体の乱数に干渉しない。

## 選択メニュー全体を使う

Canvas内に幅と高さを持つ空のRectTransformを用意し、ChoiceMenuControllerを追加
（ChoiceMenuViewは自動追加）。ViewのFontに日本語対応TMP_FontAssetを指定。
キーボードも使う場合はChoiceMenuKeyboardInputを追加。
CanvasにはGraphicRaycaster、シーンにはEventSystem + InputSystemUIInputModuleを1つ用意する。
共用Canvas/EventSystemをモジュールが勝手に生成・削除することはない。
TMPを初めて使うプロジェクトでは、TMP Essential Resourcesと使用フォントをホスト側で準備する。
英語デモにはLiberationSans SDFを設定済み。日本語を表示する場合は日本語フォントへ差し替える。

```csharp
using HundredHour.UI.Choices;

// 既存コンポーネントをInspectorから参照してShowしてもよい。
var menu = ChoiceMenuFactory.Create(choiceArea, japaneseFont);
menu.Confirmed += index => HandleChoice(index); // 0始まり
menu.Show(new[] { "音の因果 ── ■■■■■ 100%", "定めて、死路へ進む" });

// 外部のInputAction/UIから操作する場合：
menu.Move(1);
menu.Select(0);
menu.Confirm();

// 一時的に閉じる：
menu.Close();
// 完全に取り外す（Factoryが作った子のみ）：
UnityEngine.Object.Destroy(menu.gameObject);
```

Show直後の同フレームは入力を無視するため、呼び出し元の決定キーの持ち越しを防ぐ。
上/下・W/Sで循環、Enter/テンキーEnter/Spaceで決定、1〜9・テンキー1〜9で直接決定。
マウスホバーで移動、左クリック・タップで決定。決定後は入力をロックし、
0.05秒の消灯・点灯を2回繰り返して閉じ、結果イベントを1回通知する。
空リストは閉じる。Showの再呼び出し、Close、無効化では古い決定待ちをキャンセルし通知しない。
無効化から復帰した際はShowを呼び直す。
音を追加するときはSelectionChanged / Confirmedを購読する（解除も呼び出し側で行う）。
InspectorのOn Confirmedも使用できる。同じ処理を両方へ重複登録しない。
複数メニューが同時に開く場合、操作対象以外のChoiceMenuKeyboardInputを無効化する。

## 責務と取り外し

- **Model**: ChoiceMenuModel。Unity非依存のラベル・選択位置・決定ロック。
- **View**: ChoiceMenuView / ChoiceRowView / ChoiceSelectionFrame。描画とポインター通知。
- **Controller**: ChoiceMenuController。状態と表示の接続・決定演出・結果イベント。
- **入力アダプター**: ChoiceMenuKeyboardInput。削除して独自入力に差し替え可能。
- **生成窓口**: ChoiceMenuFactory。既存Canvasの指定範囲にメニューを作る。

モジュールをプロジェクトから外す場合は、呼び出し元の参照と付けたコンポーネントを外し、
使用中のシーンからコンポーネントを外した後、Assets/Scripts/UI/ChoiceMenuとAssets/UI/ChoiceMenuを削除する。他機能への参照・追加パッケージはない。

## 検証（2026-09-11 / Unity 6000.3.19f1）

Unity CLI → 接続済みUnity Pipelineのeval_fileで実行し、以下がPASS。

- Model: 呼び出し元配列からの独立、上下循環、不正indexの拒否、決定の一回性・ロック、空リスト、再設定。
- Frame: 独立したPreviewSceneで12本の線、非raycast、表示切替、拡縮範囲、ホストTransform不変、無効化・復帰・削除を確認。
- Editorでコンパイル済み。Pipeline Consoleのerrorは0件。

再実行する場合（Checks~はUnityの通常インポート対象外のCLI評価コード）：

```sh
unity command eval_file --file Assets/Scripts/UI/ChoiceMenu/Checks~/ModelCheck.cs --format json
unity command eval_file --file Assets/Scripts/UI/ChoiceMenu/Checks~/FrameCheck.cs --format json
```

## 英語デモ

`Assets/Scenes/Library/ChoiceMenuDemo.unity` を開いてPlay。
今回のChoiceMenuFactory / Controller / View / KeyboardInput / SelectionFrameを直接使用する。
デモ固有のゲーム進行例は `Assets/Scripts/UI/ChoiceMenu/Demo/ChoiceMenuDemo.cs` に分離。

- 上下 / W・Sで選択、Enterで決定、1〜3で直接決定。
- マウスのホバー・クリックでも操作可能。
- 決定結果を英語で表示。RまたはREOPENで再表示。
- 英語フォントに「▶」がない場合は「>」を表示して文字欠けを防ぐ。

Unity CLI/Pipelineと実際のGameビューで英語の表示を確認済み。
実キーの下→Enterで2番目、マウスクリックで3番目の結果を確認。
REOPEN→下→Enterの再操作も確認済み。
Play Mode計測では拡縮0.9800〜1.0200、透明度変化、決定時の点滅、
決定通知の一回性、Controller無効化後の表示破棄と復帰をPASS。
日本語表示・タッチ実機は未確認。

再計測（デモのPlay Mode中に実行。結果ファイルは約3秒後）：

```sh
unity command eval_file --file Assets/Scripts/UI/ChoiceMenu/Checks~/DemoRuntimeCheck.cs --format json
# 結果: Temp/ChoiceMenuRuntimeCheck.json
```
