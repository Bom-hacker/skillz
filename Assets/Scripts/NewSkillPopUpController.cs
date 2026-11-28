using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DefaultNamespace
{
    public class NewSkillPopUpController : MonoBehaviour
    {
        [Header("Input Fields")]
        public TMP_InputField nameInput;
        public TMP_InputField parentSkillInput;
        public TMP_InputField childSkillInput;
        public TMP_InputField startingXPInput;
        public TMP_InputField descriptionInput;

        [Header("Buttons")] public Button saveButton;

        private void Start()
        {
           // saveButton.onClick.AddListener(SaveNewSkill);
        }

        public void SaveNewSkill()
        {
            if (string.IsNullOrWhiteSpace(nameInput.text)) return;
            
            int.TryParse(startingXPInput.text, out int parsedXP);

            SkillNodeData newSkill = new SkillNodeData()
            {
                id = "skill_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                name = nameInput.text,
                parentSkillId = parentSkillInput.text,
                startingXP = parsedXP,
                currentXP = parsedXP,
                description = descriptionInput.text,
                posX = UnityEngine.Random.Range(-200f, 200f),
                posY = UnityEngine.Random.Range(-100f, 200f),
            };
            
            if (!string.IsNullOrWhiteSpace(childSkillInput.text))
                newSkill.childSkillIds.Add(childSkillInput.text);
            
            DataManager.Instance.CurrentData.skills.Add(newSkill);
            DataManager.Instance.SaveData();

            ClearFields();
            gameObject.SetActive(false); // Popup schließen
        }

        public void ClearFields()
        {
            nameInput.text = "";
            parentSkillInput.text = "";
            childSkillInput.text = "";
            startingXPInput.text = "";
            descriptionInput.text = "";
        }
    }
}