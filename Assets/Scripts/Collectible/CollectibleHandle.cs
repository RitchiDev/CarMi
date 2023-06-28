using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectibleHandle : MonoBehaviour
{ 
    public static CollectibleHandle Instance;

    private int score;

    [SerializeField] private TextMeshProUGUI collectibleText;

    void Start() { UpdateText(0); }

    public void UpdateText(int value)
    {
        score += value;
        collectibleText.text = score.ToString();
    }

}