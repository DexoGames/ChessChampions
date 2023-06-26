using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOption
{
    public string action;
    public Vector2Int pos;
    public string player;
    public Sprite sprite;

    public void Setup(string newAction, Vector2Int newPos, string newPlayer, Sprite newSprite)
    {
        action = newAction;
        pos = newPos;
        player = newPlayer;
        sprite = newSprite;
    }
}