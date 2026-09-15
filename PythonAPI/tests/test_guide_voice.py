import unittest
from app.live_interview import guide_voice, session_config
from app.api.live_title_tour import narration_prompt

class GuideVoiceTests(unittest.TestCase):
    def test_matching_voice_mapping(self):
        self.assertEqual(guide_voice("male"), "cedar")
        self.assertEqual(guide_voice("female"), "marin")
        self.assertEqual(guide_voice(""), "cedar")

    def test_interview_uses_selected_voice_and_style(self):
        for language in ("ja", "en"):
            for gender in ("male", "female"):
                config = session_config(language, gender)
                self.assertEqual(config["audio"]["output"]["voice"], guide_voice(gender))
                self.assertIn("male voice" if gender == "male" else "female voice", config["instructions"]) if language == "en" else self.assertIn("男性の声" if gender == "male" else "女性の声", config["instructions"])
        self.assertNotIn("composed male voice", session_config("en", "male")["instructions"])

    def test_menu_reconnect_does_not_restart_language_tutorial(self):
        ja = narration_prompt("ja", "title", "menu", "male", True)
        en = narration_prompt("en", "title", "menu", "female", True)
        self.assertIn("衣替え", ja)
        self.assertIn("makeover", en)
        self.assertNotIn("最初は『100 Hour", ja)
        self.assertNotIn("press the button below", en)
        self.assertIn("男性の声", ja)

    def test_return_and_portrait_keep_voice_without_switch_joke(self):
        menu = narration_prompt("ja", "title", "menu", "male", False)
        self.assertIn("おかえり", menu)
        self.assertNotIn("衣替え", menu)
        photo = narration_prompt("ja", "portrait", gender="male")
        self.assertIn("男性の声", photo)
        self.assertNotIn("落ち着いた男性の声", photo)

    def test_initial_screen_still_does_not_preview_gender(self):
        text = narration_prompt("ja", "title")
        self.assertIn("明示的な合図が来るまで一切しない", text)

if __name__ == "__main__":
    unittest.main()
