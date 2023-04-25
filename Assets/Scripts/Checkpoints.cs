using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Checkpoints : MonoBehaviour
{
    [SerializeField] Areas currentState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<ICheckpointAble>() != null)
        {
            AreaChanger(currentState);
        }
    }

    private void AreaChanger(Areas AreaValue)
    {
        currentState = AreaValue;

        CheckpointHandler.instance.ChangeAreaName(AreaValue);

        Debug.Log(currentState.ToString());
    }

}