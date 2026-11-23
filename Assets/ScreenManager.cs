using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject[] screens;

    void Start()
    {
        ShowScreen(0);
    }

    public void ShowScreen(int index)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].SetActive(i == index);
        }
    }
}