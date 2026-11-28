using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPopup : MonoBehaviour
{
    [Header("Header")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text levelText;
    public Button editButton;
    public Button deleteButton;
    public Button hideButton;
    public Button addButton;
    public Button closeButton;
    public Image hideButtonIcon;
    public Sprite eyeSprite;      // optional
    public Sprite eyeOffSprite;   // optional

    [Header("Body")]
    public TMP_Text descriptionText;
    public TMP_Text xpText;
    public Transform childrenContainer;
    public SkillChildRow childRowPrefab;

    [Header("Andere Popups")]
    public NewSkillPopUpController newSkillPopup;
    public ConfirmPopup confirmPopup;

    private string currentId;

    void Awake()
    {
        editButton.onClick.AddListener(() => EditSkill(currentId));
        deleteButton.onClick.AddListener(() => AskDelete(currentId));
        hideButton.onClick.AddListener(() => ToggleHidden(currentId));
        addButton.onClick.AddListener(() => AddChild(currentId));
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void OnEnable()
    {
        if (DataManager.Instance != null) DataManager.Instance.OnDataChanged += Refresh;
    }

    void OnDisable()
    {
        if (DataManager.Instance != null) DataManager.Instance.OnDataChanged -= Refresh;
    }

    // Wird vom Skill Tree und von der Suche aufgerufen
    public void Show(string skillId)
    {
        currentId = skillId;
        gameObject.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        SkillNodeData skill = SkillUtil.Find(currentId);
        if (skill == null)
        {
            gameObject.SetActive(false);
            return;
        }

        titleText.text = skill.name;
        levelText.text = "Level " + SkillUtil.GetLevel(skill.currentXP);
        descriptionText.text = string.IsNullOrEmpty(skill.description) ? "Keine Beschreibung" : skill.description;
        xpText.text = skill.currentXP + " XP";
        SetHideIcon(hideButtonIcon, skill.isHidden);

        foreach (Transform child in childrenContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        foreach (SkillNodeData child in SkillUtil.GetChildren(skill))
            BuildRow(child);
    }

    private void BuildRow(SkillNodeData child)
    {
        SkillChildRow row = Instantiate(childRowPrefab, childrenContainer);

        row.nameText.text = child.name;
        row.levelText.text = "Level " + SkillUtil.GetLevel(child.currentXP);
        row.dot.color = child.currentXP > 0 ? new Color(0.2f, 0.7f, 0.4f) : new Color(0.4f, 0.4f, 0.4f);
        row.group.alpha = child.isHidden ? 0.5f : 1f;
        SetHideIcon(row.hideIcon, child.isHidden);

        string id = child.id;
        row.openButton.onClick.AddListener(() => Show(id));
        row.editButton.onClick.AddListener(() => EditSkill(id));
        row.deleteButton.onClick.AddListener(() => AskDelete(id));
        row.hideButton.onClick.AddListener(() => ToggleHidden(id));
    }

    private void SetHideIcon(Image icon, bool hidden)
    {
        if (icon == null) return;

        if (eyeSprite != null && eyeOffSprite != null)
        {
            icon.sprite = hidden ? eyeOffSprite : eyeSprite;
        }
        else
        {
            Color c = icon.color;
            icon.color = new Color(c.r, c.g, c.b, hidden ? 0.4f : 1f);
        }
    }

    private void ToggleHidden(string id)
    {
        SkillNodeData skill = SkillUtil.Find(id);
        if (skill == null) return;

        skill.isHidden = !skill.isHidden;
        DataManager.Instance.SaveData();
    }

    private void EditSkill(string id)
    {
        SkillNodeData skill = SkillUtil.Find(id);
        if (skill != null) newSkillPopup.OpenForEdit(skill);
    }

    private void AddChild(string parentId)
    {
        SkillNodeData parent = SkillUtil.Find(parentId);
        if (parent != null) newSkillPopup.OpenForNewChild(parent);
    }

    private void AskDelete(string id)
    {
        SkillNodeData skill = SkillUtil.Find(id);
        if (skill == null) return;

        confirmPopup.Show(
            "\"" + skill.name + "\" wirklich löschen? Der XP-Fortschritt geht verloren.",
            () => DeleteSkill(id));
    }

    private void DeleteSkill(string id)
    {
        SkillzSaveData data = DataManager.Instance.CurrentData;
        SkillNodeData skill = SkillUtil.Find(id);
        if (skill == null) return;

        SkillNodeData parent = SkillUtil.Find(skill.parentSkillId);

        // Unter-Skills ruecken eine Ebene hoch
        List<SkillNodeData> children = SkillUtil.GetChildren(skill);
        foreach (SkillNodeData child in children)
            SkillUtil.SetParent(child, skill.parentSkillId);

        if (parent != null) parent.childSkillIds.Remove(skill.id);

        // Routinen und Ziele verlieren die Verknuepfung zum Skill
        foreach (RoutineData r in data.routines)
            if (r.skillId == id) r.skillId = "";
        foreach (GoalData g in data.goals)
            if (g.skillId == id) g.skillId = "";

        // War der geloeschte Skill gerade offen, zeigt das Popup danach den Parent (oder schliesst)
        if (id == currentId) currentId = parent != null ? parent.id : null;

        data.skills.Remove(skill);
        DataManager.Instance.SaveData();
    }
}