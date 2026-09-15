from __future__ import annotations
import io
import base64
import asyncio

import openai
from fastapi import APIRouter, Depends, UploadFile, File, Form
from typing import Optional, Literal
from PIL import Image, ImageOps

from app.config import settings
from app.schemas.photo import PhotoIllustrationResponse
from app.security import require_signed_request

router = APIRouter(prefix="/api", tags=["photo"])
client = openai.OpenAI(api_key=settings.openai_api_key)

MAGAZINE_ILLUSTRATION_PROMPT = (
    "この人物写真の面影(髪型・雰囲気・表情)を活かしながら、幻想的な女神・巫女のような"
    "気品あるアニメ風イラストに描き直してください。繊細な線画とセル塗り、"
    "宝石や花をあしらった髪飾り、後光のような光の輪、光の粒子を散りばめてください。"
    "背景は夕焼けに染まる豪華な庭園やプールサイド、宮殿のような建築。"
    "全体的に上質で幻想的な「運命の物語の主人公」のような一枚の雑誌表紙イラストにしてください。"
)


def compress_image(image_bytes: bytes, target_bytes: int = 500_000, max_dimension: int = 1024) -> bytes:
    img = ImageOps.exif_transpose(Image.open(io.BytesIO(image_bytes)))
    if len(image_bytes) <= target_bytes and max(img.size) <= max_dimension:
        return image_bytes

    img = img.convert("RGB")
    if max(img.size) > max_dimension:
        ratio = max_dimension / max(img.size)
        new_size = (int(img.size[0] * ratio), int(img.size[1] * ratio))
        img = img.resize(new_size, Image.LANCZOS)

    data = image_bytes
    for quality in (85, 70, 55, 40):
        buf = io.BytesIO()
        img.save(buf, format="JPEG", quality=quality)
        data = buf.getvalue()
        if len(data) <= target_bytes:
            break
    return data


def generation_size(source_size: tuple[int, int]) -> str:
    ratio = source_size[0] / source_size[1]
    return "1536x1024" if ratio > 1.2 else "1024x1536" if ratio < 1 / 1.2 else "1024x1024"


def preserve_source_aspect(encoded: str, source_size: tuple[int, int]) -> str:
    """Crop generated margins, never stretch the person or crop the uploaded source."""
    import math
    generated = Image.open(io.BytesIO(base64.b64decode(encoded))).convert("RGB")
    divisor = math.gcd(*source_size)
    unit = (source_size[0] // divisor, source_size[1] // divisor)
    multiplier = max(1, min(1536 // max(unit), int(min(generated.width / unit[0], generated.height / unit[1]))))
    target = (unit[0] * multiplier, unit[1] * multiplier)
    if max(target) > 4096:
        # Coprime source dimensions: preserve ratio to within one output pixel without excessive allocations.
        scale = 1536 / max(source_size)
        target = tuple(max(1, round(side * scale)) for side in source_size)
    output = ImageOps.fit(generated, target, method=Image.Resampling.LANCZOS)
    buffer = io.BytesIO()
    output.save(buffer, format="PNG")
    return base64.b64encode(buffer.getvalue()).decode("ascii")


@router.post(
    "/photo-illustration",
    response_model=PhotoIllustrationResponse,
    dependencies=[Depends(require_signed_request)],
)
async def photo_illustration(file: UploadFile = File(...), player_gender: Optional[Literal["male", "female"]] = Form(None)) -> PhotoIllustrationResponse:
    try:
        raw_bytes = await file.read()
        source = ImageOps.exif_transpose(Image.open(io.BytesIO(raw_bytes)))
        source_size = source.size
        normalized = io.BytesIO()
        source.convert("RGB").save(normalized, format="JPEG", quality=95)
        image_bytes = compress_image(normalized.getvalue())

        def _generate_impression() -> str:
            b64_in = base64.b64encode(image_bytes).decode("utf-8")
            response = client.chat.completions.create(
                model="gpt-4o",
                messages=[
                    {
                        "role": "user",
                        "content": [
                            {
                                "type": "text",
                                "text": (
                                    "この写真から伝わる、雰囲気・世界観・持ち主の人柄を、"
                                    "キャラクター作りの参考になるように日本語で2〜3文で描写してください。"
                                ),
                            },
                            {"type": "image_url", "image_url": {"url": f"data:image/jpeg;base64,{b64_in}"}},
                        ],
                    }
                ],
                max_tokens=300,
            )
            return response.choices[0].message.content

        def _generate_illustration() -> str:
            buf = io.BytesIO(image_bytes)
            buf.name = "photo.jpg"
            result = client.images.edit(
                model="gpt-image-1",
                image=buf,
                prompt=(MAGAZINE_ILLUSTRATION_PROMPT if player_gender != "male" else
                    "Create a premium illustrated portrait of an adult male fictional game character inspired by this photo. Contemporary elegant clothing, warm cinematic light, natural proportions. No text. Preserve the face's distinctive features while honoring the explicitly selected male game role.") + (
                    f" 元写真の縦横比は {source_size[0]}:{source_size[1]}。"
                    "元写真の構図を尊重し、人物と髪飾りを中央の安全領域に収めてください。"
                    "出力後に元写真と同じ比率で中央トリミングするため、周辺には背景の余白をとってください。"
                ),
                size=generation_size(source_size),
                quality="medium",
            )
            return preserve_source_aspect(result.data[0].b64_json, source_size)

        loop = asyncio.get_running_loop()
        impression, illustration_b64 = await asyncio.gather(
            loop.run_in_executor(None, _generate_impression),
            loop.run_in_executor(None, _generate_illustration),
        )

        return PhotoIllustrationResponse(
            success=True, impression=impression, illustration_base64=illustration_b64
        )
    except Exception as exc:
        return PhotoIllustrationResponse(success=False, error=str(exc))
