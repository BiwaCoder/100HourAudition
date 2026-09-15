import unittest
from app.api.live_finale import finale_prompt, session_config
from app.live_interview import guide_voice


class LiveFinaleTests(unittest.TestCase):
    def test_prompt_includes_names_and_summary_material(self):
        for language in ("ja", "en"):
            text = finale_prompt(language, "松村悠斗", "ひまり", "穏やかで情熱的なクリエイター", "手をつないだ。相互理解84。", "male")
            self.assertIn("松村悠斗", text)
            self.assertIn("ひまり", text)
            self.assertIn("相互理解84", text)

    def test_prompt_forbids_inventing_facts_and_breaking_character(self):
        ja = finale_prompt("ja", "松村悠斗", "ひまり", "性格", "出来事", "male")
        en = finale_prompt("en", "松村悠斗", "ひまり", "personality", "events", "male")
        self.assertIn("捏造", ja)
        self.assertIn("invent", en)
        self.assertIn("ゲームやAI", ja)
        self.assertIn("AI", en)

    def test_session_config_uses_gender_matched_voice(self):
        male = session_config("ja", "male", "松村悠斗", "ひまり", "性格", "出来事")
        female = session_config("ja", "female", "沙織", "ひまり", "性格", "出来事")
        self.assertEqual(male["audio"]["output"]["voice"], guide_voice("male"))
        self.assertEqual(female["audio"]["output"]["voice"], guide_voice("female"))
        self.assertEqual(male["model"], "gpt-live-1")
        self.assertEqual(male["delegation"], {"type": "client"})
        self.assertFalse(male["store"])


if __name__ == "__main__":
    unittest.main()
