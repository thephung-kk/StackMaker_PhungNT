using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private GameObject player;
    [SerializeField] public GameObject[] levels;
    [SerializeField] private Transform[] startPoints;
    [SerializeField] private float playerHeight;

    public int currentLevel = 0;
    private Vector3 startPosition;

    public void ActivateLevel(int levelIndex)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(i == levelIndex);
        }
        Player.Instance.currentGameState = GameState.InGame;
        player.transform.SetParent(levels[levelIndex].transform);
        startPosition = startPoints[levelIndex].position;
        startPosition.y += playerHeight;
        player.transform.position = startPosition; ;
    }
    public void CompleteLevel()
    {
        if (currentLevel < levels.Length - 1)
        {
            currentLevel++;
            ActivateLevel(currentLevel);
        }
    }
    public void DeactiveLevel(int levelIndex)
    {
        levels[levelIndex].SetActive(false);
    }
}