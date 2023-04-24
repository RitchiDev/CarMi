using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToTarget : MonoBehaviour
{
    [SerializeField] private Transform m_Target;

    [SerializeField] private bool m_LockOnRotation = false;

    private void LateUpdate()
    {
        transform.position = m_Target.position;

        if(m_LockOnRotation)
        {
            transform.rotation = m_Target.rotation;
        }
    }
}
