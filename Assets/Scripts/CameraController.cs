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

    private void LateUpdate()
    {
        HandleMovement();
        HandleRotation();
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
}
