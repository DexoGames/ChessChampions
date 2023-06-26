using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSettingButton : MonoBehaviour
{
    public Setting setting;
    public Text text;

    void Start()
    {
        ButtonCommands.ChangeSetting(setting, 0, text);
    }

    public void Change(int numChange)
    {
        ButtonCommands.ChangeSetting(setting, numChange, text);
    }
}
