using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadHighscore : MonoBehaviour
{
    [SerializeField] private TMP_Text m_HighscoreText;

    private void Start()
    {
        m_HighscoreText.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
    }
}
