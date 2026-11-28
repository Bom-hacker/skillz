using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    public TMP_Text messageText;
    public Button yesButton;
    public Button noButton;

    private Action onYes;

    void Awake()
    {
        yesButton.onClick.AddListener(() =>
        {
            Action callback = onYes;
            Close();
            callback?.Invoke();
        });
        noButton.onClick.AddListener(Close);
    }

    public void Show(string message, Action yesCallback)
    {
        onYes = yesCallback;
        messageText.text = message;
        gameObject.SetActive(true);
    }

    private void Close()
    {
        onYes = null;
        gameObject.SetActive(false);
    }
}