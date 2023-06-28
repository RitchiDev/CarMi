using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingObjects : MonoBehaviour
{
    [SerializeField] private float   movingSpeed;
    [SerializeField] private Vector3 Movedirection;

    public void Update() { transform.Translate(movingSpeed * Time.deltaTime * Movedirection); }
}