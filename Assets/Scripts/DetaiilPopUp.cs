using System.Linq;
using TMPro;
using UnityEngine;

public class DetailPopup : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text info1Text;
    public TMP_Text info2Text;
    public TMP_Text info3Text;

    private SkillzSaveData Data
    {
        get { return DataManager.Instance != null ? DataManager.Instance.CurrentData : null; }
    }

    public void ShowSkill(string id)
    {
        if (Data == null) return;

        SkillNodeData skill = Data.skills.Find(s => s.id == id);
        if (skill == null)
        {
            Debug.LogWarning("DetailPopup: Skill nicht gefunden: " + id);
            return;
        }

        SkillNodeData parent = FindSkill(skill.parentSkillId);
        string children = string.Join(", ", skill.childSkillIds
            .Select(c => FindSkill(c))
            .Where(c => c != null)
            .Select(c => c.name));

        Fill(
            skill.name,
            string.IsNullOrEmpty(skill.description) ? "Keine Beschreibung" : skill.description,
            "XP: " + skill.currentXP + " (Start: " + skill.startingXP + ")",
            "Übergeordnet: " + (parent != null ? parent.name : "Keiner"),
            "Untergeordnet: " + (children.Length > 0 ? children : "Keine"));
    }

    public void ShowRoutine(string id)
    {
        if (Data == null) return;

        RoutineData r = Data.routines.Find(x => x.id == id);
        if (r == null)
        {
            Debug.LogWarning("DetailPopup: Routine nicht gefunden: " + id);
            return;
        }

        SkillNodeData skill = FindSkill(r.skillId);

        Fill(
            r.name,
            "Voraussetzungen: " + (string.IsNullOrEmpty(r.requirements) ? "Keine" : r.requirements),
            "Skill: " + (skill != null ? skill.name : "Keiner"),
            "Typ: " + r.type,
            "Wann: " + r.when);
    }

    public void ShowGoal(string id)
    {
        if (Data == null) return;

        GoalData g = Data.goals.Find(x => x.id == id);
        if (g == null)
        {
            Debug.LogWarning("DetailPopup: Ziel nicht gefunden: " + id);
            return;
        }

        SkillNodeData skill = FindSkill(g.skillId);

        Fill(
            g.name,
            g.goal,
            "Skill: " + (skill != null ? skill.name : "Keiner"),
            "Zeitraum: " + g.time,
            "Schwierigkeit: " + g.difficulty);
    }

    // Sucht einen Skill ueber die ID, ersatzweise ueber den Namen
    private SkillNodeData FindSkill(string idOrName)
    {
        if (string.IsNullOrEmpty(idOrName)) return null;

        SkillNodeData byId = Data.skills.Find(s => s.id == idOrName);
        if (byId != null) return byId;

        return Data.skills.Find(s => string.Equals(s.name, idOrName, System.StringComparison.OrdinalIgnoreCase));
    }

    private void Fill(string title, string description, string info1, string info2, string info3)
    {
        SetText(titleText, title);
        SetText(descriptionText, description);
        SetText(info1Text, info1);
        SetText(info2Text, info2);
        SetText(info3Text, info3);

        gameObject.SetActive(true);
    }

    // Leere Felder werden ausgeblendet
    private void SetText(TMP_Text field, string value)
    {
        field.text = value;
        field.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }
}