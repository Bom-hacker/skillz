using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DefaultNamespace
{
    public class SkillTreeVisualizer : MonoBehaviour
    {
        [Header("References")] 
        public RectTransform treeContainer; // Bereich auf SkillTreeMainScreen, in dem der Baum liegt
        public SkillNodeUI skillNodePrefab; // Prefab für einen einzelnen Skill
        public RectTransform linePrefab; // Ein einfaches dünnes UI Image als Verbindungslinie
        
        public DetailPopup detailPopup;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        private void Start()
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.OnDataChanged += DrawSkillTree;
                DrawSkillTree();
            }
        }

        private void OnDestroy()
        {
            if (DataManager.Instance != null)
                DataManager.Instance.OnDataChanged -= DrawSkillTree;
        }

        public void DrawSkillTree()
        {
            // 1. Alten Baum aufräumen
            foreach (var obj in spawnedObjects)
            {
                Destroy(obj);
            }
            spawnedObjects.Clear();
            
            var skills = DataManager.Instance.CurrentData.skills;
            Dictionary<string, RectTransform> nodeTransforms = new Dictionary<string, RectTransform>();
            
            // 2. Alle Skill-Nodes spawnen
            foreach (var skill in skills)
            {
                SkillNodeUI nodeInstance = Instantiate(skillNodePrefab, treeContainer);
                RectTransform rt = nodeInstance.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(skill.posX, skill.posY);
                
                nodeInstance.Setup(skill);
                nodeInstance.Clicked = data => detailPopup.ShowSkill(data.id);
                nodeTransforms[skill.id] = rt;
                spawnedObjects.Add(nodeInstance.gameObject);
            }

            foreach (var skill in skills)
            {
                if (!string.IsNullOrEmpty(skill.parentSkillId) && nodeTransforms.ContainsKey(skill.parentSkillId) &&
                    nodeTransforms.ContainsKey(skill.id))
                {
                    DrawConnection(nodeTransforms[skill.parentSkillId].anchoredPosition, nodeTransforms[skill.id].anchoredPosition);
                }
            }
        }
        private void DrawConnection(Vector2 posA, Vector2 posB)
        {
            if (linePrefab == null) return;

            RectTransform line = Instantiate(linePrefab, treeContainer);
            line.SetAsFirstSibling();
            spawnedObjects.Add(line.gameObject);
            
            Vector2 dir  = (posB - posA).normalized;
            float distance = Vector2.Distance(posA, posB);
            
            line.anchoredPosition = posA + dir * distance * 0.5f;
            line.sizeDelta = new Vector2(distance, 6f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            line.localRotation = Quaternion.Euler(0, 0, angle);
        }

    }
}