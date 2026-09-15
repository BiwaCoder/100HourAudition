# Reality show / Participant cards

`ParticipantCard.prefab` を任意のCanvasの下へドラッグして使う独立UIパーツ。
ChoiceMenuやゲーム進行・SNSバックエンドには依存しない。
`Assets/Scenes/Library/ParticipantCardsDemo.unity` に3名を配置した確認用シーンを用意。

## 設定

Prefabルートの **Participant Card Controller → Settings** を変更すると、編集画面でも更新される。

| 項目 | 用途 |
| --- | --- |
| Portrait | 人物のSprite。画像のImport SettingsをSprite (2D and UI)にして指定。空ならサンプル人物画像 |
| Display Name / Subtitle | 名前 / 年齢・職業・地域などの補足 |
| Number | #付きの参加者番号 |
| Stars | SNSのStar数。342000 → 342K STARS。0以上で表示 |
| Support | 0〜1の支持率。バーとパーセントに反映 |
| Status / Accent | FRONTRUNNER等の任意ステータスとバッジ・バーの色 |
| Eliminated | ONで画像をグレースケール＋暗くし、ELIMINATEDとOUTスタンプを表示。OFFで元の色へ復帰 |

画像は縦横比を保った中央トリミング（Cover）。元画像や共有Materialは書き換えない。
Prefabの基準サイズは352×500。サイズはRectTransformまたは親LayoutGroupで設定。
カード自体はCanvas/EventSystemを生成しない。表示のみならEventSystemは不要。
複数並べる場合は親にGridLayoutGroup等を付ける。デモは3列のグリッド。
英語標準フォント設定済み。日本語名を使う場合はカード内のTMPに日本語フォントを指定する。

## ゲームから更新

```csharp
using HundredHour.UI.Participants;

card.SetEliminated(true);       // グレーアウト + OUT
card.SetEliminated(false);      // 復帰
card.SetPortrait(portraitSprite);
card.SetStars(352000, 0.30f);   // Star数と支持率は独立した値
card.SetData(new ParticipantCardData {
    portrait = portraitSprite,
    displayName = "Runa Aizawa",
    subtitle = "22 · Aquarium keeper · Kyoto",
    number = 1, stars = 342000, support = .28f,
    status = "FRONTRUNNER", eliminated = false
});
// Settingsのフィールドをコードから直接書き換えた場合はcard.Refresh()を呼ぶ。
```

## 構成

- ParticipantCardData: 設定・表示データ
- ParticipantCardController: Inspector/APIからViewへの接続
- ParticipantCardView: 表示更新とカード専用Materialの管理
- RoundedCardGraphic: 素材を必要としない角丸面・グラデーション
- PortraitGrayscale.shader: 写真のグレーアウト。uGUI Mask/RectMask2D対応
- Demo: 脱落切り替え、Star追加、リセットの使用例
- Editor: Unity APIによるPrefab/シーン初回生成ツール（既存を上書きしない）

Art内の人物画像はデモ用に図形で作った仮素材。実キャラクター画像へ自由に差し替え可能。
別シーンへの移植はPrefabを配置するだけ。別プロジェクトへ移す際はこのフォルダとTMPフォント依存を含める。

## 検証済み

Unity 6000.3.19f1 / Unity CLI・Pipeline / Gameビューで確認。
- Prefabを3枚配置して保存・シーン再読み込み後も人物設定を保持。
- 実ボタン操作で脱落解除→カラー復帰、Star追加→数値・バー更新。
- 自動チェックでOUT/グレーアウト連動、他カードへの非干渉、画像差し替え、縦横比、未設定時の代替画像、数値制限、シェーダーコンパイルをPASS。
- 修正後の動作確認で新しいConsoleエラー・警告なし。

再実行はデモのPlay Mode中に：
```sh
unity command eval_file --file Assets/Scripts/UI/ParticipantCards/Checks~/CardCheck.cs --format json
```
