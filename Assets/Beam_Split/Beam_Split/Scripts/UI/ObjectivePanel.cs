using TMPro;
using UnityEngine;

namespace BeamSplit.UI
{
    public class ObjectivePanel : PanelBase
    {
        [SerializeField] private TMP_Text bodyText;

        public void Show(string objectiveDescription)
        {
            if (bodyText != null)
            {
                bodyText.text = objectiveDescription;
            }

            Show();
        }
    }
}
