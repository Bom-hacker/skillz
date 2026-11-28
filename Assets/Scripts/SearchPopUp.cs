using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchPopup : MonoBehaviour
{
    public enum EntryType { Skill, Routine, Goal }

    private class SearchEntry
    {
        public string id;
        public string name;
        public EntryType type;
    }

    public TMP_InputField inputField;
    public Transform resultsContainer;
    public GameObject resultItemPrefab;
    public int maxResults = 5;

    void Awake()
    {
        inputField.onValueChanged.AddListener(OnSearchChanged);
    }

    void OnEnable()
    {
        inputField.text = "";
        ClearResults();
        inputField.ActivateInputField();
    }

    // Sammelt Skills, Routinen und Ziele aus den geladenen Daten
    private List<SearchEntry> GetAllEntries()
    {
        List<SearchEntry> entries = new List<SearchEntry>();

        if (DataManager.Instance == null || DataManager.Instance.CurrentData == null)
        {
            Debug.LogWarning("SearchPopup: Kein DataManager in der Scene oder keine Daten geladen.");
            return entries;
        }

        SkillzSaveData data = DataManager.Instance.CurrentData;

        foreach (SkillNodeData s in data.skills)
            entries.Add(new SearchEntry { id = s.id, name = s.name, type = EntryType.Skill });

        foreach (RoutineData r in data.routines)
            entries.Add(new SearchEntry { id = r.id, name = r.name, type = EntryType.Routine });

        foreach (GoalData g in data.goals)
            entries.Add(new SearchEntry { id = g.id, name = g.name, type = EntryType.Goal });

        return entries;
    }

    private void OnSearchChanged(string text)
    {
        ClearResults();

        if (string.IsNullOrWhiteSpace(text)) return;

        string search = text.Trim();

        // Treffer filtern, Namen die mit dem Suchbegriff anfangen kommen zuerst
        List<SearchEntry> matches = GetAllEntries()
            .Where(e => !string.IsNullOrEmpty(e.name) &&
                        e.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(e => e.name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            .ThenBy(e => e.name)
            .Take(maxResults)
            .ToList();

        foreach (SearchEntry entry in matches)
        {
            GameObject item = Instantiate(resultItemPrefab, resultsContainer);
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

            texts[0].text = entry.name;
            if (texts.Length > 1) texts[1].text = entry.type.ToString();

            SearchEntry chosen = entry;
            item.GetComponent<Button>().onClick.AddListener(() => OnResultSelected(chosen));
        }
    }

    private void OnResultSelected(SearchEntry entry)
    {
        Debug.Log("Ausgewaehlt: " + entry.type + " / " + entry.name + " (" + entry.id + ")");
        gameObject.SetActive(false);
    }

    private void ClearResults()
    {
        foreach (Transform child in resultsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}