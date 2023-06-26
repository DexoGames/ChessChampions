using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionImage : MonoBehaviour
{
    public Image image;
    float originalScale;
    public bool usable;

    void Start()
    {
        originalScale = transform.localScale.x;
    }

    void Update()
    {
        float posX = transform.position.x;

        float newSize = -0.08f * posX * posX + 1;
        transform.localScale = new Vector2(originalScale * newSize, originalScale * newSize);

        Color col = image.color;

        if (usable)
        {
            image.color = new Color(col.r, col.g, col.b, Mathf.Abs(1.2f - (Mathf.Abs(posX) / 5)));
        }
        else
        {
            image.color = new Color(col.r, col.g, col.b, Mathf.Abs(0.5f - (Mathf.Abs(posX) / 15)));
        }
    }
}
