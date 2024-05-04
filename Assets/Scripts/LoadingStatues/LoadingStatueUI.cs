using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingStatueUI : MonoBehaviour
{
    [SerializeField] private FighterLoadingStatue loadingStatue;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button cancelButton;

    private void Start()
    {
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });

        loadButton.onClick.AddListener(() =>
        {
            loadingStatue.LoadReplay();
        });

        textField.text = loadingStatue.GetLoadingStatueSO().messageText;

        Hide();
    }

    private void LoadingStatue_OnStatueHidden(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void LoadingStatue_OnStatueClicked(object sender, System.EventArgs e)
    {
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public string GetMessageText()
    {
        return textField.text;
    }
}
