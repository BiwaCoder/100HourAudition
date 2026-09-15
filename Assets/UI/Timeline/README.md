# Timeline Feed

`TimelineFeed.prefab`をCanvas配下に配置し、ルートのTimeline Feed Controllerを設定する。
`Assets/Scenes/Library/TimelineDemo.unity`には13件・5件表示のデモを用意。

## 設定

- **Page Size**: 1ページの件数。既定5。Inspector変更・SetPageSizeで最初のページに戻る。
- **Fade Duration**: 全体の切り替え時間（秒）。既定0.4。半分で消え、次ページを配置して残り半分で現れる。0なら即時切り替え。
- **Entries**: 順番に表示する投稿。アイコンSprite、名前、行動説明、セリフ、時刻、リアクション数を設定。
- **Show Reactions**: 投稿ごとに数値バッジを表示／非表示。
- **On Page Changed**: 切り替え完了時のページ番号（0始まり）を通知。

5件表示なら1–5 → 6–10 → 11–13。最後はボタンが無効になり「これで全件です」。
空リストは「まだ投稿はありません」。切り替え中の連打は無視される。
入力ボタン・ページ番号はフェードせず、投稿部分だけが切り替わる。
無効化すると切り替えをキャンセルし、再有効化すると最初のページへ戻る。
Time.timeScale=0でもフェードする。

```csharp
feed.SetPageSize(5);
feed.SetEntries(new [] {
    new TimelineEntry {
        speaker = "Runa", action = "shared a thought",
        dialogue = "ここから、私たちの100時間が始まる。",
        timeLabel = "now", avatar = portraitSprite,
        likes = 12000, stars = 3400, views = 890
    }
});
feed.NextPage();
feed.ResetToFirstPage();
```

SetEntriesはデータをコピーして最初から表示し、進行中のフェードをキャンセルする。
投稿のSpriteはSprite (2D and UI)としてインポートして指定。空なら名前の頭文字を表示。
リアクションは表示用の値であり、SNSへの投稿や通信はしない。

## 配置と再利用

- rootの幅を指定すると行の幅・高さを再計算。セリフは折り返して全文表示する。
- 高さはContentSizeFitterが内容に合わせて伸縮し、ボタンが常に最後の投稿の下へ移動する。
- ページ件数や長文を増やして画面より高くなる場合、呼び出し側でスクロール領域へ配置するか、表示件数を減らす。
- rootの高さを別LayoutGroupで強制しないこと。GeneratedRowsはView専用の生成コンテナ。
- `TimelineRow.prefab`で1投稿分の見た目を変更できる。名前・行動説明・時刻は一行を想定。
- シーンへの自動起動・Canvas生成なし。ボタンを操作するシーンにはEventSystem + InputSystemUIInputModuleとCanvasのGraphicRaycasterが必要。
- 共通のガラス描画（Assets/Scripts/UI/Common/Runtime/GlassSurfaceGraphic）と日本語フォント（AuditionHeader/Fonts）を再利用する。別プロジェクトへ移す場合は依存アセットも含める。
- デモの人物画像はParticipantCards/Artを参照。本体Prefabには人物カードやゲーム進行への参照はない。

## 設計

TimelinePaginationModelがページ範囲、TimelineFeedView/TimelineRowViewが表示、TimelineFeedControllerが設定とフェード進行を担当。
