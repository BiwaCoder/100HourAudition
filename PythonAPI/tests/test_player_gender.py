import json
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import Mock, patch
from fastapi.testclient import TestClient
from app.main import app
from app.interview_character import generate_profile, Transcript

class PlayerGenderTests(unittest.TestCase):
    def test_selected_gender_reaches_completion_and_introduction(self):
        template=json.loads(Path('app/characters/interview_templates.json').read_text())['characters'][0]
        for gender in ('male','female'):
            reply=SimpleNamespace(choices=[SimpleNamespace(finish_reason='stop',message=SimpleNamespace(refusal=None,content=json.dumps(template)))])
            create=Mock(return_value=reply)
            api=SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=create)))
            with patch('app.interview_character.generate_introduction',return_value='こんにちは。') as intro:
                result=generate_profile(Transcript(turns=[]),client=api,player_gender=gender)
            self.assertEqual(result['characters'][0]['gender'],gender)
            self.assertIn('fictional '+gender,create.call_args.kwargs['messages'][0]['content'])
            self.assertEqual(intro.call_args.args[0]['gender'],gender)

    def test_api_passes_explicit_gender(self):
        with patch('app.interview_character.generate_profile',return_value={'characters':[{'gender':'male','name':'悠真'}]}) as generate:
            response=TestClient(app).post('/api/build-character',json={'voice_answer':'{"turns":[]}','profile_mode':'interview','player_gender':'male'})
        self.assertEqual(response.status_code,200)
        self.assertEqual(generate.call_args.kwargs['player_gender'],'male')
        invalid=TestClient(app).post('/api/build-character',json={'voice_answer':'{}','player_gender':'invalid'})
        self.assertEqual(invalid.status_code,422)
