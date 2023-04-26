using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TMP_Text m_HighscoreText;
    [SerializeField] private TMP_Text m_CurrentScoreText;
    [SerializeField] private TMP_Text m_VictoryHeaderText;

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

    private void OnEnable()
    {
        SetGameState(GameState.Playing);
        SetGameTime(1f);
    }

    public void VictoryButActuallyGameOver()
    {
        m_VictoryHeaderText.text = "Game Over";
        SetGameState(GameState.Victory);
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

            int currentHighscore = PlayerPrefs.GetInt("Highscore", 0);

            if(currentHighscore < ExScoreManager.Instance.GetScore())
            {
                PlayerPrefs.SetInt("Highscore", ExScoreManager.Instance.GetScore());
            }

            m_HighscoreText.text = PlayerPrefs.GetInt("Highscore", 0).ToString();

            m_CurrentScoreText.text = ExScoreManager.Instance.GetScore().ToString();

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
