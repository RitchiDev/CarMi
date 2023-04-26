using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLimit : MonoBehaviour
{
    [SerializeField] private float zRotationLimit = 45f;

    private void Update()
    {
        Vector3 originalRotation = transform.eulerAngles;
        float angle = originalRotation.z;

        transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y, ClampAngle(angle, -zRotationLimit, zRotationLimit));
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90f || angle > 270f)
        {
            if (angle > 180)
            {
                angle -= 360f;
            }
            if (max > 180)
            {
                max -= 360f;
            }
            if (min > 180)
            {
                min -= 360f;
            }
        }

        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0)
        {
            angle += 360f;
        }

        return angle;
    }
}
