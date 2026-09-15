# Seaside Mansion

シーン: `Assets/Scenes/GameScene/SeasideMansion.unity`

NatureStageを複製し、SkyStudio・Sun / Sun Ambient・MistFilter・Cinemachine・Joystick Packを引き継いだ邸宅確認シーンです。SkyStudioプロファイルと空・海のマテリアルは邸宅専用の複製を使用しています。

## 到着シーンの流れ

Playすると黒からフェードイン → 中央に「100 Hour Audition」 → 上空からズームと説明 → 中庭を右から横移動 → ひまりへズーム → 背後カット、と自動再生します。説明パネルはズーム開始から0.7秒でフェード表示。タイトルにはBerkshire Swashを使用（OFL、Arrival/Fonts/OFL.txt）。

導入が終わると左下にジョイスティックが現れます。上へドラッグしてレッドカーペットをまっすぐ進むと、青緑の相手役キューブ（松村悠斗）の手前で操作が止まり、肩越しのカットと「いま、新たな物語が始まる」の煽り文を表示します。終了後はSFリアリティーショーの自己紹介へ進みます。遭遇演出は1回のみです。

- 操作中は左下のジョイスティックで移動、Spaceでジャンプ。
- 演出中はEscapeでスキップし、操作へ戻れます。
- 旧1〜4の確認カメラ切替は、物語再生との競合を避けて通常は無効です。

調整対象:

- `Mansion Arrival Story` の `MansionArrivalDirector`: 字幕、遭遇距離、人物参照。
- `Arrival/MansionIntroduction.asset`: 導入5カットの時間、ズーム、フェード、横移動量。
- `Arrival/FirstEncounter.asset`: 相手に近づいたときのカット。
- `Mansion Story Canvas`: 下部字幕と移動案内。View・進行制御・カメラデータを分離。

既存CutscenePlayerを再利用。邸宅全景に対応するためショットの許容距離を150m、高さを60mまで拡張しました。既存ショットの値は変更していません。

`lateralTravel` は右から左への平行移動量（メートル）。0で従来通り、負数で逆方向。文字のフェード時間はDirectorの `textFadeSeconds` で調整できます。`Tools/100Hour/Polish Mansion Opening` は導入プリセットを再適用します（カスタムの導入ショットを置き換えます）。変更前のシーンは `Backups/SeasideMansionOpening/BeforeTitle.unity`。

出入口は開いた構造で、建物・床・家具に静的MeshColliderを追加しています。装飾の花・葉・水流は歩行を妨げないようコライダー対象外です。追従カメラは既存のNatureFollowCameraを再利用しています。全ての狭い場所でのカメラ壁抜けを防ぐ高度なカメラ衝突回避は未追加です。

## アセット

- `Assets/Environments/SeasideMansion/SeasideMansion.fbx`: Blenderから出力したUnity用メッシュ
- `Assets/Environments/SeasideMansion/SeasideMansion.prefab`: 材質・当たり判定を設定した再利用用Prefab
- 同フォルダの `Materials`: Built-in Standard用の材質
- `ArtSource/SeasideMansion/export_unity.py`: FBX再出力スクリプト
- `Assets/Screenshots/SeasideMansion*.png`: Unityでの確認画像

Blenderの+Y（海側）はUnityでは-Zになります。1 Unity unit = 1m。
`SeasideMansionSetup.Configure` はNatureStageを複製した未設定のSeasideMansionシーンに一度だけ使う構築処理です。既存のモデルがある場合は中止します。通常の調整はシーン内のオブジェクトや専用マテリアルを編集してください。

## 確認

コンパイル成功。Playで約6.53mのジョイスティック移動、停止、接地、Cinemachine追従を確認。入口・中庭脇・ラウンジ・部屋の開口に遮る建築コライダーなし。外観・中庭・室内をキャプチャし、Consoleにランタイムエラーなし。

到着演出の確認: 導入中は操作・ジョイスティックとも無効、終了後に有効化。実際のスティック入力で相手へ接近し、遭遇時に停止、説明表示、終了後に操作復帰、再発火なしを確認。変更前のシーンはBackups/SeasideMansionArrivalに保存。

## SFリアリティーショー統合（2026-09-12）

現在は到着後の遭遇から会話ゲームへ進みます。既存の到着カットシーン・3D邸宅・キューブ・Joystickは維持。

1. 移動中からキューブの右上に人物窓。ひまりの画像は見えますが、相手は緊張によるノイズのみ。
2. 遭遇カット後に自己紹介を選択。実況・解説を経て初回脱落。
3. 同じ窓が中央へ再接続。「番組が続いている？」という疑問から、海辺の邸宅を背景に逆光の女神が見える。
4. 沙織がAIと名乗ると鮮明なイラストへ。胸元・腕・脚を覆うロングドレス、両手を祈るように合わせたクールな姿。頬は赤くせず、閉じた目が半開眼を経て静かに開く。
5. 観測・状態推定・分岐演算・フィードバック・目的関数と不確実性を説明。脱落を「現実でまだ確定していない計算上の未来」として擬似タイムリープへつなぐ。
6. 場所を選び、右の未来観測を実行。観測では発言・時間・乱数を進めず、既存ルールで候補を比較。実習時だけAGIを6補給（上限8）、自分で言葉を選ぶ。
7. 他人の視点、偶然の発動、新しいカード、巻き戻しを自由に試せる。記憶タブで「尋ねる」「伝える」「結ぶ」、デッキタブで所持カードも確認できる。既存の3章・選考・勝利/脱落まで進行。

### 実装

- `Runtime/MansionSignalDirector.cs`: Seasideの演出/チュートリアル進行、既存ShowGameへのコマンド、専用保存。
- `Runtime/MansionSignalView.cs`: uGUI表示、キューブ追従の人物窓、低輝度の動的ノイズ、開眼タイミング。
- `Editor/MansionSignalSetup.cs`: Unity APIによる初回組み込み。既に組み込み済みなので再実行不要。
- `Assets/Environments/SeasideMansion/Signal/SaoriPrayer*.png`: 逆光・閉眼・半開眼・開眼の新規イラスト4枚。
- `PrayerEyeReveal.shader` / `SaoriPrayerEyes.mat`: 目の領域だけを3段階に補間。目以外は閉眼画像を固定し、身体・背景を変形させない。0.9秒閉眼を保ち、その後1.8秒で開眼する。

RealityShowFilmの`ShowGame`、`ShowContent`、`ShowDialogue.Local`、人物画像、フォント、既存の`ChoiceMenu`（マウス/上下＋Enter）を再利用しています。Filmシーンの全面UIや2.5Dステージへは移動しません。会話はローカル生成（APIキー/通信不要）。沙織の台詞は字幕で表現し、音声合成は追加していません。

保存先はpersistentDataPathの`MansionSignal-v1.json`。既存Film用`RealityShowRun-v1.json`とは独立。再Play時は右上の「前回の続きから」で演出ページとチュートリアルも復元。通常Playは到着から始まり、会話に入ると新しい進行で保存されます。

初回組み込み前のバックアップ: `Backups/SeasideMansionSignal/BeforeSignal.unity`。
生成プロンプト・採用画像の記録: `ArtSource/SeasideMansion/Signal/README.md`。
Play検証用コード: `Checks~/VerifySignalPlay.cs`（Pipeline evalで実行。テストの保存はTemp内）。

## Mansion Detail V2（環境ディテール強化）※未コミット

噴水・列柱・ヤシ・植栽・海面を専用シェーダー/LOD付きモデルへ差し替える環境強化パス。到着演出・SF Signal機能には影響しない見た目の作り込み。

- `Editor/MansionDetailSetup.cs` の `Tools/100Hour/Apply Mansion Detail V2` で適用。**初回限定処理**で、シーンに `Mansion Detail V2` オブジェクトが既にあれば例外で中止する。既に適用済みなので通常は再実行しない。
- 旧ディテールオブジェクト（`Gallery column`,`Palm trunk`,`Fountain`等の名前接頭辞に一致するもの）は削除せず`SetActive(false)`で非表示化のみ。
- 新規シェーダー: `Shaders/CoastalWater.shader`（海・噴水盤の波/きらめき/`_Horizon`遠景カリング）、`Shaders/CoastalDetail.shader`（葉の風揺れ・石材グレイン）、`Shaders/FountainCascade.shader`,`Shaders/WaterDroplet.shader`（噴水の流水・水滴）。
- 新規アセットは `Assets/Environments/SeasideMansion/DetailV2/`: 列柱・ヤシ・植栽・噴水のLOD0/1モデル、専用マテリアル13点（`Palette.json`から生成）、生成コライダー`BasinCollision.asset`、まとめた`MansionDetails.prefab`。
- 列柱・ヤシにCapsuleCollider、噴水盤にMeshColliderを付与。装飾用LODGroup（しきい値.18/.012）を各ディテールに設定。
- Blenderソース一式は `ArtSource/SeasideMansion/DetailV2/`（`build_details.py`,`render_details.py`,`inspect_source.py`,`mesh_budget.json`,`SeasideMansion-DetailV2.blend`）。
- 適用前バックアップ: `Backups/MansionDetailV2/BeforeDetails.unity`。確認画像: `Docs/MansionDetailV2/`（courtyard/exterior/fountain/ocean/plants各1枚）。
- 2026-09-12時点でシーンには適用済みだが、コード・アセット・シーン差分ともに未コミット。次回コミット時にまとめて扱う。

## 素材の詳細化 V2

`Mansion Detail V2` を追加し、噴水・柱8本・ヤシ12本・花壇4床をBlender製の曲面と個別の葉・花弁へ更新しました。各素材にLOD0/LOD1、噴水には水面・透明な落水・上限256粒の飛沫を使用。海側にOceanモディファイアから書き出した320m四方の波形メッシュとGPUの波アニメーションを配置。遠景の海面と境界を合わせています。

元の `Seaside Mansion` 内の対象1,848オブジェクトは非表示で保持。間取り・導入・会話ゲームは維持しています。初回セットアップ `MansionDetailSetup.Apply` は適用済みで再実行不要。`MansionWaterPolish.Apply` は水面の設定を再適用可能。

編集元: `ArtSource/SeasideMansion/DetailV2/SeasideMansion-DetailV2.blend`。旧元データ `ArtSource/SeasideMansion/SeasideMansion.blend` も保持。Unity用の再利用Prefabは `Assets/Environments/SeasideMansion/DetailV2/MansionDetails.prefab`。素材別FBXは同フォルダ。描画比較と画像は `Docs/MansionDetailV2`。


## Mansion Detail V3 — 灯具、家具、建築の納まり

`MansionFurnishingSetup.Apply` で灯具6基・ソファ10台・クッション20個・丸机8台・食卓の天板2台、壁/柱の腰壁・巾木・目地・見込み・笠木を追加。適用済みのため再実行不要。旧90オブジェクトは非表示で保持。44組の2段階LOD、壁ごとの結合メッシュ。木目・布・石の材質は `CoastalFurnishing.shader`。`MansionSoftGlow` はSeasideMansionカメラのみのHDRブルームで、縦横1/4の一時RT2枚を使う。

編集元と参考画像: `ArtSource/SeasideMansion/DetailV3`。シーン適用前バックアップ: `Backups/MansionDetailV3/BeforeFurnishings.unity`。確認画像とPlay/材質検証: `Docs/MansionDetailV3`。元データの詳細と写真参考はArtSource内READMEを参照。


## 軽量な日本語 / English切り替え

開始時に言語選択画面を表示し、開始ボタンまでArrivalを待機。翻訳はResourcesのJSON辞書とGameLanguageで管理する。Unity Localizationの追加は不要。表示境界で翻訳し、既存のテキストサイズとセーブデータは保持。導入・初回脱落・AI説明・操作など183項目を登録し、未登録の動的文章は日本語へフォールバック。詳細は `Assets/Scripts/Localization/README.md`。

## 2026-09-13: 章別会話・アーキタイプ V2

MansionSignalDirectorは新ルールを有効化。自己紹介1対1、グループ1対3、ツーショット1対1。MansionConversationStageが既存キューブとカメラを使い、グループ時だけライバル2体と画像を追加。MansionSignalViewが手札を3枚に拡大、ステータスと効果文を読みやすくし、観測画面を表示。初回AI実習の後でカード→選択肢を説明。通常のシーン再セットアップ・保存は不要。

36枚のJSONカード、レア章報酬、周回デッキ選択、NPCのカードと嫉妬、好み倍率、AI話題と返答、Python検証は `Docs/ConversationDeck/README.md` を参照。新コードはAudition版コピーにも共通で適用されます。


## 会話の記憶とAI話題（2026-09-13）

現在のMansion本編はカード撤去を維持し、上部の開閉ウィンドウから会話記憶A/Bを選ぶ方式。AIは目的別話題生成と気持ち察知だけを表示。発言・返答を共有記憶として保存し、新しい組み合わせが好感度と相互理解につながる。仕様・検証・画面は [Docs/ConversationMemory](../../../../Docs/ConversationMemory/README.md) を参照。旧シーンの再セットアップ不要。

## 場所選択の移動演出（2026-09-13）

`MansionSignalDirector.ChooseRouteWithTravel` から `MansionConversationStage.TravelToRoute` を呼び、暗転 → 目線高さ1.65mで約3.4秒の歩行視点 → 暗転 → 二人の会話構図へ移行します。歩行視点には小さな上下・左右の揺れがあります。長距離の横断は暗転中に省略し、見える移動は中央通路の最後5.2mです。人物の歩行アニメーションではなくカメラによる移動表現です。

窓辺は海側の大窓前（中心z=-17）、庭は中央噴水の手前（z=4.6、噴水内への侵入を回避）。人物も到着地点へ移します。移動中はBusyで選択をロックし、会話UIを隠します。ルール更新は到着時、保存はフェード完了後。ロード時も保存されたルートに人物を配置します。既存のMansion系シーンで共通に有効になり、シーンアセットの再セットアップは不要です。

## 本文と選択肢ポップアップ（2026-09-13）

下部本文は画面高24%・横幅96%。話者と「選択肢を開く」「ログ」を上段に置き、本文は左右分割せず全幅で表示します。長文は既存MansionMessageScrollでスクロールします。
選択肢が1つの場合は下部ウィンドウ内の本文下に、その選択肢を直接クリックできるボタンとして表示します。複数の場合は既存ChoiceMenuControllerを中央モーダルに移して再利用。新しい本文・選択肢で自動表示し、閉じるボタン・外側クリック・Escで閉じ、下部ボタンで再表示します。同一表示のRefreshでは閉じた状態を保持します。閉じるとControllerが無効化されるため隠れた選択肢への入力を防ぎ、再表示時には選択位置を復元。選択確定時は閉じてから元のChoiceイベントを通知します。ログ/記憶/観測を開く際も閉じます。シーンの再生成は不要。
検証: Docs/DialoguePopup（表示・再表示・入力抑止・確定・長いリストのスクロール）。

## 接近時の顔表示と小ジャンプ（2026-09-14）

歩行中は相手をノイズで表示し、ArrivalのEncounter開始で顔を中央の拡大ウィンドウに表示。最初の自己紹介中も顔を維持する。画像未設定時はImageを無効にしノイズを維持して、白い矩形になるのを防ぐ。接近時の音声は追加しない。Auditionシーンのジャンプ高は4mから0.3mに変更（Playで上昇約0.276mを確認）。

呼び名はShowCharacter.callNameで指定。ひかり・悠斗・杏奈を明示し、未指定の人物はフルネームを使う。末尾2文字を切り取らない。
