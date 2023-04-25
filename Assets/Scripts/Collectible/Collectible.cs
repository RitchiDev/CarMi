using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collectible : MonoBehaviour
{
    private int score;
    [SerializeField] private int itemValue;

    [SerializeField] private TextMeshProUGUI collectibleText;

    void Start() { UpdateText(); }

    private void OnTriggerEnter(Collider other)
    {
        if (Timer.instance.timeLeft > 0 && other.gameObject.CompareTag("Player"))
        {
            score += itemValue;
            UpdateText();
            Destroy(this.gameObject);
        }
    }

    private void UpdateText()
    { 
        collectibleText.text = score.ToString();     
    }
}