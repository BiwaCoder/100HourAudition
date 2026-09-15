# 沙織・海辺の邸宅のAI

2026-09-12 / built-in image_gen 使用。新規イラスト（参照画像を利用）。
成果物: `Assets/Environments/SeasideMansion/Signal/SaoriSeaMansion.png`
元の参照画像は変更していません。

参照1: ユーザー提供の銀髪の女性・海・逆光のイラスト。
参照2: `Assets/Screenshots/SeasideMansionExterior.png`（今回の実際の3D邸宅）。

## 最終生成プロンプト

Use case: illustration-story. Create a NEW high quality landscape 3:2 anime game key illustration for an SF reality-show game. Image 1 is visual inspiration for an adult goddess-like AI woman, long silver hair, graceful white dress, delicate blue flowers, luminous backlighting, reflective water and poetic atmosphere. Image 2 is architectural reference: THIS game's seaside mansion is a low white modern courtyard villa with flat roofs, glass walls, palm trees and central fountain, NOT a skyscraper city. Show the adult AI guide Saori standing on the shallow reflecting pool terrace of this seaside mansion, ocean horizon and the low villa behind her. Waist-to-knee-up framing, face clearly readable, gentle intelligent welcoming expression, eyes open, one hand inviting the viewer. Dramatic sunset rim light, soft blue and white holographic particles and subtle orbit-like light filaments suggest artificial intelligence, airy sea breeze and flowing silver hair. Exquisite painted anime illustration with detailed face, cinematic serene mysterious mood. Character occupies center with mansion visible at both sides, no interface, no lettering, no watermark. Full opaque illustrated background. Save output for use as game reveal art.

## ユーザー修正後の採用アセット（最新）

旧 `SaoriSeaMansion*` は不採用。ゲーム内の参照を以下に変更。

- `SaoriPrayerSilhouette.png`: 全身を覆う祈りの姿・逆光
- `SaoriPrayerClosed.png`: 閉眼（身体・背景の固定ベース）
- `SaoriPrayerHalf.png`: 半開眼キーフレーム
- `SaoriPrayerOpen.png`: 開眼キーフレーム

すべて built-in image_gen 使用。元の参照イラストと先に生成した画像は変更せず、新規ファイルへ保存。
ユーザー指定: 胸元まで覆うドレス、脚を露出しない、赤面しないクールな美しい女神、身体を崩さず両手を前で祈るように合わせる、最初は閉眼しゲームで開眼。

### 閉眼の最終プロンプト

Use case: identity-preserve / character redesign for animation. Redesign the adult silver-haired AI goddess from the reference as COOL, dignified, sacred and beautiful, NEVER seductive. New full-body landscape 3:2 anime illustration, woman centered from head to floor-length dress hem with comfortable margin, preserve anatomically correct natural proportions, straight upright composed posture without arched back or tilted hips. BOTH HANDS are gently joined together in front of her sternum in a calm prayer pose, anatomically correct fingers and wrists. Eyes FULLY CLOSED, serene neutral face, absolutely NO BLUSH, no pink/red cheeks, no coy smile, cool pale complexion. Clothing is an elegant OPAQUE modest white ceremonial gown with HIGH CLOSED COLLAR covering the entire chest up to the neck, full long sleeves covering shoulders and arms, fabric with comfortable ease rather than body-hugging contours; a continuous floor-length flowing skirt fully conceals both legs and feet with NO slit, NO cleavage, NO bare thigh, NO translucent fabric, NO lingerie styling, NO cutouts. Delicate blue floral detailing and silver embroidery, long silver hair, refined silver-blue halo ornaments. Background: keep this actual low white modern seaside courtyard mansion, central fountain, reflecting pool, ocean horizon and luminous sunset. Goddess stands on pool terrace with reflections, majestic radiant rim light; cooler silver-blue fill light on face, warm sky, highly polished painted anime game illustration. The calm closed eyes and prayer hands are the emotional focus. This is the closed-eye keyframe for later minimal eye-opening animation, so clean stable symmetrical frontal framing. NO text, NO UI, NO watermark.

### 開眼の最終プロンプト

Use case: precise-object-edit. This is an animation keyframe. Change ONLY BOTH EYELIDS and visible irises in this exact illustration: the goddess now opens both eyes fully, gazing straight ahead with cool clear pale ice-blue eyes, composed divine expression, no smile, absolutely no blush or red cheeks. Preserve exact face geometry and exact eye positions; keep mouth identical, skin tone unchanged and neutral. EVERYTHING ELSE MUST REMAIN IDENTICAL: full body pose, prayer hands, closed high collar, opaque long sleeves, floor-length white gown fully covering legs and feet, silver hair strands, mansion, water, sunset, ornaments, composition, framing, colors and lighting. Do not zoom, shift, crop or redraw the body. Same landscape dimensions as reference. Output full image, not a crop or comparison sheet. No text or watermark.

### 半開眼の最終プロンプト

Precise animation keyframe edit. Change ONLY the two closed eyelids in this exact image to HALF-OPEN eyes: eyelids lifted halfway, slim visible bands of cool pale ice-blue irises, calm distant divine gaze. This is the intermediate frame between eyes closed and eyes fully open, not a sleepy or seductive expression. Absolutely no blush, no smile. Do not alter any other pixels or features if possible. Preserve the exact same eye positions, face geometry, skin tone, mouth, prayer hands, full-body straight posture, modest high-collar long-sleeved opaque floor-length white dress completely hiding legs and feet, hair strands, jewelry, background, light, composition, scale and framing. Full landscape image same size. No zoom, crop, labels or watermark.

### 逆光の最終プロンプト

Use case: lighting-weather. Create the BEFORE-REVEAL silhouette keyframe from this EXACT illustration. Preserve EXACT full-body straight posture, BOTH HANDS joined in prayer, eyes CLOSED, all body proportions, fully modest opaque white ceremonial floor-length gown, high closed collar, long sleeves, fully covered legs and feet, flowing silver hair outline, jewelry, background geometry, frame and scale. Change lighting only: the goddess herself is a near-black dignified silhouette, facial features hidden, with thin bright silver/gold rim light; the white seaside courtyard mansion, fountain, water and sunset sky remain clearly visible and brilliantly backlit, a radiant halo of sunlight behind her. NO exposed chest/arms/legs, no changed pose, no forward reaching, no smile or blush. This must read as a cool sacred goddess appearing in blinding light, not a darkened whole picture. No text or watermark.
