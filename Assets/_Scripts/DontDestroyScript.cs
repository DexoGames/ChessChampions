using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyScript : MonoBehaviour
{
    void Start()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag(gameObject.tag);
        if (list.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}