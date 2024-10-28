using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreManager : Singleton<HighScoreManager>
{
    [SerializeField] Text[] highScoreTexts;

    List<string> names;
    List<int> scores;
    List<int> minutes;
    List<int> seconds;

    private const int MaxHighScores = 5;
    public void SaveHighScore(string playerName, int playerScore, int playedMinutes, int playedSeconds)
    {
        if (scores == null) scores = new List<int>();
        if (names == null) names = new List<string>();
        if (minutes == null) minutes = new List<int>();
        if (seconds == null) seconds = new List<int>();

        for (int i = 0; i < MaxHighScores; i++)
        {
            string savedName = PlayerPrefs.GetString("HighScoreName" + i, "");
            int savedScore = PlayerPrefs.GetInt("Score" + i, -1);
            int savedMinutes = PlayerPrefs.GetInt("Minute" + i, -1);
            int savedSeconds = PlayerPrefs.GetInt("Second" + i, -1);

            if (savedScore != -1)
            {
                scores.Add(savedScore);
                names.Add(savedName);
                minutes.Add(savedMinutes);
                seconds.Add(savedSeconds);
            }
        }
        //add current played data
        scores.Add(playerScore);
        names.Add(playerName);
        minutes.Add(playedMinutes);
        seconds.Add(playedSeconds);

        // sort data
        for (int i = 0; i < scores.Count - 1; i++)
        {
            for (int j = i + 1; j < scores.Count; j++)
            {
                if (scores[j] > scores[i])
                {
                    SwapData(i, j);
                }
                else if (scores[j] == scores[i])
                {
                    if (minutes[j] < minutes[i])
                    {
                        SwapData(i, j);
                    }
                    else if (minutes[j] == minutes[i])
                    {
                        if (seconds[j] < seconds[i])
                        {
                            SwapData(i, j);
                        }
                    }
                }
            }
        }
        // save only 5 data
        if (scores.Count > MaxHighScores)
        {
            scores.RemoveRange(MaxHighScores, scores.Count - MaxHighScores);
            names.RemoveRange(MaxHighScores, names.Count - MaxHighScores);
            minutes.RemoveRange(MaxHighScores, minutes.Count - MaxHighScores);
            seconds.RemoveRange(MaxHighScores, seconds.Count - MaxHighScores);
        }

        // save new data
        for (int i = 0; i < MaxHighScores; i++)
        {
            if (i < scores.Count)
            {
                PlayerPrefs.SetString("HighScoreName" + i, names[i]);
                PlayerPrefs.SetInt("HighScore" + i, scores[i]);
                PlayerPrefs.SetInt("Minutes" + i, minutes[i]);
                PlayerPrefs.SetInt("Seconds" + i, seconds[i]);
            }
        }
        PlayerPrefs.Save();
    }
    public void LoadHighScores()
    {
        if (highScoreTexts == null)
        {
            Debug.LogError("HighScoreTexts array is not assigned or is empty!");
            return;
        }
        for (int i = 0; i < MaxHighScores; i++)
        {
            string name = PlayerPrefs.GetString("HighScoreName" + i, "N/A");
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            int minutes = PlayerPrefs.GetInt("Minutes" + i, 0);
            int seconds = PlayerPrefs.GetInt("Seconds" + i, 0);
            Debug.Log("Name: " + name + "\n"
                                     + "Score: " + score + "\n"
                                     + "Time Played: " + minutes + ":" + seconds);

            highScoreTexts[i].text = "Name: " + name + "\n"
                                     + "Score: " + score + "\n"
                                     + "Time Played: " + minutes + ":" + seconds;
        }
    }
    private void SwapData(int i, int j)
    {
        int tempScore = scores[i];
        scores[i] = scores[j];
        scores[j] = tempScore;

        string tempName = names[i];
        names[i] = names[j];
        names[j] = tempName;

        int tempMinute = minutes[i]; ;
        minutes[i] = minutes[j];
        minutes[j] = tempMinute;

        int tempSecond = seconds[i]; ;
        seconds[i] = seconds[j];
        seconds[j] = tempSecond;
    }
}
