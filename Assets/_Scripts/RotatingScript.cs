using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingScript : MonoBehaviour
{

    void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z + Time.deltaTime * 360);
    }
}
