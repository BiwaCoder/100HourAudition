# 100Hour Audition / Glass UI

半透明のグラデーション、白い縁取り、ネイビーとコーラルのタイトルで構成したuGUIパーツ。
背景そのものをぼかすカメラエフェクトではなく、透過するUI面を重ねた表現。
背景・素材・色が違うシーンにも配置できる。

## Prefab

- **AuditionHeader.prefab**: タイトル・説明・タイマーをまとめた透明パネル。1120×650。
- **AuditionTitle.prefab**: タイトルのみ。AuditionTitleViewで2行の文字と色を設定。
- **AuditionDescription.prefab**: 日本語説明文と4つの能力。AuditionDescriptionViewで変更。
- **AuditionClock.prefab**: 100時間に対応したHOURS / MIN / SECカウントダウン。

任意のCanvas配下へ配置する。Canvas/EventSystem/ゲームの進行処理はパーツが生成しない。
表示だけならEventSystem不要。デモのボタンを使う場合はInput Systemが必要。
各パーツを別々に移動・配置可能。全体を異なるサイズで使うときはCanvasScalerで画面へ合わせる。
個別Prefabの基準寸法は各RectTransform参照。説明を長くした場合はStoryの高さと能力チップの位置を調整する。

## タイマー

InspectorのInitial Hours（既定100）、Start Automatically、Use Unscaled Timeで設定。
既定では静止表示。デモのSTART / PAUSEまたはゲーム側のResume()で開始。
無効化中は進まず、再有効化すると残り時間を保持する。アプリ終了中の経過やセーブはホスト側の責務。
表示更新は整数秒が変化した時のみ。24時間で折り返さない。

```csharp
clock.Resume();
clock.Pause();
clock.ResetClock();              // 初期時間へ戻して停止
clock.AddTime(60);               // タイムリープ：60秒戻す
clock.SetRemainingSeconds(7200); // セーブから2時間を復元
clock.OnCompleted.AddListener(OnAuditionFinished);
```

0で停止し、On Completedを一回通知する。時間を追加して再開すれば再度通知できる。
タイマー自体はAGIエナジーの消費・能力の発動ロジックを持たない。

## 日本語とフォント

Noto Sans CJK JP（説明）とNoto Serif（タイトル・数字）を同梱。
説明の文字を静的アトラスへ収録し、日本語の追加編集には1024pxの動的フォールバックを使用。
フォールバックのClear Dynamic Data On Buildは有効。数字はAutoSizeを使わない。
フォントのライセンス文書はFonts内に同梱。
取得元：[Noto CJK](https://github.com/notofonts/noto-cjk)、[Noto Fonts](https://github.com/notofonts/noto-fonts)。
説明中のHTML空白と末尾のバックスラッシュを除去し、句読点と改行を整えて表示している。

## デモ

`Assets/Scenes/Library/AuditionHeaderDemo.unity` を開いてPlay。
START / PAUSE、TIME LEAP +60 SEC、RESET 100 HOURSで操作を試せる。
Runtimeは他の人物カード・選択UIに依存しない。

## 動作確認

Unity 6000.3.19f1 / Unity CLI・Pipelineと実Gameビューで確認。
- 日本語説明・4能力・英語タイトルを表示。
- 実ボタンでカウントダウン開始、一時停止、60秒追加、100時間リセットを確認。
- モデルの100時間表示、端数秒、時間境界、0停止、終了通知一回性、再開、無効値を検証。

モデルチェックを再実行：
```sh
unity command eval_file --file Assets/Scripts/UI/AuditionHeader/Checks~/ClockModelCheck.cs --format json
```
