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
                Debug.Log($"[ObjectivePanel] bodyText set to: \"{objectiveDescription}\" on {bodyText.name}");
            }
            else
            {
                Debug.LogError("[ObjectivePanel] bodyText is not assigned; cannot display objective text.");
            }

            Show();
        }
    }
}
