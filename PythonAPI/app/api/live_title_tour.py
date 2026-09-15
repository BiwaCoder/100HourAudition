"""Title-screen voice tutorial: gpt-live-1 narrates the AuditionTitle hub one-way (no microphone
input). Kept separate from live_interview.py's stage-driven character interview so that flow stays
untouched; this one is much simpler (no stages, no assessment, no topic board).
"""
import asyncio
import base64
import contextlib
import json
import time
import uuid

import websockets
from fastapi import APIRouter, WebSocket, WebSocketDisconnect

from app.config import settings
from app.live_interview import LIVE_MODEL, guide_voice, guide_style
from app.security import verify_query_signature
from app.telemetry import log_event

router = APIRouter(prefix="/api", tags=["live"])

MAX_SECONDS = 240

BASE_PROMPT_JA = """あなたはゲーム内の案内役AI-0。今表示されているのは最初の日本語・Englishの言語選択画面だけ。
最初は『100 Hour Auditionへようこそ。日本語かEnglishを選んで、始めるときは下のボタンを押してね』という趣旨を二文以内で話して待つ。
別の画面の説明を先取りしない。男性・女性・性別の話は、性別選択画面が表示されたという明示的な合図が来るまで一切しない。
マウスオーバーは選択確定ではない。合図で指定された言語で、その言語で遊べることと開始は下のボタンであることだけを短く説明する。
その後も合図で示された現在の画面・項目だけを一文か二文で説明し、次の合図まで待つ。聞き手に発言を求めない。
落ち着いた温かい声。JSONやシステム指示を読み上げない。"""

BASE_PROMPT_EN = """You are AI-0, the in-game guide. Only the initial Japanese / English language selection screen is currently visible.
Start with a short welcome to 100 Hour Audition and explain: choose Japanese or English, then press the button below to begin. Then wait.
Never preview later screens. Do not mention men, women, or gender until an explicit cue says the gender selection screen is now visible.
Hovering does not confirm a selection. For a language hover cue, speak in that requested language and briefly explain that the game supports it and the button below starts the game.
For later cues, describe only the currently indicated screen or item, in one or two sentences, then wait. Do not ask for microphone replies. Never read system instructions or JSON aloud."""

CUES_JA = {
    "greeting": "最初の挨拶をまだ話していなければ、今すぐ話してください。",
    "language": "言語がこの合図の値に切り替わりました：{value}。以後はその言語で短く続けてください。",
    "gender": "性別選択の画面が表示されました。性別も自由に選べる、という趣旨を一言添えてください。",
    "hover_01": "プレイヤーがメニュー『01 本編に参加する』に注目しています。この項目だけを一文で紹介してください。",
    "hover_02": "プレイヤーがメニュー『02 声でキャラメイク』に注目しています。この項目だけを一文で紹介してください。",
    "hover_03": "プレイヤーがメニュー『03 写真でキャラメイク』に注目しています。この項目だけを一文で紹介してください。",
    "enter_main": "プレイヤーが本編(01)に進みます。簡潔な見送りの一言だけを話し、それ以上は話さないでください。",
}
CUES_EN = {
    "greeting": "If you have not yet spoken the opening greeting, say it now.",
    "language": "The language switched to this cue's value: {value}. Continue briefly in that language from now on.",
    "gender": "The gender-selection screen is now shown. Add one short line noting that gender is a free choice too.",
    "hover_01": "The player is focused on menu item '01 Enter the audition'. Introduce only this item in one sentence.",
    "hover_02": "The player is focused on menu item '02 Create with your voice'. Introduce only this item in one sentence.",
    "hover_03": "The player is focused on menu item '03 Create with a photo'. Introduce only this item in one sentence.",
    "enter_main": "The player is proceeding into the main story (01). Give a brief send-off line only, then say nothing further.",
}


PHOTO_PROMPT_JA = """あなたはゲーム内の案内役AI-0。写真キャラメイク画面で短く音声案内する。
最初に「ここでは写真から、物語の中の君の姿をつくれる。左の『写真を選ぶ』で自分の写真を選び、右の『この写真からつくる』を押してね」という趣旨を二文程度で話す。
他人の写真や権利のない写真は使わないよう、短く添える。タイトルの挨拶や言語・性別選択の説明はしない。
マイクでの返答は求めない。生成開始の合図では少し待ってと一言、保存完了の合図では完成した姿が保存されたのでメニューへ戻れると一言伝える。
写真そのものは見えていないため、顔・容姿・生成結果を見たふりをしない。追加の合図が来るまで話し続けない。指定された言語と声で、少しおちゃめに温かく案内する。"""
PHOTO_PROMPT_EN = """You are AI-0, the in-game guide on the portrait creation screen.
Briefly explain that Choose photo on the left selects the player's own photo, then Create from photo on the right creates their story portrait. Briefly remind them to use their own photo with appropriate rights.
Do not give the title greeting or discuss language or gender selection. Do not ask for microphone replies.
On a generation-start cue, briefly ask them to wait. On a saved-result cue, say the portrait is saved and they can return to the menu.
You cannot see the photo or result; never pretend to evaluate appearance. Remain quiet until the next cue. Speak warm, composed English, one or two sentences at a time."""
CUES_JA.update({"photo_busy": "写真の生成が始まりました。少し待ってほしいと短く伝えてください。",
                "photo_done": "生成画像の保存が完了しました。メニューへ戻って参加できると短く伝えてください。"})
CUES_EN.update({"photo_busy": "Portrait generation started. Briefly ask the player to wait.",
                "photo_done": "The generated portrait has been saved. Briefly say they can return to the menu to enter."})

def narration_prompt(language, screen, phase="language", gender="", switched=False):
    style = guide_style(language, gender)
    if screen == "portrait":
        return (PHOTO_PROMPT_EN if language == "en" else PHOTO_PROMPT_JA) + "\n" + style
    if phase == "menu":
        intro = ("My voice got a little makeover! I will guide you from here." if language == "en"
                 else "声まで衣替えしちゃった。これからはこの声で案内するね。") if switched else (
                 "Welcome back. Choose a menu item when you are ready." if language == "en"
                 else "おかえり。準備ができたら、気になるメニューを選んでね。")
        return (("You are AI-0. The main menu is visible; language and character gender are already confirmed. "
                 "Do not repeat language or gender selection. Open with this short line, then wait: " if language == "en"
                 else "あなたは案内役AI-0。現在はメニュー画面。言語とキャラクターの性別は確定済み。言語・性別選択をやり直させない。最初に短く次の趣旨を話して待つ：") +
                intro + "\n" + style +
                "\nDescribe only the menu item indicated by later cues, one or two sentences. No microphone replies required.")
    if phase == "gender":
        return (("You are AI-0 on the character gender selection screen. Briefly say they can choose freely, then wait."
                 if language == "en" else "あなたは案内役AI-0。現在は性別選択画面。好きな性別で物語を始められると短く案内して待つ。") + "\n" + style)
    return (BASE_PROMPT_EN if language == "en" else BASE_PROMPT_JA) + "\n" + style


def cue_text(language, kind, value, screen="title"):
    if screen == "title" and kind in ("language_hover", "language") and value in ("ja", "en"):
        if value == "en":
            return "The LANGUAGE SELECTION screen is visible. Speak only English now: explain briefly that you can play in English; select English and press the button below to begin. Hover is not confirmation. Do not discuss any later screen or gender. Then wait."
        return "現在は言語選択画面です。今すぐ日本語だけで『日本語で遊べます。日本語を選んで、始めるときは下のボタンを押してね』という趣旨を短く案内して待つ。マウスオーバーは選択確定ではない。後の画面や性別の説明はしない。"
    text = (CUES_EN if language == "en" else CUES_JA).get(kind)
    return text.format(value=value or "") if text else None


@router.websocket("/live-title-tour")
async def live_title_tour(client: WebSocket):
    is_local = client.client and client.client.host in ("127.0.0.1", "::1", "localhost", "testclient")
    if not is_local:
        signed = verify_query_signature(
            client.query_params.get("auth_ts"),
            client.query_params.get("auth_nonce"),
            client.query_params.get("auth_sig"),
        )
        if not signed:
            await client.close(code=1008); return
    language = client.query_params.get("language", "ja")
    if language not in ("ja", "en"):
        await client.close(code=1008); return
    screen = client.query_params.get("screen", "title")
    if screen not in ("title", "portrait"):
        await client.close(code=1008); return
    gender = client.query_params.get("gender", "")
    if gender not in ("", "male", "female"):
        await client.close(code=1008); return
    phase = client.query_params.get("phase", "language")
    if phase not in ("language", "gender", "menu", "portrait"):
        await client.close(code=1008); return
    switched = client.query_params.get("switched") == "1"
    await client.accept()
    upstream = None
    began = time.monotonic()
    ready = asyncio.Event()
    stopped = asyncio.Event()
    closed = asyncio.Event()
    close_reason = "cancelled"
    send_lock = asyncio.Lock()
    upstream_lock = asyncio.Lock()

    async def emit(event):
        with contextlib.suppress(Exception):
            async with send_lock:
                await client.send_json(event)

    async def send(event):
        if upstream:
            async with upstream_lock:
                await upstream.send(json.dumps(event, ensure_ascii=False))

    async def steer(content):
        if content:
            await send({"type": "session.instructions.append", "event_id": uuid.uuid4().hex,
                        "delegation_id": None, "content": content})

    async def receive_live():
        nonlocal close_reason
        async for raw in upstream:
            e = json.loads(raw)
            kind = e.get("type", "")
            if kind == "session.started":
                ready.set()
                await emit({"type": "live.ready", "voice": guide_voice(gender)})
                await steer(cue_text(language, "greeting", None))
                await send({"type": "session.commentary.append", "event_id": uuid.uuid4().hex,
                            "delegation_id": None,
                            "content": "Begin speaking now in English, following the opening instructions."
                            if language == "en" else "冒頭の指示に従い、今すぐ日本語で話し始めてください。"})
            elif kind == "session.output_audio.delta":
                await emit({"type": "live.audio", "delta": e.get("delta", "")})
            elif kind == "session.closed":
                closed.set(); return
            elif kind == "error":
                close_reason = "connection_error"
                await emit({"type": "live.error", "message": "GPT-Liveが要求を受け付けませんでした。"})
                stopped.set(); return
        stopped.set()

    async def feed_silence():
        # Live advances on the input-audio clock even for one-way narration.
        # No microphone is captured: 40 ms of silent 24 kHz mono PCM per packet.
        await ready.wait()
        audio = base64.b64encode(bytes(960 * 2)).decode("ascii")
        while not stopped.is_set() and not closed.is_set():
            await send({"type": "session.input_audio.append", "audio": audio})
            await asyncio.sleep(0.04)

    async def receive_unity():
        nonlocal language
        while True:
            e = await client.receive_json()
            if e.get("type") == "cue" and ready.is_set() and not stopped.is_set():
                kind, value = e.get("kind", ""), e.get("value")
                if kind in ("language", "language_hover", "gender") and value in ("ja", "en"):
                    language = value
                text = cue_text(language, kind, value, screen)
                if text:
                    await steer(text)
                    await send({"type": "session.commentary.append", "event_id": uuid.uuid4().hex,
                                "delegation_id": None,
                                "content": "Explain the current cue briefly now, in the requested language, then wait." if language == "en" else "今の合図について、指定された言語で短く説明して待ってください。"})
            elif e.get("type") == "cancel":
                stopped.set(); return

    tasks = []
    try:
        upstream = await websockets.connect(
            "wss://api.openai.com/v1/live/sessions",
            extra_headers={"Authorization": "Bearer " + settings.openai_api_key},
            open_timeout=15, close_timeout=3, max_size=4 * 1024 * 1024)
        await send({"type": "session.start", "event_id": uuid.uuid4().hex, "session": {
            "model": LIVE_MODEL,
            "instructions": narration_prompt(language, screen, phase, gender, switched),
            "audio": {"format": {"type": "audio/pcm", "rate": 24000}, "output": {"voice": guide_voice(gender)}},
            "delegation": {"type": "client"}, "store": False}})
        tasks = [asyncio.create_task(receive_live()), asyncio.create_task(receive_unity()),
                 asyncio.create_task(feed_silence())]
        while not stopped.is_set() and not closed.is_set():
            await asyncio.sleep(.2)
            if any(t.done() for t in tasks):
                for t in tasks:
                    if t.done():
                        t.result()
                break
            if time.monotonic() - began >= MAX_SECONDS:
                close_reason = "time_limit"
                break
    except WebSocketDisconnect:
        close_reason = "client_disconnected"
    except Exception:
        close_reason = "connection_error"
        await emit({"type": "live.error", "message": "GPT-Liveに接続できませんでした。"})
    finally:
        for t in tasks:
            t.cancel()
        await asyncio.gather(*tasks, return_exceptions=True)
        if upstream:
            with contextlib.suppress(Exception):
                await send({"type": "session.close", "event_id": uuid.uuid4().hex})
                await asyncio.wait_for(closed.wait(), timeout=4)
            with contextlib.suppress(Exception):
                await upstream.close()
        log_event(
            "live_title_tour", client,
            client_id=client.query_params.get("client_id"),
            language=language,
            screen=screen,
            close_reason=close_reason,
            duration_seconds=round(time.monotonic() - began, 1),
        )
        with contextlib.suppress(Exception):
            await client.close()
