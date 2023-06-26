using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveMenu : MonoBehaviour
{
    public Vector2Int pos;
    public string player;
    public MoveMenuButton buttonPrefab;
    [SerializeField] RectTransform rt;
    MoveMenuButton[] buttons;
    Chessman piece;
    bool active = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetCreator(Chessman i)
    {
        piece = i;
        active = true;
    }

    void Set() { piece.isMenuOption = true; }

    void Disable() { foreach (MoveMenuButton b in buttons) { b.button.interactable = false; } }

    public void SetButtons(int menuType)
    {
        buttons = GetComponentsInChildren<MoveMenuButton>();

        foreach(MoveMenuButton b in buttons)
        {
            b.button.onClick.RemoveAllListeners();

            if(menuType != 2) b.button.onClick.AddListener(Set);
            b.button.onClick.AddListener(Disable);
        }

        switch (menuType)
        {
            case (1):
                SetImposter();
                break;
            case (2):
                Debug.Log(piece.AdjacentPieces(piece.GetXBoard(), piece.GetYBoard()).Count);
                SetPirate(piece.AdjacentPieces(piece.GetXBoard(), piece.GetYBoard()));
                break;
        }
    }

    void SetImposter()
    {
        void Move() { piece.MovePlateSpawn(pos.x, pos.y, false, false); }
        void Spawn() { piece.SpawnPiecePlateSpawn(pos.x, pos.y, false, "imp_dummy"); }

        buttons[0].button.onClick.AddListener(Move);
        buttons[1].button.onClick.AddListener(Spawn);
    }

    void SetPirate(List<string> adjacentPieces)
    {
        int count = piece.capturedPieces.Count + adjacentPieces.Count;

        float gap = 100;
        float width = count * gap;

        for (int i = 0; i < count; i++)
        {
            if(i < piece.capturedPieces.Count)
            {
                MoveMenuButton button = Instantiate(buttonPrefab, transform);
                button.image.sprite = Chessman.NameToSprite(piece.capturedPieces[i], piece.player);
                string capturedPiece = piece.capturedPieces[i];

                void Move() { Debug.Log(capturedPiece); piece.MakeMovePlates(capturedPiece); }
                button.button.onClick.AddListener(Move);
            }
            else
            {
                MoveMenuButton button = Instantiate(buttonPrefab, transform);
                button.image.sprite = Chessman.NameToSprite(adjacentPieces[i-piece.capturedPieces.Count], piece.player);
                string capturedPiece = adjacentPieces[i-piece.capturedPieces.Count];

                void Move() { Debug.Log(capturedPiece); piece.MakeMovePlates(capturedPiece); }
                button.button.onClick.AddListener(Move);
            }

            Setup();
        }
    }

    void OnDisable()
    {
        foreach (MoveMenuButton b in buttons)
        {
            b.button.onClick.RemoveAllListeners();
        }
    }

    public void Setup()
    {
        MoveMenuButton[] allButtons = GetComponentsInChildren<MoveMenuButton>();
        int count = allButtons.Length;

        float gap = 100;
        float width = count * gap;

        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

        for(int i = 0; i < count; i++)
        {
            MoveMenuButton button = allButtons[i];
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((i * gap) - width/2, 0);
        }
    }
}