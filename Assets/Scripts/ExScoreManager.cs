using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExScoreManager : MonoBehaviour
{
    public static ExScoreManager Instance { get; private set; }

    [SerializeField] private JumpController m_JumpController;

    [SerializeField] private TMP_Text m_ScoreText;

    [SerializeField] private Image m_MultiplierBar;
    [SerializeField] private TMP_Text m_MultiplierScoreText;

    private int m_CurrentTextScore;

    [SerializeField] private float m_ScoreLerpTime = 1f;

    private int m_CurrentScore;
    private int m_MaxScore = 999999999;

    private int m_ScoreMultiplier = 1;
    private float m_MaxScoreMultiplerTime = 3f;
    private float m_CurrentScoreMultiplierTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("An instance of " + this + " already existed!");
            Destroy(this);
        }

        m_ScoreText.text = m_CurrentTextScore.ToString("000000000");
        m_MultiplierScoreText.text = $"Multi: x{m_ScoreMultiplier}";
    }

    private void Update()
    {
        ScoreMultiplierTimer();   
    }

    private void ScoreMultiplierTimer()
    {
        if (m_CurrentScoreMultiplierTime > 0)
        {
            m_CurrentScoreMultiplierTime -= Time.deltaTime;
            m_MultiplierBar.gameObject.SetActive(true);
            m_MultiplierScoreText.gameObject.SetActive(true);
        }
        else
        {
            m_ScoreMultiplier = 1;
            m_MultiplierBar.gameObject.SetActive(false);
            m_MultiplierScoreText.gameObject.SetActive(false);
        }

        m_MultiplierBar.fillAmount = m_CurrentScoreMultiplierTime / m_MaxScoreMultiplerTime;
    }

    public void AddScore(int score)
    {
        m_ScoreMultiplier++;

        if(m_ScoreMultiplier > 1)
        {
            m_CurrentScoreMultiplierTime = m_MaxScoreMultiplerTime;
        }

        m_MultiplierScoreText.text = $"Multi: x{m_ScoreMultiplier}";
        m_CurrentScore = Mathf.Clamp(m_CurrentScore + (score * m_ScoreMultiplier), 0, m_MaxScore);

        StartCoroutine(CountTo(m_CurrentScore));
    }

    public int GetScore()
    {
        return m_CurrentScore;
    }

    private IEnumerator CountTo(int target)
    {
        int start = m_CurrentTextScore;
        float timer = m_ScoreLerpTime;

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            float progress = timer / m_ScoreLerpTime;
            m_CurrentTextScore = (int)Mathf.Lerp(start, target, progress);

            m_ScoreText.text = m_CurrentTextScore.ToString("000000000");

            yield return null;
        }

        m_CurrentTextScore = target;
        m_ScoreText.text = m_CurrentTextScore.ToString("000000000");
    }
}
