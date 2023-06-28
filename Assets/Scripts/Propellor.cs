using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Propellor : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(320f * Time.deltaTime * Vector3.up, Space.World);
    }
}
