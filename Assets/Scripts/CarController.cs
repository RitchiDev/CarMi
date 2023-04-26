using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    private PlayerInput m_Input;
    private InputAction m_MovementAction;
    private InputAction m_BrakeAction;
    private JumpController m_JumpController;

    private float m_HorizontalInput;
    private float m_VerticalInput;
    private bool m_IsBraking;

    private float m_CurrentSteerAngle;
    private float m_CurrentbreakForce;
    private float m_CurrentAcceleration;
    private float m_CurrentSpeed;
    private float m_VelocityAngle;
    public float VelocityAngle => m_VelocityAngle;
	public int CarDirection { get { return m_CurrentSpeed < 1 ? 0 : (m_VelocityAngle < 90 && m_VelocityAngle > -90 ? 1 : -1); } }

    [SerializeField] private AnimationCurve m_MotorTorqueFromRpmCurve;
    private float m_EngineRPM;

    [SerializeField] private float m_RpmEngineToRpmWheelsLerpSpeed;
    [SerializeField] private float m_MaxMotorTorque;

    [SerializeField] private float m_MaxRPM;
    [SerializeField] private float m_MinRPM;

    [SerializeField] private float m_BreakForce;
    [SerializeField] private float m_MaxSteerAngle;

    [SerializeField] private float m_UprightTurnSpeed = 10f;
    [SerializeField] private Transform m_CenterOfMass;
    private Rigidbody m_Rigidbody;
    private bool m_AllWheelsAreGrounded;

    [SerializeField] private List<WheelConfig> m_Wheels = new List<WheelConfig>();

    [Header("FL")]
    [SerializeField] private WheelCollider m_FrontLeftWheelCollider;

    [Header("FR")]
    [SerializeField] private WheelCollider m_FrontRightWheelCollider;

    [Header("Fuel")]
    [SerializeField] TMP_Text m_SpeedText;
    [SerializeField] RectTransform m_FuelArrow;
    [SerializeField] private Image m_FuelImage;
    [SerializeField] private float m_MaxFuel = 120f;
    [SerializeField] float m_MinArrowAngle = 0;
    [SerializeField] float m_MaxArrowAngle = -315f;
    private float m_CurrentFuel;

    [Header("Effect")]
    [SerializeField] private ParticleSystem m_AsphaltSmokeEffect;
    public ParticleSystem AsphaltSmokeEffect => m_AsphaltSmokeEffect;
    [SerializeField] private TrailRenderer m_TrailRenderer; // Trail renderer, The lifetime of the tracks is configured in it.
    public TrailRenderer TrailRenderer => m_TrailRenderer;
    [SerializeField] private Transform m_TrailHolder; // Parent for copy of the trail renderer.
    private Queue<TrailRenderer> m_AvailableTrails = new Queue<TrailRenderer>();

    [Header("Engine Sound")]
    [SerializeField] private AudioSource m_EngineAudioSource;
    [SerializeField] float m_PitchOffset = 0.5f;

    [Header("Slip Sound")]
    [SerializeField] private AudioSource m_TireSlipAudioSource;
    [SerializeField] private float m_MinSlipSound = 0.15f;
    [SerializeField] float m_MaxSlipForSound = 1f;
    private float m_CurrentMaxSlip;

    private void Awake()
    {
        m_Input = new PlayerInput();
        m_JumpController = GetComponent<JumpController>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.centerOfMass = m_CenterOfMass.localPosition;

        m_CurrentFuel = m_MaxFuel;

        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];
            wheelConfig.SetController(this);
        }
    }

    private void OnEnable()
    {
        m_CurrentMaxSlip = m_Wheels[0].CurrentMaxSlip;

        m_MovementAction = m_Input.Player.Movement;
        m_MovementAction.Enable();

        m_BrakeAction = m_Input.Player.Brake;
        m_BrakeAction.Enable();

        //m_MovementAction.performed += UpdateMovementValue;
    }

    private void OnDisable()
    {
        m_MovementAction.Disable();
        m_BrakeAction.Disable();
        //m_MovementAction.performed -= UpdateMovementValue;
    }

    private void Update()
    {
        if(transform.position.y < -10f)
        {
            GameManager.Instance.VictoryButActuallyGameOver();
            gameObject.SetActive(false);
        }

        m_CurrentSpeed = m_Rigidbody.velocity.magnitude;
		m_VelocityAngle = -Vector3.SignedAngle(m_Rigidbody.velocity, transform.TransformDirection (Vector3.forward), Vector3.up);

        GetInput();

        UpdateWheelsVisual();

        CheckForGrounded();

        HandleMaxSlip();

        UpdateAudio();

        UpdateFuelAndSpeed();

        CheckFuel();
    }

    private void FixedUpdate()
    {
        TurnUpright();

        HandleMotor();
        HandleSteering();
        UpdateWheels();
        CheckForBraking();
    }

    // If using performed
    private void UpdateMovementValue(InputAction.CallbackContext context)
    {
        m_HorizontalInput = context.ReadValue<Vector2>().x;
        m_VerticalInput = context.ReadValue<Vector2>().y;
    }

    private void GetInput()
    {
        m_HorizontalInput = m_MovementAction.ReadValue<Vector2>().x;
        m_VerticalInput = m_MovementAction.ReadValue<Vector2>().y;

        m_CurrentAcceleration = m_VerticalInput;
        m_IsBraking = m_BrakeAction.IsPressed();
    }

    private void CheckFuel()
    {
        if(m_CurrentFuel <= 0f)
        {
            GameManager.Instance.SetGameState(GameState.Victory);
            GameManager.Instance.SetGameTime(0f);
            // Game over
        }
    }

    private void UpdateFuelAndSpeed()
    {
        if (m_CurrentSpeed > 0.1f && m_JumpController.AllWheelsAreGrounded())
        {
            m_CurrentFuel -= Time.deltaTime;
        }

        m_FuelImage.fillAmount = m_CurrentFuel / m_MaxFuel;

        float procent = m_CurrentFuel / m_MaxFuel;
        float angle = (m_MaxArrowAngle - m_MinArrowAngle) * procent + m_MinArrowAngle;
        m_FuelArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        m_SpeedText.text = (m_CurrentSpeed * 3.6f).ToString("000"); // In hour
    }

    private void HandleMaxSlip()
    {
        m_CurrentMaxSlip = m_Wheels[0].CurrentMaxSlip;

        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];

            if (m_CurrentMaxSlip < wheelConfig.CurrentMaxSlip)
            {
                m_CurrentMaxSlip = wheelConfig.CurrentMaxSlip;
            }
        }
    }

    private void UpdateAudio()
    {
        // Engine PRM sound
        if (m_JumpController.AllWheelsAreGrounded())
        {
            m_EngineAudioSource.pitch = (m_EngineRPM / m_MaxRPM) + m_PitchOffset;
            //if (!m_EngineAudioSource.isPlaying)
            //{
            //    m_EngineAudioSource.Play();
            //}
        }
        else
        {
            m_EngineAudioSource.pitch = 0.5f + m_PitchOffset;

            //if (m_EngineAudioSource.isPlaying)
            //{
            //    m_EngineAudioSource.Stop();
            //}
        }

        // Slip sound logic
        if (m_MaxSlipForSound > m_MinSlipSound)
        {
            if (!m_TireSlipAudioSource.isPlaying)
            {
                m_TireSlipAudioSource.Play();
            }

            float slipVolumeProcent = m_CurrentMaxSlip / m_MaxSlipForSound;
            m_TireSlipAudioSource.volume = slipVolumeProcent * 0.5f;
            m_TireSlipAudioSource.pitch = Mathf.Clamp(slipVolumeProcent, 0.75f, 1);
        }
        else
        {
            m_TireSlipAudioSource.Stop();
        }
    }

    public TrailRenderer GetAvailableTrail(Vector3 startPosition)
    {
        TrailRenderer trail = null;
        if (m_AvailableTrails.Count > 0)
        {
            trail = m_AvailableTrails.Dequeue();
        }
        else
        {
            trail = Instantiate(m_TrailRenderer, m_TrailHolder);
        }

        trail.transform.position = startPosition;
        trail.gameObject.SetActive(true);

        return trail;
    }

    public void SetAvailableTrail(TrailRenderer trail)
    {
        StartCoroutine(WaitUntillTrailHasDissapeared(trail));
    }

    /// <summary>
	/// The trail is considered busy until it has disappeared.
	/// </summary>
	private IEnumerator WaitUntillTrailHasDissapeared(TrailRenderer trail)
    {
        trail.transform.SetParent(m_TrailHolder);
        yield return new WaitForSeconds(trail.time);
        trail.Clear();
        trail.gameObject.SetActive(false);
        m_AvailableTrails.Enqueue(trail);
    }

    private void HandleMotor()
    {
        float rpm = m_CurrentAcceleration > 0 ? m_MaxRPM : m_MinRPM;
        float speed = m_CurrentAcceleration > 0 ? m_RpmEngineToRpmWheelsLerpSpeed : m_RpmEngineToRpmWheelsLerpSpeed * 0.2f;

        m_EngineRPM = Mathf.Lerp(m_EngineRPM, rpm, speed * Time.fixedDeltaTime);

        float motorTorqueFromRpm = m_MotorTorqueFromRpmCurve.Evaluate(m_EngineRPM * 0.001f);
        float motorTorque = m_CurrentAcceleration * (motorTorqueFromRpm * m_MaxMotorTorque);

        if (!Mathf.Approximately(m_CurrentAcceleration, 0))
        {
            // If the direction of the car is the same as Current Acceleration.
            if (CarDirection * m_CurrentAcceleration >= 0)
            {
                // If the rpm of the wheel is less than the max rpm engine * current ratio, then apply the current torque for wheel, else not torque for wheel.
                float maxWheelRPM = m_EngineRPM;

                if (m_FrontLeftWheelCollider.rpm <= maxWheelRPM)
                {
                    m_FrontLeftWheelCollider.motorTorque = motorTorque;
                }
                else
                {
                    m_FrontLeftWheelCollider.motorTorque = 0;
                }

                if (m_FrontRightWheelCollider.rpm <= maxWheelRPM)
                {
                    m_FrontRightWheelCollider.motorTorque = motorTorque;
                }
                else
                {
                    m_FrontRightWheelCollider.motorTorque = 0;
                }
            }
            else
            {
                //for (int i = 0; i < m_Wheels.Count; i++)
                //{
                //    WheelConfig wheelConfig = m_Wheels[i];
                //    wheelConfig.GetWheelCollider().brakeTorque = m_BreakForce;
                //}
            }
        }
        else
        {
            m_FrontLeftWheelCollider.motorTorque = 0;
            m_FrontRightWheelCollider.motorTorque = 0;
        }

        //m_FrontLeftWheelCollider.motorTorque = m_VerticalInput * m_MotorForce;
        //m_FrontRightWheelCollider.motorTorque = m_VerticalInput * m_MotorForce;
    }

    private void CheckForBraking()
    {
        // APPLY DRY CONCEPT
        if (m_IsBraking)
        {
            for (int i = 0; i < m_Wheels.Count; i++)
            {
                WheelConfig wheelConfig = m_Wheels[i];
                wheelConfig.GetWheelCollider().brakeTorque = m_BreakForce;
            }
        }
        else
        {
            for (int i = 0; i < m_Wheels.Count; i++)
            {
                WheelConfig wheelConfig = m_Wheels[i];
                wheelConfig.GetWheelCollider().brakeTorque = 0f;
            }
        }
    }

    private void TurnUpright()
    {
        if(m_AllWheelsAreGrounded)
        {
            return;
        }

        Quaternion q = Quaternion.FromToRotation(m_Rigidbody.transform.up, Vector3.up) * m_Rigidbody.transform.rotation;
        m_Rigidbody.transform.rotation = Quaternion.Slerp(m_Rigidbody.transform.rotation, q, Time.deltaTime * m_UprightTurnSpeed);
    }

    private void HandleSteering()
    {
        m_CurrentSteerAngle = m_MaxSteerAngle * m_HorizontalInput;
        m_FrontLeftWheelCollider.steerAngle = m_CurrentSteerAngle;
        m_FrontRightWheelCollider.steerAngle = m_CurrentSteerAngle;
    }

    private void UpdateWheels()
    {
        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];
            wheelConfig.FixedUpdate();
        }
    }

    private void UpdateWheelsVisual()
    {
        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];
            wheelConfig.UpdateVisual();
        }
    }

    private void CheckForGrounded()
    {
        m_AllWheelsAreGrounded = true;

        for (int i = 0; i < m_Wheels.Count; i++)
        {
            WheelConfig wheelConfig = m_Wheels[i];
            if (!wheelConfig.GetIfIsGrounded())
            {
                m_AllWheelsAreGrounded = false;
            }
        }
    }
}
