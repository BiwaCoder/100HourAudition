import asyncio
import json
import unittest
from types import SimpleNamespace
from unittest.mock import patch
from app.api import realtime
from app.character_prompts import build_single_question_system_message, load_character, build_extraction_prompt
from app.schemas.realtime import BuildCharacterRequest

class RealtimeUxTests(unittest.TestCase):
    def test_japanese_and_stage_owned_instructions(self):
        prompt=build_single_question_system_message(load_character())
        self.assertIn('日本語だけ',prompt)
        self.assertIn('マイクチェックの発話は人物像の回答に含めません',prompt)
        self.assertIn('自分で次の質問へ進まない',prompt)

    def test_token_payload_uses_manual_turns_and_japanese(self):
        captured={}
        class Client:
            async def __aenter__(self): return self
            async def __aexit__(self,*args): pass
            async def post(self,url,**kwargs):
                captured.update(kwargs['json'])
                return SimpleNamespace(raise_for_status=lambda:None,json=lambda:{'value':'TEST_EPHEMERAL'})
        with patch.object(realtime.httpx,'AsyncClient',Client):
            result=asyncio.run(realtime.realtime_token())
        self.assertTrue(result.success)
        self.assertEqual(result.interviewer_name, 'AI-0')
        self.assertEqual(captured['session']['audio']['output']['voice'], 'cedar')
        self.assertIn('生まれたばかりのAI案内役AI-0（エーアイゼロ）', captured['session']['instructions'])
        self.assertIn('成熟した男性', captured['session']['instructions'])
        audio=captured['session']['audio']['input']
        self.assertEqual(audio['transcription']['language'],'ja')
        self.assertFalse(audio['turn_detection']['create_response'])
        self.assertFalse(audio['turn_detection']['interrupt_response'])
        self.assertEqual(audio['turn_detection']['silence_duration_ms'],900)

    def test_two_answer_extraction_preserves_japanese(self):
        answers='質問1: 好きなもの\n回答1: 絵本\n質問2: 性格\n回答2: 穏やかで誠実'
        prompt=build_extraction_prompt(answers)
        self.assertIn(answers,prompt)
        self.assertIn('全ての値は必ず日本語',prompt)
        class LLM:
            def __init__(self,**kwargs):
                self.client=SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=self.create)))
            def create(self,**kwargs):
                self_outer.assertIn(answers,kwargs['messages'][1]['content'])
                return SimpleNamespace(choices=[SimpleNamespace(message=SimpleNamespace(content=json.dumps({'name':'花音','personality':'穏やかで誠実'})))])
        self_outer=self
        with patch.object(realtime,'LLMInterface',LLM):
            result=realtime.build_character(BuildCharacterRequest(voice_answer=answers))
        self.assertTrue(result.success)
        self.assertEqual(result.character['name'],'花音')

if __name__=='__main__': unittest.main()
