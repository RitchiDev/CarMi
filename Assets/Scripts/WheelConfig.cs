using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelConfig
{
	[SerializeField] private WheelCollider m_WheelCollider;
	[SerializeField] private Transform m_WheelTransform;
	[SerializeField] private float m_SlipAmountNeededParticles;
	[SerializeField] private Vector3 m_SlipTrailOffset;
	[SerializeField] private TrailRenderer m_SlipTrail;
    private WheelHit m_WheelHit;

    public float CurrentMaxSlip { get { return Mathf.Max(CurrentForwardSleep, CurrentSidewaysSleep); } }
	public float CurrentForwardSleep { get; private set; }
	public float CurrentSidewaysSleep { get; private set; }

	public void FixedUpdate()
    {
		if (m_WheelCollider.GetGroundHit(out m_WheelHit))
		{
			var prevForwar = CurrentForwardSleep;
			var prevSide = CurrentSidewaysSleep;

			CurrentForwardSleep = (prevForwar + Mathf.Abs(m_WheelHit.forwardSlip)) / 2;
			CurrentSidewaysSleep = (prevSide + Mathf.Abs(m_WheelHit.sidewaysSlip)) / 2;
		}
		else
		{
			CurrentForwardSleep = 0;
			CurrentSidewaysSleep = 0;
		}
	}

	public bool GetIfIsGrounded()
    {
		return m_WheelCollider.isGrounded;
	}

	public WheelCollider GetWheelCollider()
    {
		return m_WheelCollider;
    }

	/// <summary>
	/// Update visual logic (Transform, FX).
	/// </summary>
	public void UpdateVisual()
	{
		UpdateTransform();

		if (m_WheelCollider.isGrounded && CurrentMaxSlip > m_SlipAmountNeededParticles)
		{
			//Emit particle.
		}
	}

	private void UpdateTransform()
	{
		Vector3 position;
		Quaternion rotation;
		m_WheelCollider.GetWorldPose(out position, out rotation);
		m_WheelTransform.position = position;
		m_WheelTransform.rotation = rotation;
	}
}
