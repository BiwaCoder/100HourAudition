import json
import os

CHARACTER_PATH = os.path.join(os.path.dirname(__file__), "characters", "astra.json")

QUESTION_TEXT = "君は何が好きだ？"

JSON_FORMAT_EXAMPLE = """{
  "name": "...",
  "jobs": "...",
  "personality": "...",
  "self_awareness": "...",
  "communication_style": ["...", "..."],
  "fashion_sense": {"style": "...", "preferences": "..."},
  "conversation_points": ["...", "..."],
  "traits": ["...", "..."],
  "hobbies": ["...", "..."],
  "emotional_expressions": ["...", "..."],
  "example_lines": ["...", "..."],
  "appearance": {"hair": "...", "eyes": "...", "skin": "...", "body": "..."}
}"""


def load_character(path: str = CHARACTER_PATH) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    return data.get("character", data)


def build_system_message(char: dict) -> str:
    lines = [
        f"あなたは「{char['name']}」というキャラクターです。以下の設定に従って会話してください。",
    ]
    if char.get("jobs"):
        lines += ["", "【職業】", char["jobs"]]
    lines += [
        "",
        "【性格】",
        char["personality"],
        "",
        "【自己認識】",
        char["self_awareness"],
        "",
        "【コミュニケーションスタイル】",
    ]
    lines += [f"- {s}" for s in char["communication_style"]]

    lines += ["", "【特徴】", "、".join(char["traits"])]
    lines += ["", "【趣味】", "、".join(char["hobbies"])]

    lines += ["", "【会話のポイント】"]
    lines += [f"- {p}" for p in char["conversation_points"]]

    fashion = char["fashion_sense"]
    lines += ["", "【ファッション】", f"スタイル: {fashion['style']}", f"好み: {fashion['preferences']}"]

    appearance = char.get("appearance", {})
    lines += ["", "【外見】"]
    for key, label in [("hair", "髪"), ("eyes", "瞳"), ("skin", "肌"),
                        ("body", "体型"), ("posture", "姿勢")]:
        if appearance.get(key):
            lines.append(f"{label}: {appearance[key]}")

    lines += ["", "【感情表現の例】"]
    lines += [f"- {e}" for e in char["emotional_expressions"]]

    lines += ["", "【セリフ例】"]
    lines += [f"- {l}" for l in char["example_lines"]]

    lines += [
        "",
        "上記の設定を踏まえ、一人称視点でキャラクターになりきって自然に会話してください。",
        "地の文や説明は書かず、セリフのみで応答してください。",
    ]

    return "\n".join(lines)


def build_single_question_system_message(interviewer: dict) -> str:
    base = build_system_message(interviewer)
    if interviewer.get("voice_direction"):
        base += "\n\n【声の演技】\n" + interviewer["voice_direction"]
    base += (
        "\n\n【今回のミッション】\n"
        "会話は必ず日本語だけで行います。ユーザーが別の言語で話しても日本語で返してください。\n"
        "アプリが進行を管理します。指定された台詞だけを話し、自分で次の質問へ進まないでください。\n"
        "順序は、あなたから開始案内とマイクチェック、質問1、質問2、キャラクターJSON生成です。\n"
        "マイクチェックでは『私はAI-0、エーアイゼロ。生まれたばかりのAIだ。これから、君の新しいキャラクターを作ろう。まずはマイクチェックだ。何か話してくれ。』と案内します。\n"
        "マイクチェックの発話は人物像の回答に含めません。\n"
        "質問1では好きなものや夢中になっていること、質問2では性格や大切にしていることを聞きます。\n"
        "一度の発話で一つの段階だけを案内し、相手の確認を待ちます。追加質問や地の文は不要です。"
    )
    return base


def build_extraction_prompt(voice_answer: str, photo_impression: str = "", language: str = "ja") -> str:
    exchange = f"AI-0による二つの質問と、確認済みの音声回答:\n{voice_answer}"
    if photo_impression:
        exchange += f"\n相手が提出した写真の印象: {photo_impression}"

    output_language = "Write all human-readable values in English, including a natural character name. Keep JSON keys and structure unchanged." if language == "en" else "相手の回答が英語であっても、name(人名は自然な日本語名に創作)を含む全ての値は必ず日本語で出力してください。"
    return f"""以下は、AI-0が相手にヒアリングをした内容です。
この内容から読み取れる「相手」の性格・好み・雰囲気をもとに、
相手をモデルにした新しいキャラクターを、次のJSON形式にそって作成してください。
情報が少ない部分は、回答から自然に連想できる範囲で補って構いません。
出力はJSONオブジェクトのみとし、説明文やコードブロックの記号は含めないでください。
{output_language}

【フォーマット】
{JSON_FORMAT_EXAMPLE}

【ヒアリング内容】
{exchange}
"""
