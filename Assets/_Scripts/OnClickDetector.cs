using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnClickDetector : MonoBehaviour, IPointerDownHandler
{
    public bool pressed;
    public bool autoRelease;


    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    void Update()
    {
        if (!Input.GetMouseButton(0) && autoRelease)
        {
            pressed = false;
        }
    }
}