# 100 Hour Audition

**An AI-native reality dating show starring *another you*, born from your voice and photos.**

Built for the Tokyo AI (TAI) × OpenAI 100-Hour Game Builder Challenge.

---

## Project Overview

**"100 Hour Audition"** is an AI-native reality dating show starring *another you*, born from your voice and photos. It's for anyone who wants the thrill of flirting — and wants to get better at it.

In daily life, you aren't always the main character. Love is the one place where you truly get to be the protagonist, yet there is nowhere to practice it. So we built a show where you can.

Here's how it works: `gpt-live-1` interviews you by voice about your personality and tastes, and `gpt-image-1` turns your photo into an in-world portrait — together, *another you* is born. Inside the show, you meet an AI cast with their own desires and secrets. Each scene, you choose your approach like playing a card: a joke, a question, a small confession. The cast remembers every word, so one line in your first introduction can change the final confession. No script. Every run becomes a story that is yours alone.

Watching your other self blush, stumble, and finally speak up is both entertainment and rehearsal. Don't study love — play with it. And before you know it, tomorrow feels a little more exciting.

## How It's Built

The project is two codebases in one repository:

| Path | What it is | Stack |
|---|---|---|
| `Assets/` | The Unity game client (desktop app + WebGL browser build) | Unity 6000.3.19f1, C#, TextMesh Pro, Cinemachine, new Input System |
| `PythonAPI/` | The backend that talks to OpenAI on the client's behalf | FastAPI, `openai` SDK, `websockets`, deployed with uvicorn behind nginx |

```
Unity client (desktop or WebGL)
        │  HTTPS / WSS (HMAC-signed requests)
        ▼
PythonAPI (FastAPI, uvicorn)
        │
        ├── app/api/chat.py            → Chat Completions (NPC dialogue, memory-driven topics)
        ├── app/api/live.py            → gpt-live-1 relay (in-story interview / voice conversation)
        ├── app/api/live_title_tour.py → gpt-live-1 relay (voice tutorial on the title/menu screens)
        ├── app/api/photo.py           → gpt-image-1 (photo → in-world portrait)
        ├── app/api/realtime.py        → ephemeral token issuance for the legacy Realtime voice path
        └── app/interview_character.py → turns the spoken interview into a structured character sheet
        │
        ▼
   OpenAI API (gpt-live-1, gpt-image-1, Chat Completions)
```

### Unity client (`Assets/`)

- `Assets/Scripts/AuditionEntry/` — title screen, language/gender selection, voice-guided character creation flow
- `Assets/Scripts/RealityShow/` — the core show/conversation rules engine (dialogue, memory, relationship stages, rival AI)
- `Assets/Scripts/Environments/SeasideMansion/` — the main show scene director and staging
- `Assets/Scripts/LiveInterview/`, `Assets/Scripts/RealtimeVoice/` — voice session handling for the two voice pipelines (`gpt-live-1` relay and the legacy Realtime API path)
- `Assets/Scripts/Net/` — API endpoint config, HMAC request signing, and `PlatformSocket` — a small cross-platform WebSocket abstraction (`ClientWebSocket` on Standalone/Editor, the browser's native `WebSocket` via a `.jslib` bridge on WebGL, since `System.Net.WebSockets` doesn't exist in WebGL builds)
- `Assets/Plugins/WebGL/` — the `.jslib` bridges that make WebSocket, microphone capture (`getUserMedia`), and low-latency PCM voice playback work inside a browser, since none of `System.Net.WebSockets`, `UnityEngine.Microphone`, or `OnAudioFilterRead` are available there
- `Assets/WebGLTemplates/HundredHour/` — the WebGL page template: a real user-gesture gate to satisfy browser autoplay policy before voice starts, plus a shader-invariance fix for a WebGL-only rendering artifact

### Backend (`PythonAPI/`)

- `app/main.py` — FastAPI app, routes everything above
- `app/security.py` — timestamp + nonce + HMAC-SHA256 request signing (`X-Auth-*` headers for REST, `auth_ts`/`auth_nonce`/`auth_sig` query params for WebSockets, since browsers can't set custom headers on a WebSocket handshake)
- `app/live_interview.py` — stage-driven interview prompts (JA/EN), gender-aware guide voice/tone
- `app/telemetry.py` — anonymous usage logging, written outside the deployed directory so redeploys never touch it
- `tests/` — unit tests for the interview flow, live-voice guidance, and conversation balance

## Running It Locally

### Unity client

1. Open the project root in Unity `6000.3.19f1` (or later).
2. Point `Assets/Resources/ApiEndpointConfig.asset` at your PythonAPI instance (`localHost` for local dev, or toggle `useCloud` for the deployed one). WebGL builds always use the cloud host, since a browser can't reach `127.0.0.1` on the developer's machine.
3. Press Play from `Assets/Scenes/Audition/AuditionTitle.unity`.

### PythonAPI

```bash
cd PythonAPI
python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt

export OPENAI_API_KEY=sk-...
python run.py   # uvicorn app.main:app --host 0.0.0.0 --port 8001
```

`API_HMAC_SECRET` is optional for local development — when unset, request signing is a no-op on both client and server (same policy used in production, just with the secret set).

## Deployment

The live build runs on a single Sakura Cloud VPS, both behind nginx with TLS via Let's Encrypt on private subdomains not listed here (kept unpublished to avoid inviting uncontrolled traffic against a paid API budget):

- The WebGL build is served as static files.
- `PythonAPI` runs as a systemd service (`hundredhour-api.service`, uvicorn).

Both server and client are redeployed with `rsync` + a service restart; see `Docs/` for the detailed runbooks written during development (`Docs/WebGLAudio/`, `Docs/WebGLNoise/` cover the two trickiest WebGL fixes: browser autoplay unlocking and a shader-invariance rendering bug).

## Assets & Licensing

Built with Unity Engine, TextMesh Pro, Cinemachine, and Input System (all official Unity packages). OpenAI APIs (Realtime API, Chat Completions, Images API) are used in accordance with OpenAI's Terms of Use.

The project uses Unity Asset Store packages — Funly Sky Studio, Stylizer, Low Poly Vegetation Pack, and Joystick Pack — under their respective licenses (Unity Asset Store EULA). They are not included in this repository; see the section below.

Music, 3D models, and illustrations were created with AI coding agents and generative tools (Codex, Claude Code, Blender, and gpt-image-1).

### Fonts
All bundled fonts are released under the SIL Open Font License 1.1; each license text sits next to the font file.

| Font | Used for | Source |
|---|---|---|
| Berkshire Swash | Title logo | Google Fonts (Astigmatic) |
| Noto Sans CJK JP | Japanese body text | Google / Adobe Noto CJK release |
| Noto Serif | Headings | Google Fonts (Noto Project) |
| Liberation Sans | TextMesh Pro default | Bundled with TextMesh Pro (Red Hat) |

## Not Included in This Repository

### Asset Store packages (not redistributable under their licenses)
Install the following from the Unity Asset Store yourself and place them directly under `Assets/` with the same folder names.

| Folder | Package |
|---|---|
| `Assets/FunlySkyStudio` | Sky Studio (Funly) |
| `Assets/Stylizer` | Stylizer |
| `Assets/Low Poly Vegetation Pack` | Low Poly Vegetation Pack |
| `Assets/Joystick Pack` | Joystick Pack |

### API shared secret
Put the same value as the server's `API_HMAC_SECRET` on a single line in `Assets/Resources/ApiSecret.local.txt` (git-ignored) and the client will sign its requests. If the file is missing, requests are sent unsigned, which still works against a server that has no secret configured (local development).

## Why This Repository Has So Few Commits
This repository started as a history-free snapshot of the private development repository. The original commit history contained paid Unity Asset Store packages (which cannot be redistributed) and an API shared secret that was serialized into a config asset. The secret has since been rotated and the packages removed from version control, but rather than rewrite and republish that history, the public repository began from a clean snapshot of the source. Development continues in the private repository, and updates are pushed here as periodic snapshots.
