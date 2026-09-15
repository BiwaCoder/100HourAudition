# 審査導入の検証（2026-09-13）

- Python37テストPASS。冒頭の日英必須内容、50秒チェック、ボタン選択で必須チェックが無効にならないこと、assistantだけを確認対象にすること、説明/質問の不足分だけ促すことを確認。
- ローカルrelay→実Live APIに無音PCMを送る自動確認も実施したが、55秒の試験期間にassistant文字起こしを取得できず、冒頭の実発話は検証できなかった。終了時cancelを送信し接続を閉じた。マイクは使っていない。
- この試験を実音声の成功とは扱わない。実マイクの会話で案内・質問・割り込み後の復帰を確認する必要がある。

## 2026-09-13 クモノから会話を開始

冒頭 instructions の受理を `client_event_id` で確認してから、session-wide の `session.commentary.append` を一度送る。日本語／英語を指定し、相手の発言を待たず審査案内と質問を話し始める。別の指示の受理や重複通知では再開始しない。マイクの無音PCMも送信を継続する既存実装を使用。

公式手順: https://developers.openai.com/api/docs/guides/live-conversations#greet-before-the-caller-speaks

前回未確認だった無音開始をローカルリレー → 実OpenAI APIで再検証。入力は無音PCMのみ、ユーザー発言なし。日本語は接続readyから2.24秒、英語は2.10秒で非無音の出力PCMを受信。審査・リアリティーショー・デジタル空間へのワープ・最初の質問まで日本語20.07秒、英語13.10秒で確認。`Opening-ja.json` / `Opening-en.json` に記録。これはAPI音声／文字起こしの検証であり、Unityスピーカーの実聴検証ではない。

回帰テスト `tests/test_live_greeting.py`: ユーザー音声なしで開始要求、受理ID照合、重複受理の抑制、日英指定、キャンセル時の接続終了。
