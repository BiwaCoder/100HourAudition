# 100 Hour Audition — 新しい入口

開始シーン: `Assets/Scenes/Audition/AuditionTitle.unity`。Build Settings先頭に登録。
言語選択 → 本編 / 声でキャラメイク / 写真でキャラメイク。各コピーにメニューへ戻る導線あり。

- `AuditionVoice`: GPTLiveVoiceTestのコピー。既存GPT-Live会話と生成APIを使い、生成成功イベントから人物JSONを参加者へ保存。
- `AuditionPortrait`: PhotoIllustrationTestのコピー。画像表示とボタンを調整、同じホログラム背景。成功した画像を参加者PNGへ保存。
- `SeasideMansionAudition`: SeasideMansionのコピー。RuntimeでShowContentを複製し、名前・人物情報・趣味・顔写真を適用。IDはルール互換のためhimari。自己紹介、相手の自己紹介への返答、自己紹介の記憶に趣味を使用。画像は人物の写真窓に使用し、3Dキューブの形状は従来通り。
- `AuditionTitle`: 新規。Berkshire Swashと低速のホログラム。表示言語とプロフィール状態を反映。

保存先は `Application.persistentDataPath/AuditionEntry/participant.json` と `portrait.png`。再作成時は参加者データを更新する。画像と人物像は独立して作れる。本編セーブは人物JSONのSHA256先頭16桁別で、元MansionSignalのセーブと分離。既存人物を作っていなくてもひまりで開始できる。

元の3シーンは変更なし。共通スクリプトの接続箇所は追加イベント・新コピーのBridgeがある場合の言語スキップ・customParticipantの場合の自己紹介変更。PhotoIllustrationの比率修正は元のテストでも有効。

画像生成APIは入力比率に応じて正方形/縦長/横長を選択。生成後は元比率に中央トリミングする（引き伸ばさない）。任意比率の直接生成ではなく、生成画像周辺の一部が切れる場合がある。元写真そのものをトリミングして送信することはない。Unityの表示はFitInParentで元の画像枠に収める。

写真選択はUnity Editor対応、macOS standaloneのネイティブ選択処理も追加（実ビルド未検証）。Windows/mobile/WebGLのファイル選択は未対応。APIサーバー・認証設定は既存のApiEndpointConfigを利用する。

検証: `Docs/AuditionEntry/verification.txt` とスクリーンショット。Python画像比率の3テスト（6形状とモックエンドポイント）、Unity生成イベントによる保存→メニュー→本編の適用、Intro→Walking、英語/日本語、Input System UIを確認。今回の実音声会話・実画像生成・配布ビルドは未実施。テストデータは削除済み。

Editor生成 `AuditionSceneSetup.Create()` は既存コピーがあると停止する。Polishは初回の見た目セットアップ用で再実行しない。保存済みのシーンを直接調整する。

## プレイヤーの性別（2026-09-13）

タイトルで言語→性別→メニュー。性別はプレイするキャラクターの設定としてPlayerPrefsに保存。女性は既存participant.json/portrait.pngと本編セーブを維持し、男性はparticipant-male.json/portrait-male.png、game-male-*へ分離。男性配役はResources/AuditionCast/MaleCast.json。攻略対象：星野ひかり、ライバル：雨宮陽翔・星野蓮・橘蒼真。内部IDはルール互換のため従来のものを使う。デフォルト男性主人公は悠真、専用の生成画像yuma.pngを使用。男性プレイ時の主人公キューブは黒。新規4人の画像は生成済み。

音声JSONと写真生成APIにplayer_genderを送信。完成JSONのcharacters[0].genderに保存し、設定補完と自己紹介生成にも使用。性別は実際のユーザーについて推定せず、選択したゲーム設定として扱う。PythonAPI更新の反映が必要。検証記録：Docs/AuditionEntry/Gender。

### タイトル音声が無言になる問題（2026-09-13）

live-title-tourは指示のみ送信し入力音声ストリームがないため、接続後もLiveが進まず無言になっていた。サーバーでsession.started後から40msごとに24kHz/mono/PCM16の無音960サンプルを送るタスクを追加。マイク収録はしない。切断・終了時は他タスクとともにキャンセル。
実接続の比較: 修正前10秒はlive.ready=1、live.audio=0。修正後はlive.ready=1、live.audio=1、PCMピーク4096を受信して終了。音声を端末では再生していない。Unityコンパイル成功、Playは停止のまま。AuditionTitleNarratorにReady/AudioChunks/LastError、エラー時のNoticeとConsole警告を追加（秘密を含むURLや例外全文は表示しない）。タイトル案内は一方向の音声であり、マイク会話は面談シーン側。

### 写真画面の音声案内（2026-09-13）

AuditionSceneBridgeが写真画面にもAuditionTitleNarratorを生成し、screen=portraitで接続。写真専用の日英プロンプトで選び方を説明し、BusyChanged(true)と保存成功で生成中・完成の合図を送信。写真そのものは音声側に送らず、見たと主張しない。タイトルの既存ON/OFF設定を共有し、画面退出で切断。マイクなしの音声案内方式。
実APIで音声受信（PCMピーク2832）、AuditionPortraitのPlayでReady=true、AudioChunks=165、LastError=null、AudioSource再生を確認。画像生成・写真送信は行わず、Play停止。既存のタイトル無音修正も未コミットのまま保持。

### 言語ボタンのホバー案内（2026-09-13）

日本語/EnglishのPointerEnterをLanguageFocusedで通知。0.35秒滞在後にlanguage_hoverを送信し、PointerExitは待機を取り消す。ホバーはGameLanguage設定を変更しない。接続前の合図も値とともに保持。サーバーは指定言語で「その言語で遊べる」「下のボタンで開始」を案内し、commentaryで発話を促す。
初期プロンプトを言語選択だけに限定。性別説明は実際のGenderScreenShown合図で初めて与え、性別画面では確定済みUI言語に戻す。写真画面の言語切替は従来の汎用案内を維持。
検証: Python4テストPASS、Unityコンパイル、日英PointerEnterと設定不変を確認。実GPT-Liveで冒頭は「日本語かEnglishを選んで、始めるときは下のボタン」、英語ホバーは「You can play in English. Select English and then press the button below to begin.」、日本語ホバーは対応する日本語説明を取得。冒頭に性別の発言なし。Play停止、音声ON/OFF設定は元へ復帰。

恋愛対象の詳細設定と立ち絵は `RealityShow/Runtime/RomanticLeadProfiles.cs` に集約。女性主人公の恋愛対象は松村悠斗、既存の女性ライバルhikariは白石杏奈へ変更。詳しくは `Docs/RomanticLeads/README.md`。
