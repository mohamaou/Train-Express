using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[Serializable]
public class UIPanels
{
    [SerializeField] private GameObject startPanel, drawPanel, playPanel, winPanel, losePanel;
    public void SetPanel(GameState state = GameState.Start)
    {
        startPanel.SetActive(false);
        drawPanel.SetActive(false);
        playPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        GameManager.State = state;
        GameManager.Instance.currentLevel.playerCamera.SetCamera(state);
        return;
        switch (state)
        {
            case GameState.Start:
                startPanel.SetActive(true);
                break;
            case GameState.Draw:
                drawPanel.SetActive(true);
                break;
            case GameState.Play:
                playPanel.SetActive(true);
                break;
            case GameState.Lose:
                losePanel.SetActive(true);
                break;
            case GameState.Win:
                winPanel.SetActive(true);
                break;
        }
    }

    public void HidePlayPanel()
    {
        playPanel.SetActive(false);
    }
}

public class UiManager : MonoBehaviour
{
    public static UiManager Instance {get; protected set;}
    public UIPanels panels;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private DOTweenAnimation editButton;
    [SerializeField] private GameObject fast, slow;
    [SerializeField] private Transform hand;

    private void Start()
    {
        Instance = this;
        PlayerPrefs.SetInt("Level Index", SceneManager.GetActiveScene().buildIndex);
        panels.SetPanel();
        levelText.text = "Level " + GameManager.Level;
        hand.gameObject.SetActive(true);
    }
    


    #region Buttons
    public void Draw()
    {
        GameManager.Instance.Draw();
    }
    public void Play()
    {
        GameManager.Instance.Play();
    }
    public void Restart()
    {
        TinySauce.OnGameFinished(false, 0, GameManager.Level.ToString());
        GameManager.Instance.Restart();
    }
    public void Next()
    {
        var level = GameManager.Level;
        TinySauce.OnGameFinished(true, 0, level.ToString());
        PlayerPrefs.SetInt("Level",level+1);
        SceneManager.LoadScene(0);
    }
    public void ClearRoad()
    {
        DrawRailroad.Instance.ClearRoad();
    }
    public void Edit()
    {
        Train.Instance.Back();
        editButton.DOPause();
        GameManager.Instance.Edit();
        panels.SetPanel(GameState.Draw);
    }

    public void Speed()
    {
        Time.timeScale = Time.timeScale == 1f ? 2f: 1f;
        fast.SetActive(Time.timeScale == 1f);
        slow.SetActive(Time.timeScale != 1f);
    }
    #endregion
    
    private void Update()
    {
        hand.position = Input.mousePosition;
        if(Input.GetKeyDown(KeyCode.N))Next();
    }

    public void Wrong()
    {
        editButton.DORestart();
    }
    public void HidePlayPanel()
    {
        panels.HidePlayPanel();
    }
}
