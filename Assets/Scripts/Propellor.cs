using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propellor : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 320f, Space.World);
    }
}
