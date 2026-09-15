# 軽量な日本語 / English切り替え

Unity Localization・Addressables等の追加パッケージは使用しない。文字サイズ、RectTransform、既存フォントは切り替え処理から変更しない。既存UIに元からある自動サイズ調整はそのまま。

## 使い方

翻訳辞書は `Assets/Resources/Localization/GameText.json`。1項目に `key`, `ja`, `en` を置く。新しい項目は `menu.continue` のような安定したキーを推奨。既存の日本語本文からも引けるため、一度に全テキストをキーへ置き換える必要はない。

```json
{"key":"menu.continue","ja":"続きから","en":"Continue"}
```

```csharp
using HundredHour.Localization;
label.text = GameLanguage.Text("menu.continue");
GameLanguage.Select(GameLocale.English); // PlayerPrefsに保存、Changed通知
GameLanguage.Select(GameLocale.Japanese);
```

変数を含む表示は辞書に `{0}`、`{1}` 等を置き、`GameLanguage.Format(key, args...)` を使う。既存の日本語から表示する場合も登録したテンプレートに一致すれば値を保持して翻訳する。明示的な区切り（改行、` / `、` · `、`：`）を含む合成UIは部分ごとに辞書を引く。任意の部分文字列を一括置換しない。

固定ラベルには `LocalizedText` をTMPまたは旧UI.Textと同じオブジェクトへ追加し、keyと日本語フォールバックを指定。`Changed` イベントで更新する。ゲームロジックが毎回上書きするラベルでは、書き込み側で `Text` / `Format` を呼ぶ。無登録の文章や空の英訳は日本語にフォールバックする。

## SeasideMansionへの統合

- `MansionArrivalDirector.Start` が `LanguageStartMenu.WaitForChoice(font)` を待つ。日本語/Englishを選び、開始ボタンを押すまで導入は進まない。Time.timeScaleは変更しない。
- 毎回の起動で選択画面を表示。前回の選択は `PlayerPrefs["100Hour.Language"]` に保存し、次回の選択状態として復元する。
- `MansionArrivalView` と `MansionSignalView` の表示境界で翻訳。ゲームのID、ルール、ShowContent、ゲームのセーブデータは翻訳で書き換えない。
- 初回自己紹介・脱落・7ページのAI開示・実習・主要操作・通常カード名/説明・初期NPC会話を登録。
- 別シーンへ使う場合は、その開始処理から同じゲートを呼ぶ。実行中のEventSystemと既存の日本語対応TMPフォントを利用する。

## 現時点の範囲・未対応

これは全プロジェクトの翻訳完了ではなく、今後追加できる軽量な土台とSeasideMansionの初期対応。

- 記憶合成で動的に作る文章、短縮済みの発言を引用する未来予測通知、視点の長い通知、一部の特殊な結果表示は日本語フォールバックが残る。
- RealityShowFilm / RealityShowの独立シーン、RealtimeVoiceTestなどの別テストシーンは今回未統合。音声入力/生成の言語やAI APIのSystemPromptも変更していない。
- プレイヤーや外部AIが自由生成した文章を翻訳するサービスは呼ばない。API費用は発生しない。
- 文字サイズごとの英語向け調整、全解像度・全分岐のレイアウト検証は今回の範囲外。

## 検証

`Checks~/VerifyLanguage.cs` はUnity CLI evalで実行。辞書の重複・空英訳・テンプレート・フォールバック・導入とAI説明を検証。`Docs/Localization` に結果とPlay画面。開始の待機、英語選択と保存、実ボタンの開始、日英往復でゲーム状態とテキストサイズを保持することを確認する。
