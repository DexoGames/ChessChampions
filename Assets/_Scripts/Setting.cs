using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Setting", fileName = "New Setting")]
public class Setting : ScriptableObject
{
    public string[] options;
    public int currentOption;
}



public static class Settings
{
    public static Setting Load(string name)
    {
        return Resources.Load<Setting>(name);
    }
}