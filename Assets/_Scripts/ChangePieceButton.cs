using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePieceButton : MonoBehaviour
{
    Game game;
    [SerializeField] Text text;
    [SerializeField] Button deletePieceButton;
    GameObject pieceToDestroy;

    void Start()
    {

    }

    public void SetGame(Game obj)
    {
        game = obj;
        text.text = Chess.IntToPiece(game.currentTestPiece).ToUpper();
    }

    public void ChangePiece(int num)
    {
        if (game.currentTestPiece + num > Chess.GetNames().Length - 1)
        {
            game.currentTestPiece = 0;
        }
        else if (game.currentTestPiece + num < 0)
        {
            game.currentTestPiece = Chess.GetNames().Length - 1;
        }
        else
        {
            game.currentTestPiece += num;
        }

        text.text = Chess.IntToPiece(game.currentTestPiece).ToUpper();

        game.CreateTestPieces();
    }

    public void ChangePieceChoice(int num)
    {
        if (game.currentTestPiece + num > Chess.GetNames().Length - 1)
        {
            game.currentTestPiece = 0;
        }
        else if (game.currentTestPiece + num < 0)
        {
            game.currentTestPiece = Chess.GetNames().Length - 1;
        }
        else
        {
            game.currentTestPiece += num;
        }

        text.text = Chess.IntToPiece(game.currentTestPiece).ToUpper();
    }

    public void SpawnPlayer()
    {
        game.AddPiece(Chess.IntToPiece(game.currentTestPiece));
    }

    public void SetDestoryPiece(GameObject input)
    {
        pieceToDestroy = input;

        if(pieceToDestroy == null)
        {
            deletePieceButton.interactable = false;
        }
        else
        {
            deletePieceButton.interactable = true;
        }
    }

    public void DestroyPiece()
    {
        if(pieceToDestroy != null)
        {
            Chessman.DestroyMovePlates();
            Destroy(pieceToDestroy);
            SetDestoryPiece(null);
        }
    }
}
