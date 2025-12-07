using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    
    [Header("UI")]
    [SerializeField] protected Button playButton;
    [SerializeField] protected Button resetButton;
    [SerializeField] protected GameObject winPanel;
    [SerializeField] protected GameObject losePanel;

    protected void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one UIManager in scene!");
            return;
        }

        instance = this;
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
    }
}
