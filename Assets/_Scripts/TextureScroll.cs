using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureScroll : MonoBehaviour
{
    [SerializeField] float xPower;
    [SerializeField] float yPower;

    Image render;

    void Start()
    {
        render = GetComponent<Image>();
        render.material.mainTextureOffset = Vector2.zero;
    }

    void Update()
    {
        render.material.mainTextureOffset += new Vector2(xPower * Time.deltaTime / 10, yPower * Time.deltaTime / 10);
    }
}