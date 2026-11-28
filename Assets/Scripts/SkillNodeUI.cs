using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class SkillNodeUI : MonoBehaviour
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI xpText;
        public Image backgroundImage;
        
        public SkillNodeData NodeData { get; private set; }

        public void Setup(SkillNodeData data)
        {
            NodeData = data;
            if (titleText != null) titleText.text = data.name;
            if (xpText != null) xpText.text = data.currentXP + "XP";

            if (backgroundImage != null)
            {
                backgroundImage.color = data.currentXP > 0
                    ? new Color(0.2f, 0.7f, 0.4f)
                    : new Color(0.4f, 0.4f, 0.4f);
            }
        }
    }
}