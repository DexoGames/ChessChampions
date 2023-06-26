using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasGroup))]
public class StartUI : MonoBehaviour
{
    CanvasGroup image;
    bool fade;
    const float FADE_TIME = 0.18f;
    const float START_SIZE = 0.9f;

    void OnEnable()
    {
        image = GetComponent<CanvasGroup>();
        image.alpha = 0;
        transform.localScale = new Vector3(START_SIZE, START_SIZE, 1);
        fade = false;
        StartCoroutine(WaitAndFadeIn());
    }

    IEnumerator WaitAndFadeIn()
    {
        float timeWait = (-transform.position.y + Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y) / 18f;

        yield return new WaitForSeconds(timeWait);

        fade = true;
    }

    void Update()
    {
        if (fade)
        {
            Vector3 scale = transform.localScale;
            image.alpha += Time.deltaTime / FADE_TIME;
            transform.localScale = new Vector3(scale.x + Time.deltaTime / FADE_TIME / (1 / (1 - START_SIZE)), scale.y + Time.deltaTime / FADE_TIME / (1 / (1 - START_SIZE)), 1);
            if(image.alpha >= 1)
            {
                fade = false;
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}