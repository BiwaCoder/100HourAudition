import asyncio
import base64
import json
import unittest
from unittest.mock import patch
from fastapi.testclient import TestClient
from starlette.websockets import WebSocketDisconnect
from app.main import app
from app.live_interview import Interview, session_config, MAX_SECONDS

class LiveInterviewTests(unittest.TestCase):
    def test_stage_order_and_no_transition_while_speaking(self):
        flow=Interview()
        flow.record('user','はじめまして',100,400)
        self.assertFalse(flow.should_advance(10,False))
        self.assertTrue(flow.should_advance(10,True))
        for t in (10,25,45):flow.advance(t)
        self.assertEqual(flow.transitions[-1][1],'likes')
        self.assertTrue(flow.can_create())
        flow.record('user','海を眺めるのが好き',45000,47000)
        flow.assessments['likes']={'enough':True}
        self.assertTrue(flow.should_advance(58,True))
        self.assertTrue(flow.can_create())
        self.assertIn('海を眺めるのが好き',flow.build_input())
        self.assertNotIn('assistant',flow.build_input())
        self.assertEqual(MAX_SECONDS,120)

    def test_late_transcript_keeps_original_stage(self):
        flow = Interview()
        flow.advance(20)
        flow.record('user', 'はじめまして', 18000, 19000)
        self.assertEqual(flow.user_text('greeting'), 'はじめまして')
        self.assertEqual(flow.user_text('show'), '')

    def test_deadline_reserves_closing_explanation(self):
        flow=Interview()
        self.assertTrue(flow.begin_closing(108))
        self.assertEqual(flow.stage,5)
        self.assertEqual(flow.entered,108)
        self.assertFalse(flow.begin_closing(110))
        self.assertFalse(flow.should_advance(119,True))
        self.assertEqual(MAX_SECONDS-108,12)

    def test_english_prompt_and_json_language(self):
        from app.live_interview import stage_prompt, stage_label
        from app.character_prompts import build_extraction_prompt
        from app.schemas.realtime import BuildCharacterRequest
        self.assertIn("Speak English", session_config("en")["instructions"])
        for stage in range(6):
            self.assertTrue(stage_prompt("en",stage).isascii())
            self.assertTrue(stage_label("en",stage).isascii())
        prompt=build_extraction_prompt("I enjoy the ocean", language="en")
        self.assertIn("values in English",prompt)
        self.assertNotIn("必ず日本語で出力",prompt)
        self.assertEqual(BuildCharacterRequest(voice_answer="test").language,"ja")
        with self.assertRaises(ValueError): BuildCharacterRequest(voice_answer="test",language="xx")

    def test_live_contract_and_no_realtime_voice_loop(self):
        config=session_config()
        self.assertEqual(config['model'],'gpt-live-1')
        self.assertEqual(config['audio']['format']['rate'],24000)
        self.assertEqual(config['audio']['output']['voice'],'cedar')
        self.assertEqual(config['delegation']['type'],'client')
        self.assertNotIn('turn_detection',config['audio'])

    def test_relay_start_and_cancel_closes_upstream(self):
        sent=[]
        class Socket:
            def __init__(self):self.events=None;self.closed=False
            async def send(self,raw):
                event=json.loads(raw);sent.append(event)
                if event['type']=='session.start':await self.events.put(json.dumps({'type':'session.started','session':{'id':'test'}}))
                if event['type']=='session.close':await self.events.put(json.dumps({'type':'session.closed','usage':{'seconds':1}}))
            def __aiter__(self):return self
            async def __anext__(self):return await self.events.get()
            async def close(self):self.closed=True
        socket=Socket()
        async def connect(*args,**kwargs):
            socket.events=asyncio.Queue()
            return socket
        with patch('app.api.live.websockets.connect',connect):
            with TestClient(app).websocket_connect('/api/live-interview?language=en') as ws:
                self.assertEqual(ws.receive_json()['type'],'live.ready')
                self.assertEqual(ws.receive_json()['label'],'First encounter')
                ws.send_json({'type':'audio','audio':base64.b64encode(b'\0'*4800).decode(),'playback_pending':False})
                ws.send_json({'type':'cancel'})
                # Server must complete cleanup before closing the client transport.
                try:
                    reply=ws.receive_json()
                    self.assertNotEqual(reply.get("type"), "live.error", reply)
                except WebSocketDisconnect:pass
        self.assertIn('Speak English', sent[0]['session']['instructions'])
        self.assertTrue(socket.closed)
        self.assertIn('session.close',[e['type'] for e in sent])
        self.assertIn('session.input_audio.append',[e['type'] for e in sent])

class AuditionPurposeTests(unittest.TestCase):
    def test_first_prompt_contains_purpose_warp_and_personal_question(self):
        from app.live_interview import stage_prompt
        for language,words in [('ja',['リアリティーショー','審査','デジタル空間','ワープ','夢中']),('en',['reality show','audition','digital world','warp','into lately'])]:
            prompt=stage_prompt(language,0)
            for word in words:self.assertIn(word,prompt)

    def test_tap_does_not_satisfy_briefing_or_question(self):
        f=Interview(stage=3)
        f.topic_selections.append({'title':'写真','kind':'topic'})
        self.assertTrue(f.ensure_question(50))
        self.assertIn('審査',f.checkpoint_prompt())
        self.assertIn('今一つ質問',f.checkpoint_prompt())

    def test_spoken_opening_avoids_repeating_checkpoint(self):
        f=Interview()
        for i,word in enumerate(['リアリティー','ショーの審査。デジタル空間へワープする。','最近、何に夢中？']):
            f.record('assistant',word,i*100,100+i*100)
        self.assertEqual(f.opening_progress(),(True,True))
        self.assertFalse(f.ensure_question(50))

    def test_user_cannot_fulfill_host_briefing_and_partial_is_repaired(self):
        f=Interview()
        f.record('user','リアリティーショーの審査。デジタルへワープ。何が好き？',0,10)
        f.record('assistant','何が好き？',10,20)
        self.assertEqual(f.opening_progress(),(False,True))
        self.assertTrue(f.ensure_question(50))
        self.assertIn('人物像への質問は発話済み',f.checkpoint_prompt())
        self.assertIn('今伝える',f.checkpoint_prompt())

if __name__=='__main__':unittest.main()
