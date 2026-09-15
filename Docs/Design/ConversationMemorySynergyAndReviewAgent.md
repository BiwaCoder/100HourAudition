# 会話メモリ・シナジー × OpenAI Agents SDK レビューエージェント 設計書

対象: 会話バトルのコアメカニクス強化(記憶シナジー・目的指向の話題生成)と、それを繰り返しプレイで検証・調整するためのエージェント機構。
**このドキュメントは設計のみ。実装・Unity操作は行っていない。**

---

## 0. 前提: 既に動き始めている実装との関係

このドキュメントを書いている最中に確認したところ、**まさに同じ方向性の実装が並行して進行中**だった。

- `Assets/Scripts/RealityShow/Runtime/ShowConversationMemory.cs`(新規) — `TopicGoals`(3つの会話目的)、`SelectRecall`、`BeginTopic`、`ComposeRecall`、`SynergyKey`/`MemoryBonus`、`RecordConversation`、`SenseFeelings`
- `ShowState.cs` — `understanding`(相互理解)、`ConversationMemory`型、`memorySynergies`リスト、`topicGoal`/`recallA`/`recallB`/`memoryFeedback`
- `MansionSignalDirector.cs` — `MansionMemoryJournal journal`への参照(ただし該当ファイルは執筆時点でまだ存在せず、実装が進行中と思われる)
- 沙織の開眼演出の最終ページも、すでに「未来観測・偶然の発動・タイムリープ」への言及が消え、「上部の『ふたりの記憶』」「気持ちの察知」だけを説明する内容に書き換わっていた

つまり要求内容と実装の方向性はすでに一致している。よって本書は**ゼロから設計するのではなく、進行中の仕組みを正式に言語化・補強し、まだ存在しない「評価エージェント」層(6章)を新規に設計する**という構成にする。実装側が今後変わっても、この文書の6章(エージェント設計)と7章(検証プロセス)は独立して有効なはずである。

---

## 1. ゴール

- ゲームを「コミュニケーションに特化した体験」に絞り込む
- 検証したい核となる3つの仮説:
  1. 会話内容が「ふたりの記憶」として蓄積されることに意味を感じるか
  2. 記憶×記憶、記憶×新しい話題の組み合わせ(シナジー)が、好感度・相互理解を伸ばす行為として魅力的か
  3. 「目的を選んでAIに新しい話題を作らせる」体験が、便利ボタンではなく**意味のある選択**として機能しているか

---

## 2. 現状把握(進行中の実装から読み取れる仕様)

| 要素 | 内容 |
|---|---|
| `TopicGoals` | `"深く知る"` / `"自分も伝える"` / `"一緒の未来"` の3種。目的を選んでからAIに話題を作らせる |
| `SelectRecall(id, second)` | 記憶ジャーナルから記憶A・記憶Bを選ぶ(2つまで) |
| `BeginTopic(goal)` | AGI消費(既存`acquireCost`を流用)、選んだ目的と記憶Aの話題をシードにAI生成へ |
| `ComposeRecall()` | ローカル(非AI)版。記憶を使った会話選択肢を1つ挿入 |
| `MemoryBonus` | 記憶1つの再利用で+4、記憶2つの連結で+7。**同じ記憶は3回まで**しか新しい角度を生めない(使い尽くし制限) |
| `RecordConversation` | 発言確定後、ボーナスに応じて`understanding`(相互理解)を+1/+4/+6。会話ログに`ConversationMemory`として保存 |
| `SenseFeelings` | 旧UsePerspectiveの再設計版。「相手が今どちらを求めているか」をヒントし、記憶ジャーナルを見るよう促す |

この設計はすでに「AIが代わりに選ばない、手がかりを渡すだけ」という本プロジェクトの一貫した思想(同調プロトコルで言語化した原則)に沿っている。`SenseFeelings`はヒントを出すだけで、実際にどの記憶を選ぶかはプレイヤーに委ねられている。

---

## 3. 一時的に隠す機能(テストスコープ)

検証対象を「記憶シナジー」と「目的指向の話題生成」に絞るため、以下は**一時的に非表示**にする。

| 隠す機能 | 理由 |
|---|---|
| 未来観測(Forecast) | 最適解を教えてしまい、記憶を自分で選ぶ楽しさを弱める |
| 偶然の発動(SetWeather) | コミュニケーション以外の変数を増やし、検証対象がぼやける |
| タイムリープ(TimeLeap) | 「失敗をやり直す」安全弁があると、記憶の組み合わせを慎重に選ぶ動機が薄れる |
| 割り込みアピール(InterruptAppeal) | 章1限定の対人リスク機構で、記憶シナジーの検証とは別軸 |

**残す2つ**:
1. 目的を選んで新しい話題を生成する機能(`BeginTopic`/`ComposeRecall`系)
2. 相手の気持ち察知(`SenseFeelings`)

すでに開眼演出の最終ページがこの2つだけを説明する内容に更新されていたことから、実装側も同じ縮小スコープで進めている可能性が高い。

---

## 4. 上部バー「ふたりの記憶」ウィンドウ — UI仕様

`MansionMemoryJournal`という名前で参照されているが、ファイル自体は未実装(執筆時点)。以下は仕様としての提案。

- **配置**: 画面上部バーに開閉トグル(アイコン+「ふたりの記憶」ラベル)
- **開いている間**: 他の操作(会話選択・ツールパネル)をブロックする(`MansionSignalDirector.Choose()`にはすでに`journal.IsOpen`のガードが入っている)
- **一覧表示**: `ConversationMemory`ごとに、話題(topic)の短縮表示＋自分の発言の抜粋
- **選択操作**: 1つタップ→記憶A、別の1つタップ→記憶B。同じものを連続タップすると解除
- **使用済み表示**: `MemoryBonus`が0になる(=3回使い切った)記憶には「出し尽くした」等の印を出し、無駄なタップを減らす
- **確定操作**: 「この記憶で話す」(`ComposeRecall`)/「新しい話題を作る」(目的3択→`BeginTopic`)の2導線

---

## 5. 相互理解(understanding)の意味づけ

- 好感度(trust)とは別軸の指標。「どれだけ相手を正しく理解できたか」を表す
- 増加要因は`RecordConversation`のとおり(通常+1、記憶1つ再利用+4、記憶2つ連結+6)
- **現時点では「見えるが使い道が薄い」指標**。これをどう活かすかを検証すること自体も今回のテスト目的に含める(オープン論点として8章に記載)

---

## 6. OpenAI Agents SDKによる実行・評価・修正エージェント設計(新規)

### 6.1 全体の流れ

```
[Plan] 目標レンジを人間が定義
   ↓
[Do] 人間が実際に何度かプレイする(生成キャラ・AI有効)
   ↓  ※ここで会話テレメトリが自動的にJSONLへ蓄積される
[Check] レビュースクリプトを手動実行 → Agentがテレメトリを読み、6.5の観点で評価
   ↓
[Act] Agentが数値の変更案(パッチファイル)を根拠つきで提示
   ↓
人間が読んで判断 → 承認した項目だけ設定ファイルに反映
   ↓
[Do] へ戻って再プレイ
```

### 6.2 「シンプル」に保つための設計判断

- **常駐サーバーは作らない**。Agentはプレイの合間に手動実行する1本のスクリプトで十分(`PythonAPI/agents/review_conversation.py`のようなイメージ、今回は未実装)
- **既存のテレメトリ基盤を流用**。`PythonAPI/app/telemetry.py`の`log_event()`パターンに、新しいイベント種別(`memory_turn`/`session_summary`)を追加するだけで済む設計にする
- **ライブ設定は直接書き換えない**。Agentは「パッチ案ファイル」を出力するだけにし、人間が読んで反映するかどうかを決める(均衡プロトコルで定めたガードレールと同じ思想)

### 6.3 記録するテレメトリ(拡張案)

**`kind: "memory_turn"`**(発言確定のたびに1件)
```
client_id, loop, chapter, turn, target,
action_id, goal (使ったTopicGoal、未使用ならnull),
recall_a_topic, recall_b_topic (記憶ID経由ではなく可読な話題文字列で記録する),
bonus (0 / 4 / 7), understanding_before, understanding_after, trust_delta,
memory_count_at_this_point
```

**`kind: "session_summary"`**(1プレイ終了時)
```
client_id, total_turns, total_memories_formed,
synergy_fired_count, goal_usage_counts (3種それぞれの使用回数),
sense_feelings_used_count, final_understanding, final_trust,
exit_phase (Victory / Eliminated / 途中離脱)
```

### 6.4 Agentに与えるツール(function_tool)

| ツール名 | 役割 |
|---|---|
| `read_recent_turns(limit)` | `memory_turn`イベントを新しい順に読む |
| `read_session_summaries(limit)` | `session_summary`イベントを読む |
| `read_balance_config()` | 現在のチューニング値(6.6)を読む |
| `propose_balance_patch(changes, rationale)` | 変更案を人間レビュー用のパッチファイルへ書き出す(ライブ設定は変更しない) |

OpenAI Agents SDKの`Agent`+`function_tool`+`Runner.run()`をそのまま使う想定。ハンドオフや複数エージェント連携は不要(シンプルさ優先)。

### 6.5 評価観点(Agentのinstructionsに埋め込む基準)

1. **シナジー発火率** = シナジー成立ターン数 ÷ 総発言ターン数。目安レンジ(例: 20〜40%)より低ければ「記憶の使い道が伝わっていない」、高すぎれば「毎回起きて特別感がない」と判定する
2. **目的(TopicGoal)の偏り** — 3種類が均等に使われているか。1種類に偏っていれば理由を指摘する(コスト差・文言の魅力差など)
3. **気持ち察知→シナジーの動線** — `SenseFeelings`使用直後、数ターン以内にシナジーが発生しているか(ヒントが実際に活きているかの代理指標)
4. **相互理解の伸び方** — 序盤で頭打ちになっていないか、逆に早期に100到達して以降退屈になっていないか

### 6.6 チューニング対象パラメータ(将来の`MemoryBalance.json`案)

```
memoryBonusSingle        既定 4
memoryBonusPair          既定 7
maxSynergyPerMemory      既定 3
understandingInsightBase 既定 1
understandingInsightSingle 既定 4
understandingInsightPair 既定 6
topicGoalCost            既定 acquireCostと共通
senseFeelingsCost        既定 1
```

### 6.7 ガードレール

- 1回のレビューで1パラメータあたりの変更幅は**±30%まで**
- 提案には必ず根拠(rationale)を添える。根拠のない数値変更は採用しない
- **最低プレイセッション数**(例: 3回分)が溜まるまではレビューを走らせない(1回のブレを過剰に信用しない)
- 反映は必ず人間の承認を経る

---

## 7. 検証プロセス(「何度かプレイして確かめる」の具体化)

1. まずデフォルトキャラ・AI無効(ローカル生成のみ)で数回プレイし、フローが破綻しないか確認
2. 生成キャラ+AI有効で3〜5回プレイし、テレメトリを溜める
3. レビュースクリプト(将来実装)を実行し、6.5の観点でAgentの評価を読む
4. 気になった点を**1つだけ**手動で調整する(複数同時に変えない)
5. 2に戻って再検証

**数値だけに頼らない**: シナジーが成立した瞬間に驚きや納得感があるかという主観評価を、プレイのたびに一言メモしておく。Agentの評価はあくまで補助であり、最終判断は人間の手触りで行う。

---

## 8. オープンな論点(今後判断すること)

- 相互理解(understanding)の数値を、UI表示以外にどう活かすか(記憶カテゴリ解禁・エンディング分岐・察知の精度向上など)
- 記憶ジャーナルで「使い切った記憶」をどう視覚的に示すか
- 3つのTopicGoalのラベル(「深く知る」「自分も伝える」「一緒の未来」)は直感的に選び分けられるか
- OpenAI Agents SDKを実際に使うか、既存のChat Completions+手書き評価ロジックで十分か(シンプルさを最優先するなら、Agents SDK自体が過剰という判断もあり得る)
