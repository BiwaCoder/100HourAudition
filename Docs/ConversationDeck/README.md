# 会話デッキ V2（2026-09-13）

SeasideMansion と SeasideMansionAudition の `MansionSignalDirector` に組み込み。シーンの再セットアップは不要。タイトルから本編へ進み、初回脱落→沙織の説明→タイムリープの後にアーキタイプを選ぶ。

## 遊び方

1. 自己紹介（主人公＋悠斗）。最初に未来観測を体験し、カードの準備、実際の発言の順に説明。自己紹介には第一印象ボーナス。
2. グループ会話（主人公＋悠斗＋生存中のライバル2人）。主導権で発言を強化。2・4発言目に割り込みを予告。主導権5、防御、カウンター、譲る選択肢で対処。「ちょっと待って」は主導権不足だと弱い。
3. ツーショット（主人公＋悠斗）。過去の会話を拾う、未来を約束する、嫉妬を素直に話す。親密さで発言が強くなる。
4. 各章をクリアするとレア3候補から1枚。最終章でも報酬を選んでからエピローグ。クリア報酬は周回後も保持。

各発言に集中3、手札3枚。手札を使わず発言してもよい。カードを使っても発言するまで時間は進まない。山札が空なら捨て札を再利用する。罠は最大3個、発動したものを消費。バフ・知恵・罠は章をまたがず、親密さと相手の好みの観測は引き継ぐ。

## アーキタイプとカード

共通データ `Assets/Resources/ConversationBalance.json` に36枚（各系統・通常8枚＋レア4枚）。章報酬は自分の系統。過去に獲得したレアは、別系統に変えてもデッキへ入る。

- ポジティブ：スマイル、ボディタッチ、感嘆、甘える、あだ名。笑顔バフと数発言続く余韻。前のポジティブ札と「わあ、すごい！」が連携。
- 戦略的：自己開示と知恵。知恵の必要枚数が足りないカードは使用不可。悪い噂で相手を妨害する代わりに自分の好感度が下がる。「噂より本人を知りたい」で切り返す連携。
- 芸術家：植物、天気、場所、割り込みに反応する罠。AI植物話題＋植物の伏線、雨を呼ぶ＋雨音の罠などで能動的に発動できる。

悠斗の好みは初期データでは芸術家で固定。該当系統の好感度獲得は整数演算で×1.2（端数切捨て）。「他の会話を覗く」で知り、次のループ／章巻き戻しのデッキ選択に活かせる。

## AIと観測

`useAI=true` が本編の初期設定。既存ChatApiClient/PythonAPIを再利用する。AI話題カードは9枚あり、カードの発想素材、直近の会話、相手の人物像から新しい話題と発言候補を作る。発言後のNPC返答にもAIを使用。

AIには数値・勝敗を決めさせない。JSONの長さ、台詞内の話題の存在などを検証して採用。不正応答や通信失敗はカード固有の発想素材とローカル台詞で続行し、画面に代替を表示。待機中の二重クリック・発言・追加消費を防ぐ。

他の参加者も独立した知恵・バフ・罠・集中・AGIを持ち、通常札を1枚選んで台詞と行動を決める。ライバル側の話題生成はAPIを呼ばずカードの発想素材を使用。各発言後に好感度、スター、嫉妬が変わり、観測にカード名・選択・応酬・好感度変化を記録。妨害札による嫉妬攻撃は主人公にも影響する。全NPCに毎ターンLLMを呼ぶ方式ではない。

## バランス検証

Python標準ライブラリだけで実行できる。

```sh
python3 -m unittest discover -s PythonAPI/tests -p test_conversation_balance.py -v
python3 PythonAPI/balance/conversation.py --runs 1000 --policy random --output /tmp/conversation-runs
python3 PythonAPI/balance/conversation.py --runs 1000 --policy greedy --output /tmp/conversation-greedy
python3 PythonAPI/balance/tune_conversation.py --runs 300 --output /tmp/conversation-candidate
```

`conversation.py` は3章のカード、AI支援コスト、会話選択、NPC行動、選考、レア報酬をオフラインで反復。APIは呼ばず、話題はカードの素材で代替する。ラン結果・ターンJSONL・カード使用数・配布数を出す。`tune_conversation.py` はライバル強度と自己開示札の強さを探索し、効果文も数値に合わせた**候補JSON**を出す。元データの自動上書きはしない。

調整対象：カードのcost/agiCost/wisdomCost/power/持続/条件/連携/weight（候補抽選の重み）/topicSeed/description。全体の上限値、好み倍率、AIスキルの消費、割り込み減点、NPC圧力も同じJSONにある。

`VerifyDeckRules.cs` をPipelineで実行すると324ケースのC#入出力を `parity-fixtures.json` に出す。Pythonテストは実際のUnity出力に対してカードの状態変化・好感度計算を比較する。Unity側は3系統×30シードの進行も検証。

**適用範囲**：共有カード処理と章別好感度式の一致を検証している。Python側のランはバランス検討用のシミュレーションで、Unityのシードと完全に同一のリプレイではない（乱数器、初期会話選択、記憶の履歴などが異なる）。実機の勝率・自由生成文の面白さを保証する数字ではない。

初回調整後、ランダム方策各1000回で完走率はポジティブ99.0%、戦略的72.5%、芸術家97.0%。現状はEasy寄りで、戦略的は連携の習熟を要する。最終バランスは未確定。`Balance/summary.json` と `Tuning/search.json` に数値を保存。

Unity実プレイのログは `Application.persistentDataPath/ConversationTelemetry-v2.jsonl`。バランス版、シード、章・発言、使用カード、選択、好感度増分、AGI、集中、知恵、主導権、親密さ、話題を記録。既存の本編セーブ先は維持し、新しいステータスとNPCの内部状態も保存する。

## 検証資料

- `RulesVerification.txt`：Unityルール検証。
- `PlayVerification.txt`：Playでのチュートリアル、3枚表示、4人表示、観測、生成話題、保存再開。
- `PresentationVerification.txt`：会話後に寄って引くカメラと、レア効果文の収まり。
- `LegacyRulesVerification.txt`：旧ルールの30シード回帰確認。
- `AIVerification.txt`：実通信の採用元と二重消費防止検証。
- `00-archetype.png` ～ `06-rare-reward.png`：実画面。

会話演出は既存キューブ、顔写真、出力カメラ、ポストエフェクトを再利用。グループは広角、ツーショットは狭い画角で、発言後に寄る。動的な変更なので保存済みシーン／Prefabを書き換えない。旧RealityShow/Filmのルールは `deckMechanics=false` のまま維持。
