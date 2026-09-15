using TMPro;
using UnityEngine;

namespace HundredHour.UI.Participants.Demo
{
    public sealed class ParticipantCardsDemo : MonoBehaviour
    {
        [SerializeField] ParticipantCardController[] cards;
        [SerializeField] TMP_Text feedback;
        public void Configure(ParticipantCardController[] participants, TMP_Text label) { cards=participants; feedback=label; }
        public void ToggleElimination()
        {
            var card=cards[2]; card.SetEliminated(!card.Settings.eliminated);
            feedback.text=card.Settings.displayName+(card.Settings.eliminated ? " is OUT." : " is back in the show.");
        }
        public void AddStars()
        {
            var card=cards[0]; card.SetStars(card.Settings.stars+10000,card.Settings.support+.02f);
            feedback.text="Runa received 10K new stars.";
        }
        public void ResetCards()
        {
            cards[0].SetStars(342000,.28f); cards[1].SetStars(231000,.19f); cards[2].SetStars(48000,.04f);
            for(int i=0;i<cards.Length;i++) cards[i].SetEliminated(i==2);
            feedback.text="Live standings  /  2 active · 1 eliminated";
        }
    }
}
