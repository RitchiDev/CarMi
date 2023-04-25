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
    private InputAction m_MovementAction;
    private InputAction m_ChargeJumpAction;

    private bool m_AllWheelsAreGrounded;
    private bool m_InAir;

    private float m_HorizontalInput;
    private float m_VerticalInput;
    private bool m_IsChargingJump;

    private Rigidbody m_Rigidbody;
    [SerializeField] private CameraController m_CameraController;

    [SerializeField] private List<WheelConfig> m_Wheels = new List<WheelConfig>();

    [Header("In Air")]
    [SerializeField] private float m_InAirMovementSpeed;
    [SerializeField] private ParticleSystem m_PoofEffect;

    [Header("Jump")]
    [SerializeField] private float m_JumpLength = 100f;
    [SerializeField] private float m_TimeToTarget = 5f;
    [SerializeField] private Color m_ChargeColor = Color.yellow;
    [SerializeField] private Color m_MaxChargeColor = Color.red;
    [SerializeField] private ParticleSystem m_JumpStreamEffect;

    [Header("Charge")]
    [SerializeField] private float m_MaxJumpChargeTime = 3f;
    [SerializeField] private TMP_Text m_JumpChargeTimeText;
    [SerializeField] private Image m_JumpChargeImage;
    [SerializeField] private ParticleSystem m_JumpChargeEffect;
    private float m_JumpChargeTimer;

    [Header("Flight Sound")]
    [SerializeField] private AudioSource m_JumpAudioSource;
    [SerializeField] private AudioSource m_ThudAudioSource;
    //[SerializeField] private AudioSource m_InAirAudioSource;

    [Header("Charge Sound")]
    [SerializeField] private AudioSource m_ChargeAudioSource;
    [SerializeField] float m_PitchOffset = 0.5f;

    [Header("Layers")]
    [SerializeField] protected LayerMask m_LayerMask;

    [Header("Collision")]
    [SerializeField] protected float m_CollisionRayLength = 2.25f;
    [SerializeField] protected Vector3 m_CollisionRayOffset = Vector3.zero;
    private Vector3 m_PositionLastFrame;

    private void Awake()
    {
        m_Input = new PlayerInput();
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        m_PositionLastFrame = transform.position;
    }

    private void OnEnable()
    {
        m_MovementAction = m_Input.Player.Movement;
        m_MovementAction.Enable();

        m_ChargeJumpAction = m_Input.Player.ChargeJump;
        m_ChargeJumpAction.Enable();

        //m_MovementAction.performed += UpdateMovementValue;
    }

    private void OnDisable()
    {
        m_ChargeJumpAction.Disable();
        m_MovementAction.Disable();

        //m_MovementAction.performed -= UpdateMovementValue;
    }

    private void Update()
    {
        GetInput();

        UpdateVisuals();

        JumpTimer();

        CheckForJump();

        m_PositionLastFrame = transform.position;
    }

    private void FixedUpdate()
    {
        CheckForGrounded();

        CheckForGroundHit();

        HandleInAirMovement();
    }

    private void GetInput()
    {
        m_HorizontalInput = m_MovementAction.ReadValue<Vector2>().x;
        m_VerticalInput = m_MovementAction.ReadValue<Vector2>().y;

        m_IsChargingJump = m_ChargeJumpAction.IsPressed();
    }

    private void HandleInAirMovement()
    {
        if (!m_InAir)
        {
            return;
        }

        // Should be rigidbody.rotation, but since that doesn't work and this a game jame... *shrug*
        transform.rotation = Quaternion.LookRotation(m_Rigidbody.velocity, Vector3.up);

        m_Rigidbody.AddForce(m_Rigidbody.transform.right * m_HorizontalInput * m_InAirMovementSpeed);
    }

    private void CheckForGroundedWithRay()
    {
        Vector3 toLastPosition = CurrentAndLastPositionDifferenceWithOffset().normalized;
        float distance = CurrentAndLastPositionDifferenceWithOffset().magnitude + m_CollisionRayLength;

        RaycastHit hit;

        //Physics.RaycastAll(new Ray(m_PositionLastFrame, (transform.position - m_PositionLastFrame).normalized), (transform.position - m_PositionLastFrame).magnitude);

        if (Physics.Raycast(m_PositionLastFrame + m_CollisionRayOffset, toLastPosition, out hit, distance, m_LayerMask))
        {
            if (hit.transform.gameObject != null)
            {

            }
            else
            {

            }
        }
    }

    private Vector3 CurrentAndLastPositionDifferenceWithOffset()
    {
        Vector3 transformPosition = transform.position + m_CollisionRayOffset;
        Vector3 positionLastFrame = m_PositionLastFrame + m_CollisionRayOffset;
        return (transformPosition - positionLastFrame);
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

    private void CheckForGroundHit()
    {
        bool groundHit = false;

        RaycastHit hit;
        Ray checkGround = new Ray(m_Rigidbody.position, Vector3.down);

        // This should be reversed DRY
        if (m_InAir)
        {
            if (Physics.Raycast(checkGround, out hit, 2f, m_LayerMask))
            {
                groundHit = true;
                Debug.Log("Hit Ground");
            }
        }
        else
        {
            if (Physics.Raycast(checkGround, out hit, 2f, m_LayerMask))
            {
                if(m_CameraController.FOVIsDifferentThanStart())
                {
                    Debug.Log("Reset FOV");

                    m_CameraController.ResetFOV();
                }
            }
        }

        //for (int i = 0; i < m_Wheels.Count; i++)
        //{
        //    WheelConfig wheelConfig = m_Wheels[i];
        //    if (wheelConfig.GetWheelCollider().GetGroundHit(out WheelHit hit))
        //    {
        //        if (m_InAir)
        //        {
        //            Debug.Log("Wheel on ground");
        //        }

        //        groundHit = true;
        //    }
        //}

        if (groundHit && m_InAir && m_AllowHitGround)
        {
            Debug.Log("Hit Ground");

            //m_CameraController.SetShakeMultiplier(1f);
            //m_CameraController.ShakeCamera(false);
            m_ThudAudioSource.Play();
            m_PoofEffect.Play();

            m_CameraController.ResetFOV();
            m_JumpStreamEffect.gameObject.SetActive(false);

            m_CameraController.TimedShakeCamera(0.2f);

            m_JumpChargeTimer = 0f;
            m_InAir = false;
        }
    }

    private void CheckForJump()
    {
        if (m_JumpChargeTimer >= m_MaxJumpChargeTime)
        {
            if (!m_IsChargingJump && !m_InAir)
            {
                Jump();
            }
        }
    }

    private void JumpTimer()
    {
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
    }

    private void UpdateVisuals() // And charge audio
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
                m_JumpChargeImage.gameObject.SetActive(true);
                m_JumpChargeImage.color = m_MaxChargeColor;
            }
            else if (m_JumpChargeTimer < m_MaxJumpChargeTime && m_JumpChargeTimer > 0)
            {
                m_JumpChargeImage.gameObject.SetActive(true);
                m_JumpChargeImage.color = m_ChargeColor;
            }
            else
            {
                m_JumpChargeImage.gameObject.SetActive(false);
            }
        }

        // Charge particle effect
        if (m_JumpChargeEffect != null)
        {
            if (m_JumpChargeTimer > 0f)
            {
                //m_ChargeAudioSource.pitch = Mathf.Clamp((m_JumpChargeTimer / m_MaxJumpChargeTime), 0.15f, 1f);

                if (!m_ChargeAudioSource.isPlaying)
                {
                    m_ChargeAudioSource.Play();
                }

                m_JumpChargeEffect.gameObject.SetActive(true);
            }
            else
            {
                if (m_ChargeAudioSource.isPlaying)
                {
                    m_ChargeAudioSource.Stop();
                }

                m_JumpChargeEffect.gameObject.SetActive(false);
            }
        }

        // Screenshake
        if (m_CameraController != null)
        {
            if (m_JumpChargeTimer > 0f)
            {
                m_CameraController.ShakeCamera(true);
            }
            else if (m_JumpChargeTimer <= 0f && m_JumpChargeTimer > -1f && m_AllWheelsAreGrounded)
            {
                m_CameraController.ShakeCamera(false);
            }
        }
    }

    private bool m_AllowHitGround;

    private IEnumerator DelayedJump()
    {
        Debug.DrawRay(m_Rigidbody.position, m_Rigidbody.transform.forward * m_JumpLength, Color.red);

        Debug.Log("Jumped");

        m_JumpAudioSource.Play();
        m_PoofEffect.Play();

        m_AllowHitGround = false; //
        m_AllWheelsAreGrounded = false; //
        m_JumpChargeTimer = -2f; //

        m_JumpStreamEffect.gameObject.SetActive(true);

        m_CameraController.ShakeCamera(false);
        m_CameraController.SetShakeMultiplier(1.5f);
        m_CameraController.PulseAndFov(0.1f, m_TimeToTarget * 0.9f, 110f);

        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        Vector3 targetPoint = m_Rigidbody.position + m_Rigidbody.transform.forward * m_JumpLength;

        float timeToTarget = m_TimeToTarget;

        Vector3 throwSpeed = CalculateJump(m_Rigidbody.position, targetPoint, timeToTarget);

        m_Rigidbody.AddForce(throwSpeed, ForceMode.VelocityChange);

        m_InAir = true; //

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);

        m_AllowHitGround = true; // 
    }

    private void Jump()
    {
        StartCoroutine(DelayedJump());
    }

    public bool AllWheelsAreGrounded()
    {
        return m_AllWheelsAreGrounded;
    }

    private Vector3 CalculateJump(Vector3 origin, Vector3 target, float timeToTarget)
    {
        // calculate vectors
        Vector3 toTarget = target - origin;
        Vector3 toTargetXZ = toTarget;
        toTargetXZ.y = 0;

        // calculate xz and y
        float y = toTarget.y;
        float xz = toTargetXZ.magnitude;

        // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
        // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
        // so xz = v0xz * t => v0xz = xz / t
        // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
        float t = timeToTarget;

        float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
        float v0xz = xz / t;

        // create result vector for calculated starting speeds
        Vector3 result = toTargetXZ.normalized;        // get direction of xz but with magnitude 1
        result *= v0xz;                                // set magnitude of xz to v0xz (starting speed in xz plane)
        result.y = v0y;                                // set y to v0y (starting speed of y plane)

        return result;
    }
}
