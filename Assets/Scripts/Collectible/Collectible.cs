using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public class Collectible : MonoBehaviour
{

    [SerializeField] private int itemValue;
    [SerializeField] private TMP_Text valueText;

    private int score;
    private Rigidbody rigidbodyItem;


    [SerializeField] private float deathTime;

    [SerializeField] private TextMeshProUGUI collectibleText;

    void Start() { UpdateText(); rigidbodyItem = GetComponent<Rigidbody>(); }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //CarController player = other.GetComponent<CarController>();
            Vector3 Direction = (other.transform.position - transform.position).normalized;
      //      rigidbodyItem.AddForce(Direction);
            transform.Translate(Direction);
        }

        if (Timer.instance.timeLeft > 0 && other.gameObject.CompareTag("Player"))
        {

            valueText.text    = itemValue.ToString();
            valueText.enabled = true;
            CollectibleHandle.Instance.UpdateText(itemValue);

            Destroy(this.gameObject);
        }
    

            score += itemValue;
            UpdateText();
            Destroy(this.gameObject, deathTime);
        
    }

    private void UpdateText()
    {
        collectibleText.text = score.ToString();
    }

}
