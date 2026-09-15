# 性別選択に応じた案内音声（2026-09-14）

ユーザー確認済み：男性キャラクター選択→女性ボイスmarin、女性キャラクター選択→男性ボイスcedar。未確定の言語選択は従来cedar。
タイトルのGenderSelectedイベントで旧WebSocketとPCMを停止し、選択した性別・現在画面・切り替え通知を渡して再接続する。古い接続の遅延コールバックは世代番号で破棄する。
切替直後は「声まで衣替えしちゃった。これからはこの声で案内するね」という趣旨で短く案内。メニューから言語選択の説明へ戻らない。メニュー再訪は「おかえり」の趣旨。軽いユーモアを入れるが、からかわず、長話しない。
写真案内と声でキャラメイクの面接も同じ性別→声の対応とスタイルを適用する。本編の恋愛NPCに新規音声は追加しない。

PythonAPIの/api/live-title-tourにはgender, phase, switchedを追加（すべて検証、既存クライアントの省略を許容）。既存/api/live-interviewのgenderをsession_configへ渡す。共有guide_voice / guide_styleで統一。
Liveのaudioは起動時設定なので接続を作り直す方式：
https://developers.openai.com/api/docs/guides/live-conversations （Update session configuration）
モデルはgpt-live-1のまま。接続・案内には既存のAPI設定を使用する。新たなマイク入力は取得しない。

検証:
- Unityコンパイル成功。
- test_guide_voice.py 5件成功（対応、英日、選択後の画面維持、写真、言語選択の先取り防止）。
- test_title_language_guidance.py 4件成功。
- test_live*.py 21件中19件成功。残り2件はHEADの変更前live_interview.pyでも同じ失敗を確認（旧「デジタル空間」の挨拶期待、opening_progressの既存期待）。今回の音声切り替えによる新規失敗なし。
- ローカル稼働API経由の実gpt-live-1接続：男性/title→marin、女性/title→cedar、男性/portrait→marin。各192000 bytes（24kHz mono PCM16、4秒）の音声受信を確認。Readyだけでなくaudioまで確認。
- 音色の主観的な印象や、おちゃめさの聞こえ方はゲームでの試聴調整の余地あり。
