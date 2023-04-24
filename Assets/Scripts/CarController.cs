using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    private PlayerInput m_Input;
    private InputAction m_MovementAction;
    private InputAction m_BrakeAction;

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

    [SerializeField] AnimationCurve m_MotorTorqueFromRpmCurve;
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

    private void Awake()
    {
        m_Input = new PlayerInput();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.centerOfMass = m_CenterOfMass.localPosition;
    }

    private void OnEnable()
    {
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
        m_CurrentSpeed = m_Rigidbody.velocity.magnitude;
		m_VelocityAngle = -Vector3.SignedAngle(m_Rigidbody.velocity, transform.TransformDirection (Vector3.forward), Vector3.up);

        GetInput();

        UpdateWheelsVisual();

        CheckForGrounded();
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
