using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int itemValue;
    [SerializeField] private TMP_Text valueText;

    private void OnTriggerEnter(Collider other)
    {
        if (Timer.instance.timeLeft > 0 && other.gameObject.CompareTag("Player"))
        {
            valueText.text    = itemValue.ToString();
            valueText.enabled = true;
            CollectibleHandle.Instance.UpdateText(itemValue);

            Destroy(this.gameObject);
        }
    }
}