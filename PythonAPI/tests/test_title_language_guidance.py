import unittest
from app.api.live_title_tour import cue_text, narration_prompt

class TitleLanguageGuidanceTests(unittest.TestCase):
    def test_hover_language_overrides_session_language(self):
        en = cue_text('ja', 'language_hover', 'en')
        ja = cue_text('en', 'language_hover', 'ja')
        self.assertIn('Speak only English', en)
        self.assertIn('button below', en)
        self.assertIn('日本語だけ', ja)
        self.assertIn('下のボタン', ja)

    def test_click_and_hover_give_same_start_guidance(self):
        for code in ('ja', 'en'):
            self.assertEqual(cue_text(code, 'language', code), cue_text(code, 'language_hover', code))

    def test_gender_is_separate_explicit_screen_cue(self):
        self.assertIn('表示されました', cue_text('ja', 'gender', 'ja'))
        self.assertIn('explicit cue', narration_prompt('en', 'title'))
        self.assertNotIn('自由に選べる', narration_prompt('ja', 'title'))

    def test_portrait_language_change_does_not_describe_title(self):
        self.assertNotIn('button below', cue_text('en', 'language', 'en', 'portrait'))
        self.assertIn('Choose photo', narration_prompt('en', 'portrait'))

if __name__ == '__main__':
    unittest.main()
