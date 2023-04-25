using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collectible : MonoBehaviour
{
    private int score;
    private Rigidbody rigidbodyItem;

    [SerializeField] private int itemValue;
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
            score += itemValue;
            UpdateText();
            Destroy(this.gameObject, deathTime);
        }
    }

    private void UpdateText()
    {
        collectibleText.text = score.ToString();
    }
}