using System;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public enum GameState
{
    Start, Draw, Play, Win, Lose
}


[Serializable]
public class PlayerCamera
{
    [SerializeField] private GameObject playCamera, drawCamera;

    public void SetCamera(GameState state)
    {
        drawCamera.SetActive(true);
        playCamera.SetActive(false);
        return;
        drawCamera.SetActive(state == GameState.Start || state == GameState.Draw);
        playCamera.SetActive(state == GameState.Play || state == GameState.Win || state == GameState.Lose);
       
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; protected set;}
    public static GameState State;
    [SerializeField] private LevelManager[] levels;
    [HideInInspector] public LevelManager currentLevel;
    public Transform cameraPoint;
    public static int Level = 1;



    private void Awake()
    {
        Instance = this;
        Level = PlayerPrefs.GetInt("Level",1);
        MakeLevel();
        TinySauce.OnGameStarted(levels.ToString());
    }

    private void MakeLevel()
    {
        var l = Level;
        if (l >= levels.Length+1) l = Random.Range(1, levels.Length + 1);
        currentLevel = Instantiate(levels[l-1]);
    }

    public void Draw()
    {
        UiManager.Instance.panels.SetPanel(GameState.Draw);
    }
    public void Play()
    {
        UiManager.Instance.panels.SetPanel(GameState.Play);
        Train.Instance.MoveTrain();
    }

    public void Win()
    {
        UiManager.Instance.panels.SetPanel(GameState.Win);
    }

    public void Lose()
    {
        UiManager.Instance.panels.SetPanel(GameState.Lose);
    }

    private float playTime;
    private void Update()
    {
        playTime += Time.deltaTime;
        print(playTime);
        if(Input.GetMouseButtonDown(0) && State == GameState.Start) Draw();
        KeyBoard();
    }
    
    private void KeyBoard()
    {
        if (Input.GetKeyDown(KeyCode.Space)) playTime = 0;
        if (Input.GetKeyDown(KeyCode.E)) UiManager.Instance.Edit();
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.P)) Play();
        if (Input.GetKeyDown(KeyCode.S)) Time.timeScale = Time.timeScale == 1 ? 0.1f : 1f;
    }
    



    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void CheckIfTakeAllPeople()
    {
        var peoples = FindObjectsOfType<People>();
        var win = true;
        for (int i = 0; i < peoples.Length; i++)
        {
            if (!peoples[i].IsInTrain()) win = false;
        }

        if (win) currentLevel.bar.DOScale(0, 0.6f);
    }

    public void Edit()
    {
       currentLevel.bar.DOScale(1, 0.6f);
    }
}


#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(GameManager))]
public class EnemyPartEditor : Editor
{ 
    public override void OnInspectorGUI()
    { 
        base.OnInspectorGUI();
        if(GUILayout.Button("Randomized Characters"))
        {
            var people = FindObjectsOfType<People>();
            for (int i = 0; i < people.Length; i++)
            {
                people[i].RandomizedCharacters();
                switch (Random.Range(0,4))
                {
                    case 0:
                        people[i].transform.eulerAngles = new Vector3(0, 90, 0);
                        break;
                    case 1:
                        people[i].transform.eulerAngles = new Vector3(0, 90, 0);
                        break;
                    case 2:
                        people[i].transform.eulerAngles = new Vector3(0, 180, 0);
                        break;
                    case 3:
                        people[i].transform.eulerAngles += new Vector3(0, 270, 0);
                        break;
                }
            }
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
