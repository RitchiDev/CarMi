using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{

    public float totalTime;

    private float timeLeft;

    [SerializeField] private TextMeshProUGUI timerText;

    private void Start() { timeLeft = totalTime; }

    private void Update()
    {
        timeLeft   -= Time.deltaTime;

        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);


        string timeLeftString = string.Format("{0:0}:{1:00}", minutes, seconds);

        timerText.text = timeLeftString;

        if (timeLeft <= 0) { DoSomething(); }
    }

    private void DoSomething()
    {
        timerText.text = "Time up!";
    }
}