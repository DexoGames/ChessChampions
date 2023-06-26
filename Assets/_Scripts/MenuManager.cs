using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MenuType { Main, Settings, Online, Info, Board };

public class MenuManager : MonoBehaviour
{
    public MenuType menuType;
    public GameObject[] menus;
    ChessBackgroundScript background;

    void Start()
    {
        background = GameObject.FindGameObjectWithTag("Background").GetComponent<ChessBackgroundScript>();
        menuType = MenuType.Main;
        SetMenu();
    }

    public void SetMenu()
    {
        CloseAllMenus();

        menus[(int)menuType].SetActive(true);

        background.StartFade((int)menuType);
    }

    void CloseAllMenus()
    {
        foreach(GameObject obj in menus)
        {
            obj.SetActive(false);
        }
    }

    public void ChangeMenuType(int target)
    {
        ButtonCommands.ChangeMenuType((MenuType)target);
    }

    public void LocalMultiplayer()
    {
        ButtonCommands.LocalMultiplayer();
    }
    public void OnlineMultiplayer()
    {
        ButtonCommands.OnlineMultiplayer();
    }
    public void VsComputer()
    {
        ButtonCommands.VsComputer();
    }


    public void SaveAndExitBoard()
    {
        CustomBoardSave.SaveBoard();
    }
}
