using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBodyTilt : MonoBehaviour
{
    [SerializeField] private Transform Body;                                // Link to car body (Visual).
    [SerializeField] private CarController m_CarController;					// The car controller.

    [SerializeField] private float MaxAngle = 10;                           // Max tilt angle of car body.
    [SerializeField] private float AngleVelocityMultiplayer = 0.2f;         // Rotation angle multiplier when moving forward.
    [SerializeField] private float RearAngleVelocityMultiplayer = 0.4f;     // Rotation angle multiplier when moving backwards.
    [SerializeField] private float MaxTiltOnSpeed = 60;                     // The speed at which the maximum tilt is reached.

    private float Angle;

	private void Update()
	{
		if (m_CarController.CarDirection == 1)
        {
			Angle = -m_CarController.VelocityAngle * AngleVelocityMultiplayer;
        }
		else if (m_CarController.CarDirection == -1)
		{
			Angle = LoopClamp(m_CarController.VelocityAngle + 180, -180, 180) * RearAngleVelocityMultiplayer;
		}
		else
		{
			Angle = 0;
		}

		Angle = Mathf.Clamp(Angle, -MaxAngle, MaxAngle);
		Body.localRotation = Quaternion.AngleAxis(Angle, Vector3.forward);
	}

	private float LoopClamp(float value, float minValue, float maxValue)
	{
		while (value < minValue || value >= maxValue)
		{
			if (value < minValue)
			{
				value += maxValue - minValue;
			}
			else if (value >= maxValue)
			{
				value -= maxValue - minValue;
			}
		}

		return value;
	}
}
