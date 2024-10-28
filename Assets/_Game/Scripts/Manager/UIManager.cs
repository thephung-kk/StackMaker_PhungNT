using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public GameObject homePanel;
    public GameObject completeLevelPanel;
    public GameObject winGamePanel;
    public GameObject lostPanel;
    public GameObject achievementPanel;
    public GameObject ingamePanel;
    private float elapsedTime = 0f;
    public int minutes;
    public int seconds;
    public string username;

    [SerializeField] Text timerText;
    [SerializeField] Text scoreText;
    [SerializeField] Text resultText;
    [SerializeField] Text finalResultText;
    [SerializeField] Text bricksStackText;

    [SerializeField] InputField nameInput;

    private void Awake()
    {
        homePanel.SetActive(true);
        completeLevelPanel.SetActive(false);
        winGamePanel.SetActive(false);
        lostPanel.SetActive(false);
        achievementPanel.SetActive(false);
        ingamePanel.SetActive(false); 
    }
    
    private void Update()
    {
        if (Player.Instance.currentGameState == GameState.InGame)
        {
            ingamePanel.SetActive(true);
            elapsedTime += Time.deltaTime;
            minutes = Mathf.FloorToInt(elapsedTime / 60F);
            seconds = Mathf.FloorToInt(elapsedTime % 60F);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            scoreText.text = "Score: " + Player.Instance.score.ToString();
        }
        if (Player.Instance.currentGameState == GameState.FinishLevel)
        {
            bricksStackText.text = "Bricks stack: " + Player.Instance.score.ToString();
        }
        if (Player.Instance.currentGameState == GameState.Lost)
        {
            resultText.text = "Your Score: " + Player.Instance.score.ToString() 
                +"\n" + "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        if (Player.Instance.currentGameState == GameState.Won)
        {
            finalResultText.text = "Your Score: " + Player.Instance.score.ToString() 
                +"\n" + "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }


    }
    public void PlayGame()
    {
        username = nameInput.text;
        if (username.Equals(""))
        {
            username = "N/A";
        }
        LevelManager.Instance.ActivateLevel(LevelManager.Instance.currentLevel);
        Player.Instance.currentGameState = GameState.InGame;
        homePanel.SetActive(false);
    }
    public void Replay()
    {
        Player.Instance.ChangeAnim("idle");
        Player.Instance.score = 0;
        elapsedTime = 0f;
        LevelManager.Instance.currentLevel = 0; 
        LevelManager.Instance.ActivateLevel(LevelManager.Instance.currentLevel);
        Player.Instance.currentGameState = GameState.InGame;
        lostPanel.SetActive(false); 
    }
    public void NextLevel()
    {
        Player.Instance.ChangeAnim("idle");
        Player.Instance.currentGameState = GameState.InGame;
        LevelManager.Instance.CompleteLevel();
        completeLevelPanel.SetActive(false); 
    }
    public void QuitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void ShowArchievement()
    {
        homePanel.SetActive(false);
        winGamePanel.SetActive(false); 
        HighScoreManager.Instance.LoadHighScores();
        achievementPanel.SetActive(true); 
    }
    public void Home()
    {
        Player.Instance.ChangeAnim("idle");
        Player.Instance.score = 0;
        elapsedTime = 0f;
        LevelManager.Instance.DeactiveLevel(LevelManager.Instance.currentLevel);
        lostPanel.SetActive(false);
        LevelManager.Instance.currentLevel = 0;
        Player.Instance.currentGameState = GameState.WaitToStart;
        lostPanel.SetActive(false);
        completeLevelPanel.SetActive(false);
        homePanel.SetActive(true);
        ingamePanel.SetActive(false);
        winGamePanel.SetActive(false);
        achievementPanel.SetActive(false);
    }

}

