using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLimit : MonoBehaviour
{
    [SerializeField] private float xRotationLimit = 20;
    [SerializeField] private float yRotationLimit = 20;
    [SerializeField] private float zRotationLimit = 20;

    private void Update()
    {
        float angle = transform.eulerAngles.z;
        transform.eulerAngles = new Vector3(0f, 0f, ClampAngle(angle, -45f, 45f));
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
