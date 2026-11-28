using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraXpPopup : MonoBehaviour
{
    public TMP_InputField skillInput;
    public Transform suggestionsContainer;
    public GameObject suggestionItemPrefab;
    public TMP_InputField xpInput;
    public Button saveButton;
    public Button closeButton;
    public int maxSuggestions = 5;

    private string selectedId;

    void Awake()
    {
        skillInput.onValueChanged.AddListener(OnSkillTextChanged);
        saveButton.onClick.AddListener(Save);
        if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void OnEnable()
    {
        selectedId = null;
        skillInput.SetTextWithoutNotify("");
        xpInput.SetTextWithoutNotify("");
        ClearSuggestions();
    }

    private void OnSkillTextChanged(string text)
    {
        selectedId = null; // Es wird wieder getippt: bisherige Auswahl verwerfen
        ClearSuggestions();

        if (string.IsNullOrWhiteSpace(text) ||
            DataManager.Instance == null || DataManager.Instance.CurrentData == null)
            return;

        string search = text.Trim();

        List<SkillNodeData> matches = DataManager.Instance.CurrentData.skills
            .Where(s => !string.IsNullOrEmpty(s.name) &&
                        s.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(s => s.name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            .ThenBy(s => s.name)
            .Take(maxSuggestions)
            .ToList();

        foreach (SkillNodeData skill in matches)
        {
            GameObject item = Instantiate(suggestionItemPrefab, suggestionsContainer);
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

            texts[0].text = skill.name;
            if (texts.Length > 1) texts[1].text = "Level " + SkillUtil.GetLevel(skill.currentXP);

            SkillNodeData chosen = skill;
            item.GetComponent<Button>().onClick.AddListener(() => SelectSkill(chosen));
        }
    }

    private void SelectSkill(SkillNodeData skill)
    {
        selectedId = skill.id;
        skillInput.SetTextWithoutNotify(skill.name); // ohne neue Vorschlagsliste auszuloesen
        ClearSuggestions();
    }

    private void Save()
    {
        // Entweder aus der Liste gewaehlt oder der Name wurde komplett eingetippt
        string skillId = selectedId ?? SkillUtil.ResolveId(skillInput.text);

        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogWarning("ExtraXpPopup: Skill nicht gefunden: " + skillInput.text);
            return;
        }

        if (!int.TryParse(xpInput.text, out int amount) || amount <= 0)
        {
            Debug.LogWarning("ExtraXpPopup: Bitte eine XP-Zahl größer 0 eingeben.");
            return;
        }

        SkillUtil.AddXP(skillId, amount);
        gameObject.SetActive(false);
    }

    private void ClearSuggestions()
    {
        foreach (Transform child in suggestionsContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }
}