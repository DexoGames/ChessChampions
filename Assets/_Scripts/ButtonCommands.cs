using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ButtonCommands
{
    public static void LocalMultiplayer()
    {
        Settings.Load("GameMode").currentOption = 0;
        SceneManager.LoadScene("Game");
    }
    public static void VsComputer()
    {
        Settings.Load("GameMode").currentOption = 2;
        SceneManager.LoadScene("Game");
    }
    public static IEnumerator OnlineMultiplayer()
    {
        Settings.Load("GameMode").currentOption = 1;
        yield return new WaitUntil(() => Settings.Load("GameMode").currentOption == 1);
        SceneManager.LoadScene("SelectChampions");
    }

    public static void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    public static void ChangeMenuType(MenuType target)
    {
        MenuManager menu = GameObject.FindObjectOfType<MenuManager>();
        menu.menuType = target;
        menu.SetMenu();
    }

    public static void ChangeSetting(Setting setting, int num, Text text)
    {
        if (setting.currentOption + num > setting.options.Length - 1)
        {
            setting.currentOption = 0;
        }
        else if (setting.currentOption + num < 0)
        {
            setting.currentOption = setting.options.Length - 1;
        }
        else
        {
            setting.currentOption += num;
        }

        if(text != null)
        {
            text.text = setting.options[setting.currentOption].ToUpper();
        }

        if(num != 0)
        {
            PlayerPrefs.SetInt(setting.name, setting.currentOption);
        }
    }
}