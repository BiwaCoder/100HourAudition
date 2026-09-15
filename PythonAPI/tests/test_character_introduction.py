import json
from types import SimpleNamespace
from unittest.mock import Mock
from app.interview_character import generate_introduction


import unittest


class IntroductionTests(unittest.TestCase):
    def test_introduction_uses_final_settings_and_selected_language(self):
        character = {'name': '栞', 'hobbies': ['古本探し'], 'communication_style': ['控えめな敬語']}
        create = Mock(return_value=SimpleNamespace(choices=[SimpleNamespace(
            finish_reason='stop', message=SimpleNamespace(content='はじめまして、栞です。古本屋を巡るのが好きです。', refusal=None))]))
        api = SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=create)))
        result = generate_introduction(character, client=api)
        assert '栞' in result
        messages = create.call_args.kwargs['messages']
        assert json.loads(messages[1]['content']) == character
        assert '日本語' in messages[0]['content']
        generate_introduction(character, 'en', api)
        assert 'Use English' in create.call_args.kwargs['messages'][0]['content']


    def test_timeout_preserves_setting_based_greeting(self):
        api = SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=Mock(side_effect=TimeoutError))))
        for language in ('ja', 'en'):
            text = generate_introduction({'name': 'Shiori', 'hobbies': ['reading']}, language, api)
            assert 'Shiori' in text and 'reading' in text
            assert 'TimeoutError' not in text
