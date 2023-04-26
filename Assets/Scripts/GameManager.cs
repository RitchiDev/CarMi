using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum GameState
{
    Playing = 0,
    Paused = 1,
    Victory = 2,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private GameState m_CurrentGameState = GameState.Playing;

    [SerializeField] private GameObject m_PauseMenu;
    [SerializeField] private GameObject m_VictoryMenu;
    [SerializeField] private Selectable m_FirstSelectedOnVictory;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("An instance of " + this + " already existed!");
            Destroy(this);
        }
    }

    public void SetGameState(GameState gameState)
    {
        m_CurrentGameState = gameState;

        if(gameState == GameState.Victory)
        {
            AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();

            for (int i = 0; i < allAudioSources.Length; i++)
            {
                allAudioSources[i].Stop();
            }

            EventSystem.current.SetSelectedGameObject(m_FirstSelectedOnVictory.gameObject);

            m_PauseMenu.SetActive(false);
            m_VictoryMenu.SetActive(true);
        }
    }

    public GameState GetCurrentGameState()
    {
        return m_CurrentGameState;
    }

    public void SetGameTime(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
