using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Camera m_Camera;
    [SerializeField] private Vector3 m_Offset = new Vector3();
    [SerializeField] private Transform m_Target;
    [SerializeField] private float m_MoveSpeed;
    [SerializeField] private float m_RotationSpeed;
    private Vector3 m_StartPosition;

    [Header("Camera Shake")]
    [SerializeField] private AnimationCurve m_TimedShakeStrengthCurve;
    [SerializeField] private float m_ShakeStrength = 1f;
    private float m_ShakeMultiplier = 1f;
    private bool m_ScreenShakeIsOn;
    private bool m_ShakeCamera;

    [Header("Camera Pulse")]
    [SerializeField] private float m_PulseStrength = 10;
    [SerializeField] private float m_PulseDuration = 0.5f;
    private float m_StartFieldOfView;

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

        // 1 = true, 0 = false;
        m_ScreenShakeIsOn = Convert.ToBoolean(PlayerPrefs.GetInt("ScreenShakeProperty", 1));
    }

    private void Start()
    {
        m_StartPosition = m_Camera.transform.localPosition;
        m_StartFieldOfView = m_Camera.fieldOfView;
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
    
    public void TimedShakeCamera(float duration)
    {
        if (!m_ScreenShakeIsOn)
        {
            return;
        }

        StartCoroutine(ShakeTimer(duration));
    }

    private IEnumerator ShakeTimer(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration && gameObject.activeInHierarchy)
        {
            elapsedTime += Time.deltaTime;
            float strength = m_TimedShakeStrengthCurve.Evaluate(elapsedTime / duration);
            m_Camera.transform.localPosition = m_StartPosition + UnityEngine.Random.insideUnitSphere * strength * 10f;

            yield return null;
        }

        m_Camera.transform.localPosition = m_StartPosition;
    }

    public void HandleCameraShake()
    {
        if(!m_ScreenShakeIsOn)
        {
            return;
        }

        if(m_ShakeCamera)
        {
            m_Camera.transform.localPosition = m_StartPosition + UnityEngine.Random.insideUnitSphere * m_ShakeStrength * m_ShakeMultiplier;
        }
        else
        {
            m_Camera.transform.localPosition = m_StartPosition;
        }
    }

    public void SetShakeMultiplier(float value)
    {
        m_ShakeMultiplier = value;
    }

    public void ShakeCamera(bool value)
    {
        m_ShakeCamera = value;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = m_Target.TransformPoint(m_Offset);

        if(transform.position != targetPosition)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_MoveSpeed * Time.deltaTime);
        }
    }

    private void HandleRotation()
    {
        Vector3 direction = m_Target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);
    }

    public void ResetFOV()
    {
        Pulse(0.5f);
    }

    public void PulseAndFov(float pulseDuration, float FOVReachTime, float FOV)
    {
        if (!m_ScreenShakeIsOn)
        {
            return;
        }

        StartCoroutine(PulseTimer(pulseDuration));
        StartCoroutine(JumpFOV(pulseDuration, FOVReachTime, FOV));
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

    private IEnumerator JumpFOV(float delay, float duration, float FOV)
    {
        float elapsedDelay = 0f;

        while (elapsedDelay < (delay + 0.1f) && gameObject.activeInHierarchy)
        {
            elapsedDelay += Time.deltaTime;

            yield return null;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration && gameObject.activeInHierarchy)
        {
            elapsedTime += Time.deltaTime;
            //m_Camera.fieldOfView = Mathf.Lerp(m_StartFieldOfView, FOV, elapsedTime / duration);
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, FOV, elapsedTime / duration);

            yield return null;
        }

        m_Camera.fieldOfView = FOV;
    }

    public bool FOVIsDifferentThanStart()
    {
        return m_Camera.fieldOfView != m_StartFieldOfView;
    }
}
