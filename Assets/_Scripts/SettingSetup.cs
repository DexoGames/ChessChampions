using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingSetup : MonoBehaviour
{
    void Start()
    {
        foreach(Setting setting in Resources.LoadAll<Setting>(""))
        {
           //Debug.Log(setting.name + ", " + PlayerPrefs.GetInt(setting.name));
            setting.currentOption = PlayerPrefs.GetInt(setting.name);
        }
    }
}