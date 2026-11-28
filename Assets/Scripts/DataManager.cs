using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SkillNodeData // Repräsentiert einen einzelnen Knoten im Tree
{
    public string id;
    public string name;
    public string parentSkillId;
    public List<string> childSkillIds = new List<string>();
    public int startingXP;
    public int currentXP;
    public string description;
    public float posX;
    public float posY;
}

[Serializable]
public class RoutineData // Repräsentiert eine Routine, die XP liefert
{
    public string id;
    public string name;
    public string skillId;
    public string requirements;
    public string type;
    public string when;
}

[Serializable]
public class GoalData // Repräsentiert ein konkretes Ziel, das auf einen Skill "einwirkt"
{
    public string id;
    public string name;
    public string skillId;
    public string goal;
    public string time;
    public string difficulty;
}

[Serializable]
public class SkillzSaveData // Root Container für gesamte JSON. Bündelt alle Listen in einem Objekt
{
    public List<SkillNodeData> skills = new List<SkillNodeData>();
    public List<RoutineData> routines = new List<RoutineData>();
    public List<GoalData> goals = new List<GoalData>();
}


    //Datamanager: Verwaltet das Laden, Speichern und Zurücksetzen der JSON-Daten

    public class DataManager : MonoBehaviour
    {
        // Globale Instanz für Zugriff aus anderen Skripts
        public static DataManager Instance { get; private set; }
        // Aktuell im Arbeitsspeicher gehaltene Datne
        public SkillzSaveData CurrentData { get; private set; }

        // Ausgelöst, sobald Daten gespeichert oder zurückgesetzt werden
        public event Action OnDataChanged;

        private string saveFilePath; // Aktiver Spielstand
        private string initialFilePath; // Initiale Test-JSON

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else {Destroy(gameObject);
                return;
            }
            saveFilePath = Path.Combine(Application.persistentDataPath, "skillz_save.json");
            initialFilePath = Path.Combine(Application.streamingAssetsPath, "initial_skillz_data.json");

            LoadData();
        }

        public void LoadData()
        {
            if (File.Exists(saveFilePath))
            {
                CurrentData = JsonUtility.FromJson<SkillzSaveData>(File.ReadAllText(saveFilePath));
            }
            else if (File.Exists(initialFilePath))
            {
                CurrentData = JsonUtility.FromJson<SkillzSaveData>(File.ReadAllText(initialFilePath));
                SaveData();
            }
            else
            {
                CurrentData = new SkillzSaveData();
            }
        }

        public void SaveData()
        {
            string json = JsonUtility.ToJson(CurrentData, true);
            File.WriteAllText(saveFilePath, json);
            OnDataChanged?.Invoke(); // Informiert Skill Tree & Listen über neue Daten
        }

        [ContextMenu("Reset To Initial JSON")]
        public void ResetToInitialJson()
        {
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
            LoadData();
            OnDataChanged?.Invoke();
        }

    }
