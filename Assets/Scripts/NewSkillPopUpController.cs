using System;
using System.Linq;
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

        [Header("Optional")]
        public TMP_Text titleText; // Ueberschrift im Popup

        private string editingId;       // gesetzt = Bearbeiten-Modus
        private string presetParentId;  // gesetzt = neuer Unter-Skill fuer diesen Skill

        // Bearbeiten: Felder mit den vorhandenen Werten fuellen
        public void OpenForEdit(SkillNodeData skill)
        {
            gameObject.SetActive(true);
            editingId = skill.id;

            SetTitle("Edit skill");
            nameInput.text = skill.name;

            SkillNodeData parent = SkillUtil.Find(skill.parentSkillId);
            parentSkillInput.text = parent != null ? parent.name : "";

            // Unter-Skills werden ueber das "+" im Detail-Popup verwaltet
            childSkillInput.text = string.Join(", ", SkillUtil.GetChildren(skill).Select(c => c.name));
            childSkillInput.interactable = false;

            startingXPInput.text = skill.currentXP.ToString();
            descriptionInput.text = skill.description;
        }

        // Neuer Unter-Skill: Parent ist schon festgelegt
        public void OpenForNewChild(SkillNodeData parent)
        {
            gameObject.SetActive(true);
            presetParentId = parent.id;

            SetTitle("New sub-skill");
            parentSkillInput.text = parent.name;
            parentSkillInput.interactable = false;
        }

        // Beim Schliessen immer auf "neuer Skill" zuruecksetzen
        private void OnDisable()
        {
            if (nameInput == null) return;

            editingId = null;
            presetParentId = null;
            parentSkillInput.interactable = true;
            childSkillInput.interactable = true;
            SetTitle("New skill");
            ClearFields();
        }

        public void SaveNewSkill()
        {
            if (string.IsNullOrWhiteSpace(nameInput.text)) return;

            int.TryParse(startingXPInput.text, out int parsedXP);

            string parentId = presetParentId ?? SkillUtil.ResolveId(parentSkillInput.text);

            // Getippter Parent existiert nicht: nicht stillschweigend zur Wurzel machen
            if (presetParentId == null && !string.IsNullOrWhiteSpace(parentSkillInput.text) && parentId == "")
            {
                Debug.LogWarning("Parent-Skill nicht gefunden: " + parentSkillInput.text);
                return;
            }

            if (editingId != null)
            {
                SkillNodeData skill = SkillUtil.Find(editingId);
                if (skill != null)
                {
                    skill.name = nameInput.text;
                    skill.description = descriptionInput.text;
                    skill.currentXP = parsedXP;

                    if (parentId != skill.parentSkillId && !SkillUtil.WouldCreateCycle(skill.id, parentId))
                        SkillUtil.SetParent(skill, parentId);
                }
            }
            else
            {
                SkillNodeData parent = SkillUtil.Find(parentId);

                SkillNodeData newSkill = new SkillNodeData()
                {
                    id = "skill_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    name = nameInput.text,
                    startingXP = parsedXP,
                    currentXP = parsedXP,
                    description = descriptionInput.text,
                    // Neue Knoten erscheinen ueber ihrem Eltern-Skill
                    posX = parent != null ? parent.posX + UnityEngine.Random.Range(-160f, 160f)
                                          : UnityEngine.Random.Range(-200f, 200f),
                    posY = parent != null ? parent.posY + 220f
                                          : UnityEngine.Random.Range(-100f, 200f),
                };

                DataManager.Instance.CurrentData.skills.Add(newSkill);
                SkillUtil.SetParent(newSkill, parentId);

                SkillNodeData child = SkillUtil.Find(SkillUtil.ResolveId(childSkillInput.text));
                if (child != null && !SkillUtil.WouldCreateCycle(child.id, newSkill.id))
                    SkillUtil.SetParent(child, newSkill.id);
            }

            DataManager.Instance.SaveData();
            gameObject.SetActive(false); // Popup schliessen (OnDisable raeumt auf)
        }

        public void ClearFields()
        {
            nameInput.text = "";
            parentSkillInput.text = "";
            childSkillInput.text = "";
            startingXPInput.text = "";
            descriptionInput.text = "";
        }

        private void SetTitle(string text)
        {
            if (titleText != null) titleText.text = text;
        }
    }
}