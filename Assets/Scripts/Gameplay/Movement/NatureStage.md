# NatureStage の操作・調整

対象：Assets/Scenes/GameScene/NatureStage.unity

- NaturePlayerCube：Rigidbody＋JoystickCubeMover。左下のFixed Joystickをドラッグするとカメラ基準で移動、離すと停止。Speedは3.5。スペースキーで接地中のみジャンプ（Jump Height初期値1.5）。空中での再ジャンプはできません。TerrainColliderで地面と衝突します。
- NatureFollowCamera：CinemachineCamera＋Follow＋RotationComposer。FOV 40、WorldSpaceでFollowOffset (4,3,5)、PositionDamping (0.6,0.8,0.6)。キューブを追跡します。
- Camera (1)：MainCameraタグ、CinemachineBrain、MistFilterEffect。競合するLowPolyVegetation_CameraControlは無効化しています。
- Mist：Charmoveの軽量版スクリプトと2パスShaderを使用。Intensity .533 / Blur 2.75 / HighlightBoost 1.82 / Softness .78 / Haze .079 / Spread 1.13。Built-in Render Pipeline用です。
- スクリプト：Assets/Scripts/Rendering/MistFilter/MistFilterEffect.cs
- Shader：Assets/MistFilter.shader

SkyStudioの空・天候、地形・植生は既存の設定を使用します。既存ステージにはTerrainCollider以外の障害物コライダーは確認されていません。必要な木や建物の衝突判定は対象オブジェクト側で追加してください。

検証：スティックイベントで約2.54m移動、停止時の水平速度ほぼ0、カメラ約2.56m追従、2パスミスト描画。修正後のPlay確認で新しいエラーなし。

変更前のシーン・ミストコード・ShaderはBackups/NatureStageSetup/20260912に保存。NatureStage-with-current-edits.unityは作業開始時の未保存編集を含むバックアップです。
