using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    private float rotation;
    [SerializeField] float rotationSpeed;

    void Update()
    {
        rotation += Time.deltaTime * rotationSpeed;
        transform.rotation = Quaternion.Euler(rotation, rotation, rotation);
    }
}