using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessBackgroundScript : MonoBehaviour
{
    float lerpTimer;
    Image background;
    Color oldColor;
    int index;
    [SerializeField] float transitionTime;
    [SerializeField] Color[] menuColors;

    void Awake()
    {
        background = GetComponent<Image>();
    }

    void Start()
    {
        oldColor = menuColors[0];
        background.color = oldColor;
    }

    void Update()
    {
        if (lerpTimer > 0)
        {
            background.color = Color.Lerp(oldColor, menuColors[index], lerpTimer);

            if (lerpTimer <= 1)
            {
                lerpTimer += Time.deltaTime / transitionTime;
            }
            else
            {
                lerpTimer = 0;
            }
        }
    }

    public void StartFade(int colourIndex)
    {
        index = colourIndex;
        oldColor = background.color;
        lerpTimer = 0.01f;
    }
}
