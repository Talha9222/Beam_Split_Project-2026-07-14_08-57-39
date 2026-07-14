using TMPro;
using UnityEngine;

namespace BeamSplit.UI
{
    public class TutorialPanel : PanelBase
    {
        [SerializeField] private TMP_Text bodyText;

        public const string DefaultTutorialText =
            "Tap Mirror to place a mirror, splitting the beam into two 45-degree branches.\n" +
            "Tap Red / Blue / Yellow to place a filter that tints the beam that color.\n" +
            "Tap Erase to remove a placed piece.\n" +
            "Route the correct color onto every receiver to win.";

        public void Show(string tutorialText)
        {
            if (bodyText != null)
            {
                bodyText.text = string.IsNullOrEmpty(tutorialText) ? DefaultTutorialText : tutorialText;
            }

            Show();
        }
    }
}
