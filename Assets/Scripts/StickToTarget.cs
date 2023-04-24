using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToTarget : MonoBehaviour
{
    [SerializeField] private Transform m_Target;

    private void LateUpdate()
    {
        transform.position = m_Target.position;
    }
}
