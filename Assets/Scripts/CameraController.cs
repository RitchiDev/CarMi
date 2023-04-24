using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Vector3 m_Offset = new Vector3();
    [SerializeField] private Transform m_Target;
    [SerializeField] private float m_MoveSpeed;
    [SerializeField] private float m_RotationSpeed;
    private Vector3 m_StartPosition;

    [Header("Camera Shake")]
    [SerializeField] private float m_ShakeStrength = 1f;
    private bool m_ScreenShakeIsOn;
    private bool m_ShakeCamera;

    [Header("Camera Pulse")]
    [SerializeField] private float m_PulseStrength = 10;
    [SerializeField] private float m_PulseDuration = 0.5f;
    private float m_StartFieldOfView;

    private void Awake()
    {
        // 1 = true, 0 = false;
        m_ScreenShakeIsOn = Convert.ToBoolean(PlayerPrefs.GetInt("ScreenShakeProperty", 1));
    }

    private void Start()
    {
        m_StartPosition = m_Camera.transform.localPosition;
    }

    private void Update()
    {
        HandleCameraShake();
    }

    private void LateUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    public void HandleCameraShake()
    {
        if(!m_ScreenShakeIsOn)
        {
            return;
        }

        if(m_ShakeCamera)
        {
            m_Camera.transform.localPosition = m_StartPosition + UnityEngine.Random.insideUnitSphere * m_ShakeStrength;
        }
        else
        {
            m_Camera.transform.localPosition = m_StartPosition;
        }
    }

    public void ShakeCamera(bool value)
    {
        m_ShakeCamera = value;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = m_Target.TransformPoint(m_Offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, m_MoveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 direction = m_Target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);
    }


    public void Pulse(float duration)
    {
        if (!m_ScreenShakeIsOn)
        {
            return;
        }

        StartCoroutine(PulseTimer(duration));
    }

    public void Pulse()
    {
        if (!m_ScreenShakeIsOn)
        {
            return;
        }

        StartCoroutine(PulseTimer(m_PulseDuration));
    }

    private IEnumerator PulseTimer(float duration)
    {
        float elapsedTime = 0f;

        m_Camera.fieldOfView -= m_PulseStrength;

        while (elapsedTime < duration && gameObject.activeInHierarchy)
        {
            elapsedTime += Time.deltaTime;
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_StartFieldOfView, elapsedTime / duration);

            yield return null;
        }

        m_Camera.fieldOfView = m_StartFieldOfView;
    }
}
