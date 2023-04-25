using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotInWebBuild : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Do the thing for web GL platform
            gameObject.SetActive(false);
        }
    }
}
