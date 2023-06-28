using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bonk : MonoBehaviour
{
    [SerializeField] private AudioClip      m_BonkSound;
    [SerializeField] private ParticleSystem m_ParticleSystem;

    private Rigidbody m_RigidBody;
    private Collider  m_Collider;

    private void Awake()
    {
        m_Collider  = GetComponent<Collider>();
        m_RigidBody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController player         = other.gameObject.GetComponent<CarController>();
        CarController playerInParent = other.gameObject.GetComponentInParent<CarController>();

        if (player)
        {
            Debug.Log("Hit");
            Vector3 direction = (transform.position - player.transform.position).normalized;
            
            StartCoroutine(DelayedShoot(direction));

            StartCoroutine(Deactivate());
        }
        else if (playerInParent)
        {
            Debug.Log("Hit Child");
            Vector3 direction = (transform.position - playerInParent.transform.position).normalized;

            StartCoroutine(DelayedShoot(direction));

            StartCoroutine(Deactivate());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //CarController player = collision.gameObject.GetComponent<CarController>();

        //if(player)
        //{
        //    Debug.Log("Hit");

        //    Vector3 direction = (player.transform.position - transform.position).normalized;
        //    m_RigidBody.AddForce(Vector3.up * 5f);
        //    m_RigidBody.AddForce(direction);

        //    StartCoroutine(Deactivate());
        //}
    }

    private IEnumerator DelayedShoot(Vector3 direction)
    {
        m_ParticleSystem.Play();
        CameraController.Instance.TimedShakeCamera(0.1f);

        ExScoreManager.Instance.AddScore(5);
        AudioSource.PlayClipAtPoint(m_BonkSound, transform.position);

        m_Collider.isTrigger   = false;
        m_RigidBody.useGravity = true;
        m_Collider.enabled     = false;

        yield return new WaitForSeconds(0.1f);

        m_RigidBody.AddForce(1000f * 2f * Vector3.up);
        m_RigidBody.AddForce(1555f * 2f * direction);
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}
