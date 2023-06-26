using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameInfoText : MonoBehaviour
{
    Text text;
    public Setting gameMode;
    public Setting variation;

    void Start()
    {
        text = GetComponent<Text>();

        if(gameMode.currentOption == 1)
        {
            text.text = "ONLINE";
        }
        else
        {
            text.text = variation.options[variation.currentOption].ToUpper();
        }
    }
}
