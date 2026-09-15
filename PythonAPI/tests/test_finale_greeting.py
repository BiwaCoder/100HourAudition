"""Opening lifecycle: a matched acceptance starts speech without user input."""
import asyncio
import json
import unittest
from unittest.mock import patch
from fastapi.testclient import TestClient
from starlette.websockets import WebSocketDisconnect
from app.main import app


class FinaleGreetingTests(unittest.TestCase):
    def test_starts_once_after_matching_ack_without_user_audio(self):
        for language in ('ja', 'en'):
            with self.subTest(language=language):
                sent = []

                class Socket:
                    async def send(self, raw):
                        event = json.loads(raw)
                        sent.append(event)
                        kind = event['type']
                        if kind == 'session.start':
                            await self.events.put(json.dumps({'type': 'session.started'}))
                        elif kind == 'session.instructions.append':
                            # Unrelated and duplicate acknowledgments must not start greetings.
                            for event_id in ('unrelated', event['event_id'], event['event_id']):
                                await self.events.put(json.dumps({'type': 'session.instructions.appended', 'client_event_id': event_id}))
                        elif kind == 'session.commentary.append':
                            await self.events.put(json.dumps({'type': 'session.output_transcript.delta', 'delta': 'opening'}))
                        elif kind == 'session.close':
                            await self.events.put(json.dumps({'type': 'session.closed'}))

                    def __aiter__(self): return self
                    async def __anext__(self): return await self.events.get()
                    async def close(self): pass

                socket = Socket()

                async def connect(*args, **kwargs):
                    socket.events = asyncio.Queue()
                    return socket

                with patch('app.api.live_finale.websockets.connect', connect):
                    with TestClient(app).websocket_connect('/api/live-finale?gender=female&partner_name=Hikari&player_name=Yuma&language=' + language) as ws:
                        self.assertEqual(ws.receive_json()['type'], 'live.ready')
                        self.assertEqual(ws.receive_json()['delta'], 'opening')
                        ws.send_json({'type': 'cancel'})
                        try:
                            while ws.receive_json()['type'] != 'live.finish': pass
                        except WebSocketDisconnect:
                            pass
                greetings = [e for e in sent if e['type'] == 'session.commentary.append']
                self.assertEqual(len(greetings), 1)
                self.assertIsNone(greetings[0]['delegation_id'])
                self.assertIn('English' if language == 'en' else '日本語', greetings[0]['content'])
                self.assertNotIn('session.input_audio.append', [e['type'] for e in sent])
                self.assertEqual(sent[-1]['type'], 'session.close')
