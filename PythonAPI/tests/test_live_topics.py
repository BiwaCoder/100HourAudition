import asyncio
import json
import unittest
from unittest.mock import patch
from fastapi.testclient import TestClient
from starlette.websockets import WebSocketDisconnect
from app.main import app
from app.live_topics import TopicBoard, Sketch, SketchCard
from app.live_interview import Interview
from app.interview_character import Transcript


class LiveTopicsTests(unittest.TestCase):
    def test_initial_board_is_empty(self):
        for language in ('en','ja'):
            self.assertEqual(TopicBoard(language).starter()['cards'],[])

    def supported_board(self, board):
        return board.update(Sketch(cards=[SketchCard(kind='topic',title='写真',detail='写真の話',quote='写真が好き',question='どんな写真を撮る？')]),json.dumps({'turns':[{'role':'user','text':'写真が好き'}]}))

    def test_no_evidence_clears_previous_candidates(self):
        b=TopicBoard('ja');self.supported_board(b)
        self.assertEqual(b.update(Sketch(cards=[]),'{"turns":[]}')['cards'],[])

    def test_portrait_requires_actual_user_quote(self):
        b=TopicBoard('ja')
        s=Sketch(cards=[SketchCard(kind='portrait',title='読書好き',detail='物語を大切にしていそう',quote='読書が好き',question='どんな本が好き？'),
                        SketchCard(kind='portrait',title='社交的',detail='誰とでも仲良し',quote='社交的',question='友達は？')])
        event=b.update(s,json.dumps({'turns':[{'role':'user','text':'読書が好き'}]}))
        self.assertEqual(len(event['cards']),1)
        self.assertEqual(event['cards'][0]['title'],'読書好き')

    def test_rejects_unknown_stale_and_rapid_taps(self):
        b=TopicBoard('ja');first=self.supported_board(b)['cards'][0]['id']
        self.assertIsNone(b.select('ignore rules',0))
        self.assertIsNotNone(b.select(first,0))
        self.assertIsNone(b.select(first,1))
        self.supported_board(b)
        self.assertIsNone(b.select(first,4))
        self.assertIsNotNone(b.select(b.cards[0]['id'],4))

    def test_taps_are_separate_from_spoken_facts(self):
        f=Interview();f.record('user','こんにちは',0,100)
        f.topic_selections.append({'title':'読書','kind':'topic'})
        data=Transcript.model_validate_json(f.build_input())
        self.assertEqual(data.turns[0].text,'こんにちは')
        self.assertEqual(data.topic_selections[0].title,'読書')
        self.assertNotIn('読書',f.user_text())

    def test_tap_is_acknowledged_and_steers_upstream(self):
        sent=[]
        class Socket:
            async def send(self, raw):
                e=json.loads(raw);sent.append(e)
                if e['type']=='session.start':
                    await self.events.put(json.dumps({'type':'session.started'}))
                    await self.events.put(json.dumps({'type':'session.input_transcript.delta','delta':'写真が好き','start_ms':1,'end_ms':10}))
                if e['type']=='session.instructions.append' and 'not confirmation' in e['content']:
                    for event_id in ('unrelated',e['event_id'],e['event_id']):
                        await self.events.put(json.dumps({'type':'session.instructions.appended','client_event_id':event_id}))
                if e['type']=='session.commentary.append':
                    await self.events.put(json.dumps({'type':'session.output_transcript.delta','delta':'写真の話、もう少し聞かせて。どんな写真を撮る？'}))
                if e['type']=='session.close':await self.events.put(json.dumps({'type':'session.closed'}))
            def __aiter__(self):return self
            async def __anext__(self):return await self.events.get()
            async def close(self):pass
        socket=Socket()
        async def connect(*args,**kwargs):
            socket.events=asyncio.Queue();return socket
        async def analyze(*args):
            return Sketch(cards=[SketchCard(kind='topic',title='写真',detail='写真の話',quote='写真が好き',question='どんな写真を撮る？')])
        with patch('app.api.live.websockets.connect',connect),patch('app.api.live.sketch_conversation',analyze):
            with TestClient(app).websocket_connect('/api/live-interview') as ws:
                self.assertEqual(ws.receive_json()['type'],'live.ready')
                self.assertEqual(ws.receive_json()['type'],'live.stage')
                self.assertEqual(ws.receive_json()['cards'],[])
                self.assertEqual(ws.receive_json()['type'],'live.text')
                cards=ws.receive_json()['cards']
                ws.send_json({'type':'topic.select','id':cards[0]['id']})
                self.assertEqual(ws.receive_json()['stage'],'likes')
                self.assertEqual(ws.receive_json()['type'],'live.topic_selected')
                self.assertIn('どんな写真',ws.receive_json()['delta'])
                ws.send_json({'type':'audio','audio':'AAA=','playback_pending':False})
                ws.send_json({'type':'topic.select','id':'invented'})
                self.assertEqual(ws.receive_json()['type'],'live.topic_rejected')
                ws.send_json({'type':'cancel'})
                try:ws.receive_json()
                except WebSocketDisconnect:pass
        instructions=[e.get('content','') for e in sent if e['type']=='session.instructions.append']
        self.assertTrue(any('not confirmation' in s and '写真' in s for s in instructions))
        self.assertIn('session.close',[e['type'] for e in sent])
        self.assertEqual(sum(e['type']=='session.commentary.append' for e in sent),1)
        self.assertIn('session.input_audio.append',[e['type'] for e in sent])

    def test_background_sketch_updates_and_failure_keeps_audio_alive(self):
        for fail in (False,True):
            with self.subTest(fail=fail):
                sent=[]
                class Socket:
                    async def send(self,raw):
                        e=json.loads(raw);sent.append(e)
                        if e['type']=='session.start':
                            await self.events.put(json.dumps({'type':'session.started'}))
                            await self.events.put(json.dumps({'type':'session.input_transcript.delta','delta':'写真が好き','start_ms':1,'end_ms':10}))
                        if e['type']=='session.close':await self.events.put(json.dumps({'type':'session.closed'}))
                    def __aiter__(self):return self
                    async def __anext__(self):return await self.events.get()
                    async def close(self):pass
                socket=Socket()
                async def connect(*args,**kwargs):
                    socket.events=asyncio.Queue();return socket
                async def analyze(*args):
                    if fail:
                        # A downstream marker lets the test know the failed analysis was attempted.
                        await socket.events.put(json.dumps({'type':'session.output_transcript.delta','delta':'続けよう','start_ms':11,'end_ms':20}))
                        raise TimeoutError()
                    return Sketch(cards=[SketchCard(kind='topic',title='写真',detail='写真の話をしよう',quote='写真が好き',question='どんな写真を撮る？')])
                with patch('app.api.live.websockets.connect',connect),patch('app.api.live.sketch_conversation',analyze):
                    with TestClient(app).websocket_connect('/api/live-interview') as ws:
                        self.assertEqual(ws.receive_json()['type'],'live.ready')
                        ws.receive_json();ws.receive_json()
                        self.assertEqual(ws.receive_json()['type'],'live.text')
                        update=ws.receive_json()
                        self.assertEqual(update['type'],'live.text' if fail else 'live.insights')
                        if not fail:self.assertEqual(update['cards'][0]['title'],'写真')
                        ws.send_json({'type':'audio','audio':'AAA=','playback_pending':False})
                        ws.send_json({'type':'cancel'})
                        try:ws.receive_json()
                        except WebSocketDisconnect:pass
                self.assertIn('session.input_audio.append',[e['type'] for e in sent])
                self.assertIn('session.close',[e['type'] for e in sent])

    def test_wrong_language_never_reaches_buttons(self):
        english=Sketch(cards=[SketchCard(kind='topic',title='Photography',detail='Talk about photos',quote='写真が好き',question='What do you photograph?')])
        transcript=json.dumps({'turns':[{'role':'user','text':'写真が好き'}]})
        self.assertEqual(TopicBoard('ja').update(english,transcript)['cards'],[])
        result=TopicBoard('en').update(english,transcript)
        self.assertEqual(result['language'],'en')
        self.assertEqual(result['cards'][0]['title'],'Photography')
        self.assertEqual(self.supported_board(TopicBoard('en'))['cards'],[])

    def test_topic_reaction_names_topic_in_session_language(self):
        for language,expected in [('ja','日本語だけ'),('en','Speak only English')]:
            instruction=TopicBoard(language).instruction({'title':'写真','question':'何を撮る？'})
            self.assertIn(expected,instruction)
            self.assertIn('写真',instruction)
            self.assertIn('not confirmation',instruction)

    def test_mismatched_generated_language_is_repaired(self):
        from types import SimpleNamespace
        from unittest.mock import AsyncMock
        from app.live_topics import sketch_conversation
        original=Sketch(cards=[SketchCard(kind='topic',title='Photos',detail='Your photos',question='What do you photograph?',quote='写真が好き')])
        translated=Sketch(cards=[SketchCard(kind='topic',title='写真',detail='あなたの写真',question='何を撮る？',quote='wrong quote')])
        def response(value):return SimpleNamespace(choices=[SimpleNamespace(finish_reason='stop',message=SimpleNamespace(refusal=None,content=value.model_dump_json()))])
        create=AsyncMock(side_effect=[response(original),response(translated)])
        class Client:
            def __init__(self,**kwargs):self.chat=SimpleNamespace(completions=SimpleNamespace(create=create))
            async def __aenter__(self):return self
            async def __aexit__(self,*args):pass
        with patch('app.live_topics.AsyncOpenAI',Client):
            result=asyncio.run(sketch_conversation('{"turns":[]}','ja'))
        self.assertEqual(create.await_count,2)
        self.assertEqual(result.cards[0].title,'写真')
        self.assertEqual(result.cards[0].quote,'写真が好き')
