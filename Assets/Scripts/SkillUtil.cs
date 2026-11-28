using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SkillUtil
{
    // Platzhalter: so viele XP pro Level. Mit dem Team abstimmen!
    public const int XpPerLevel = 100;

    public static int GetLevel(int xp)
    {
        return Mathf.Max(0, xp) / XpPerLevel;
    }

    public static SkillNodeData Find(string id)
    {
        if (string.IsNullOrEmpty(id) || DataManager.Instance == null || DataManager.Instance.CurrentData == null)
            return null;
        return DataManager.Instance.CurrentData.skills.Find(s => s.id == id);
    }

    // Nimmt eine ID oder einen Namen und liefert die ID ("" wenn nichts passt)
    public static string ResolveId(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName) || DataManager.Instance == null) return "";

        string text = idOrName.Trim();
        List<SkillNodeData> skills = DataManager.Instance.CurrentData.skills;
        SkillNodeData match = skills.Find(s => s.id == text)
            ?? skills.Find(s => string.Equals(s.name, text, StringComparison.OrdinalIgnoreCase));
        return match != null ? match.id : "";
    }

    // Alle direkten Unter-Skills (egal ob ueber parentSkillId oder childSkillIds verknuepft)
    public static List<SkillNodeData> GetChildren(SkillNodeData skill)
    {
        return DataManager.Instance.CurrentData.skills
            .Where(s => s.parentSkillId == skill.id || skill.childSkillIds.Contains(s.id))
            .Distinct()
            .ToList();
    }

    // Haengt einen Skill unter einen neuen Parent und haelt beide Listen konsistent
    public static void SetParent(SkillNodeData skill, string newParentId)
    {
        SkillNodeData oldParent = Find(skill.parentSkillId);
        if (oldParent != null) oldParent.childSkillIds.Remove(skill.id);

        skill.parentSkillId = newParentId;

        SkillNodeData newParent = Find(newParentId);
        if (newParent != null && !newParent.childSkillIds.Contains(skill.id))
            newParent.childSkillIds.Add(skill.id);
    }

    // Verhindert Schleifen: ein Skill darf nicht unter sich selbst oder seinen Nachfahren haengen
    public static bool WouldCreateCycle(string skillId, string newParentId)
    {
        string current = newParentId;
        int guard = 0;
        while (!string.IsNullOrEmpty(current) && guard++ < 100)
        {
            if (current == skillId) return true;
            SkillNodeData node = Find(current);
            current = node != null ? node.parentSkillId : "";
        }
        return false;
    }

    // Gibt einem Skill XP und reicht dieselbe Menge an alle Eltern-Skills nach oben weiter
public static void AddXP(string skillId, int amount, bool save = true)
{
    if (amount == 0) return;

    string current = skillId;
    HashSet<string> visited = new HashSet<string>(); // Schutz vor Endlosschleifen

    while (!string.IsNullOrEmpty(current) && visited.Add(current))
    {
        SkillNodeData skill = Find(current);
        if (skill == null) break;

        skill.currentXP = Mathf.Max(0, skill.currentXP + amount);
        current = skill.parentSkillId;
    }

    if (save) DataManager.Instance.SaveData();
}
}