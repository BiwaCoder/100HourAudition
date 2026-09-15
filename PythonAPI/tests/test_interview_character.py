import asyncio
import json
import unittest
from types import SimpleNamespace
from unittest.mock import Mock, patch
from fastapi.testclient import TestClient
from app.main import app
from app.interview_character import Transcript, Turn, Profile, Evidence, empty_profile, extract_profile as generate_profile, ground
from app.live_interview import Interview


def transcript(text):
    return Transcript(turns=[Turn(role="user", text=text, stage="greeting")])


def response(profile, finish="stop", refusal=None):
    return SimpleNamespace(choices=[SimpleNamespace(finish_reason=finish,
        message=SimpleNamespace(content=profile.model_dump_json(), refusal=refusal))])


def client(*responses):
    return SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=Mock(side_effect=responses))))


class InterviewCharacterTests(unittest.TestCase):
    def test_silence_produces_unknown_profile_without_api(self):
        api = client()
        result = generate_profile(Transcript(turns=[]), client=api)
        self.assertEqual(result['profile_quality']['status'], 'no_speech')
        self.assertIsNone(result['characters'][0]['personality'])
        self.assertEqual(result['characters'][0]['traits'], [])
        api.chat.completions.create.assert_not_called()

    def test_empty_interview_http_contract(self):
        with patch('app.interview_character.generate_profile', generate_profile):
            result = TestClient(app).post('/api/build-character', json={
                'profile_mode': 'interview', 'voice_answer': '{"turns":[]}', 'language': 'en'}).json()
        self.assertTrue(result['success'])
        self.assertEqual(len(result['character']['characters']), 1)
        self.assertIn('No user speech',result['character']['profile_quality']['observations'][0])

    def test_malformed_transcript_is_not_success(self):
        result = TestClient(app).post('/api/build-character', json={
            'profile_mode':'interview','voice_answer':'not a transcript'}).json()
        self.assertFalse(result['success'])

    def test_context_and_fragment_joining(self):
        flow = Interview()
        flow.record('assistant','読書が好きですか？',0,20)
        flow.record('user','は',30,40)
        flow.record('user','い',40,50)
        data = Transcript.model_validate_json(flow.build_input())
        self.assertEqual(data.turns[0].role, 'assistant')
        self.assertEqual(data.turns[1].text, 'はい')

    def test_invalid_citations_and_inferred_job_removed(self):
        p = empty_profile()
        p.character.name = '架空名'
        p.character.jobs = 'エンジニア'
        p.character.hobbies = ['ゲーム']
        p.evidence = [Evidence(field='name', basis='stated', quote='架空名', explanation=''),
                      Evidence(field='jobs', basis='inferred', quote='ゲーム', explanation=''),
                      Evidence(field='hobbies', basis='stated', quote='ゲームが好き', explanation='')]
        result = ground(p, transcript('ゲームが好き').turns)
        self.assertIsNone(result.character.name)
        self.assertIsNone(result.character.jobs)
        self.assertEqual(result.character.hobbies,['ゲーム'])

    def test_two_pass_review_uses_original_context_and_strict_schema(self):
        p = empty_profile()
        p.character.hobbies = ['釣り']
        p.evidence = [Evidence(field='hobbies', basis='stated', quote='釣りが好き', explanation='本人の発言')]
        api = client(response(p), response(p))
        result = generate_profile(transcript('釣りが好き'), client=api)
        self.assertEqual(result['profile_quality']['status'], 'reviewed')
        self.assertEqual(api.chat.completions.create.call_count,2)
        kwargs=api.chat.completions.create.call_args.kwargs
        self.assertTrue(kwargs['response_format']['json_schema']['strict'])
        self.assertIn('釣りが好き',kwargs['messages'][1]['content'])
        self.assertIn('draft',kwargs['messages'][-1]['content'])

    def test_review_timeout_preserves_draft(self):
        result=generate_profile(transcript('答えたくない'),client=client(response(empty_profile()),TimeoutError()))
        self.assertEqual(result['profile_quality']['status'],'draft')
        self.assertTrue(result['profile_quality']['limitations'])

    def test_refusal_or_truncation_cannot_be_successful_review(self):
        for reply in (response(empty_profile(),finish='length'),response(empty_profile(),refusal='no')):
            result=generate_profile(transcript('こんにちは'),client=client(reply))
            self.assertEqual(result['profile_quality']['status'],'generation_unavailable')
            self.assertIsNone(result['characters'][0]['personality'])

    def test_schema_has_no_optional_missing_properties(self):
        def check(node):
            if isinstance(node,dict):
                if node.get('type')=='object':
                    self.assertFalse(node['additionalProperties'])
                    self.assertEqual(set(node['properties']),set(node['required']))
                for value in node.values():check(value)
            elif isinstance(node,list):
                for value in node:check(value)
        check(Profile.model_json_schema())

    def test_silent_session_deadline_emits_finish_and_closes(self):
        sent=[]
        class Socket:
            async def send(self, raw):
                event=json.loads(raw);sent.append(event['type'])
                if event['type']=='session.start':
                    await self.events.put(json.dumps({'type':'session.started'}))
                elif event['type']=='session.close':
                    await self.events.put(json.dumps({'type':'session.closed'}))
            def __aiter__(self):return self
            async def __anext__(self):return await self.events.get()
            async def close(self):pass
        socket=Socket()
        async def connect(*args,**kwargs):
            socket.events=asyncio.Queue()
            return socket
        with patch('app.api.live.websockets.connect',connect), patch('app.api.live.MAX_SECONDS',0):
            with TestClient(app).websocket_connect('/api/live-interview') as ws:
                self.assertEqual(ws.receive_json()['type'],'live.ready')
                self.assertEqual(ws.receive_json()['type'],'live.stage')
                self.assertEqual(ws.receive_json()['type'],'live.insights')
                result=ws.receive_json()
                self.assertEqual(result['type'],'live.finish')
                self.assertEqual(json.loads(result['input']),{'turns':[]})
        self.assertIn('session.close',sent)

    def test_checkpoint_ignores_continuous_speaking(self):
        flow=Interview()
        self.assertFalse(flow.ensure_question(49))
        self.assertFalse(flow.should_advance(60,False))
        self.assertTrue(flow.ensure_question(50))
        self.assertEqual(flow.stage,3)
        self.assertFalse(flow.ensure_question(61))

    def test_checkpoint_also_prompts_when_already_in_question_stage(self):
        flow=Interview(stage=3)
        self.assertTrue(flow.ensure_question(61))
        self.assertEqual(flow.stage,3)

    def test_final_completion_is_full_and_preserves_facts(self):
        from app.interview_character import generate_profile as final, require_complete
        p=empty_profile()
        p.character.jobs='司書'
        p.evidence=[Evidence(field='jobs',basis='stated',quote='司書です',explanation='本人の発言')]
        complete=p.character.model_dump()
        def fill(obj):
            for k,v in obj.items():
                if isinstance(v,dict): fill(v)
                elif isinstance(v,list):obj[k]=['ゲーム設定']
                else:obj[k]='ゲーム設定'
        fill(complete)
        reply=SimpleNamespace(choices=[SimpleNamespace(finish_reason='stop',message=SimpleNamespace(refusal=None,content=json.dumps(complete)))])
        result=final(transcript('司書です'),client=client(response(p),response(p),reply))
        require_complete(result['characters'][0])
        self.assertEqual(result['characters'][0]['jobs'],'司書')
        self.assertNotIn('jobs',result['profile_quality']['supplemented_fields'])
        self.assertIn('appearance.hair',result['profile_quality']['supplemented_fields'])

    def test_silent_final_still_calls_completion_api_and_rejects_empty(self):
        from app.interview_character import generate_profile as final
        api=client(response(empty_profile()))
        with self.assertRaises(ValueError): final(Transcript(turns=[]),client=api)
        self.assertEqual(api.chat.completions.create.call_count,1)

    def test_gap_text_is_replaced_and_diagnostics_do_not_leak(self):
        from app.interview_character import generate_profile as final, has_gap
        p=empty_profile()
        p.character.jobs='不明'
        p.evidence=[Evidence(field='jobs',basis='stated',quote='不明',explanation='不明')]
        p.limitations=['外見については記録できなかった。']
        p.observations=['職業はわからない']
        data=p.character.model_dump()
        def fill(obj):
            for k,v in obj.items():
                if isinstance(v,dict):fill(v)
                else:obj[k]=['冒険が好き'] if isinstance(v,list) else '旅の案内人'
        fill(data)
        def reply(obj):
            return SimpleNamespace(choices=[SimpleNamespace(finish_reason='stop',message=SimpleNamespace(refusal=None,content=json.dumps(obj)))])
        bad=dict(data,jobs='記録できない')
        api=client(response(p),response(p),reply(bad),reply(data))
        result=final(transcript('不明'),client=api)
        self.assertEqual(api.chat.completions.create.call_count,5)  # Final settings, then introduction.
        self.assertTrue(result['introduction'])
        self.assertEqual(result['characters'][0]['jobs'],'旅の案内人')
        self.assertIn('jobs',result['profile_quality']['supplemented_fields'])
        self.assertNotIn('limitations',result['profile_quality'])
        self.assertFalse(has_gap(json.dumps(result,ensure_ascii=False)))
