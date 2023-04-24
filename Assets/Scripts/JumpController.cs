using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JumpController : MonoBehaviour
{
    private PlayerInput m_Input;
    private InputAction m_ChargeJumpAction;
    private bool m_IsChargingJump;
    private bool m_AllWheelsAreGrounded;

    private Rigidbody m_Rigidbody;
    [SerializeField] private CameraController m_CameraController;

    [SerializeField] private List<WheelConfig> m_Wheels = new List<WheelConfig>();

    [Header("Charge")]
    [SerializeField] private float m_MaxJumpChargeTime = 3f;
    [SerializeField] private TMP_Text m_JumpChargeTimeText;
    [SerializeField] private Image m_JumpChargeImage;
    [SerializeField] private ParticleSystem m_JumpChargeEffect;
    private float m_JumpChargeTimer;

    private void Awake()
    {
        m_Input = new PlayerInput();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        m_ChargeJumpAction = m_Input.Player.ChargeJump;
        m_ChargeJumpAction.Enable();

        //m_MovementAction.performed += UpdateMovementValue;
    }

    private void OnDisable()
    {
        m_ChargeJumpAction.Disable();
        //m_MovementAction.performed -= UpdateMovementValue;
    }

    private void Update()
    {
        m_IsChargingJump = m_ChargeJumpAction.IsPressed();

        CheckForGrounded();

        CheckForJump();
    }

    private void CheckForGrounded()
    {
        m_AllWheelsAreGrounded = true;

        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];
            if(!wheelConfig.GetIfIsGrounded())
            {
                m_AllWheelsAreGrounded = false;
            }
        }
    }

    private void CheckForJump()
    {
        if (m_JumpChargeTimer >= m_MaxJumpChargeTime)
        {
            if (!m_IsChargingJump)
            {
                m_JumpChargeTimer = 0;
                Jump();
            }
        }

        if (m_IsChargingJump && m_AllWheelsAreGrounded)
        {
            m_JumpChargeTimer += Time.deltaTime;
        }
        else
        {
            if (m_JumpChargeTimer > 0 && m_JumpChargeTimer < m_MaxJumpChargeTime)
            {
                m_JumpChargeTimer -= Time.deltaTime * m_MaxJumpChargeTime;
            }
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Charge timer
        if(m_JumpChargeTimeText != null)
        {
            m_JumpChargeTimeText.text = m_JumpChargeTimer.ToString("0");
        }

        // Charge image/slider
        if(m_JumpChargeImage != null)
        {
            m_JumpChargeImage.fillAmount = m_JumpChargeTimer / m_MaxJumpChargeTime;

            if (m_JumpChargeTimer >= m_MaxJumpChargeTime)
            {
                m_JumpChargeImage.gameObject.SetActive(false);
            }
            else
            {
                m_JumpChargeImage.gameObject.SetActive(true);
            }
        }

        // Charge particle effect
        if (m_JumpChargeEffect != null)
        {
            if (m_JumpChargeTimer > 0f)
            {
                m_JumpChargeEffect.gameObject.SetActive(true);
            }
            else
            {
                m_JumpChargeEffect.gameObject.SetActive(false);
            }
        }

        // Screenshake
        if(m_CameraController != null)
        {
            if (m_JumpChargeTimer > 0f)
            {
                m_CameraController.ShakeCamera(true);
            }
            else
            {
                m_CameraController.ShakeCamera(false);
            }
        }
    }

    private void Jump()
    {
        Debug.Log("Jumped");
    }
}
