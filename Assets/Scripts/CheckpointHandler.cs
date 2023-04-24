using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Areas
{
    Ireland,
    Netherlands,
    Denmark,
}

public class CheckpointHandler : MonoBehaviour
{
    public static CheckpointHandler instance;

    private string AreaName;

    private Areas currentArea;
    [SerializeField] private Areas onStart;
    [SerializeField] private TextMeshProUGUI currentAreaText;

    private void Awake()
    {
        instance = this; 
        ChangeAreaName(onStart);
    }

    public void ChangeAreaName(Areas currentAreaName)
    {
        currentArea = currentAreaName;
        AreaName = currentArea.ToString();
        currentAreaText.text = "" + AreaName;
    }

    public Areas CurrentArea()
    {
        return currentArea;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, new Vector3(5, 5, 15));
    }
}
/// tf ben jij aan het doen???????????????????????????????????????????????????????
/// 