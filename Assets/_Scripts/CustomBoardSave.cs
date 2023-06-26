using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoardSave : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public static void SaveBoard()
    {
        Chessman[] pieces = FindObjectsOfType<Chessman>();
        Piece[] pieceArray = new Piece[pieces.Length];
        for(int i = 0; i < pieces.Length; i++)
        {
            pieceArray[i] = new Piece { piece = Chess.PieceToInt(pieces[i].piece) + 1, x = pieces[i].GetXBoard(), y = pieces[i].GetYBoard() };
        }

        for(int i = 0; i < 64; i++)
        {
            if(i >= pieceArray.Length)
            {
                PlayerPrefs.SetInt("cb_" + i + "_p", 0);
                PlayerPrefs.SetInt("cb_" + i + "_x", 0);
                PlayerPrefs.SetInt("cb_" + i + "_y", 0);
            }
            else
            {
                PlayerPrefs.SetInt("cb_" + i + "_p", pieceArray[i].piece);
                PlayerPrefs.SetInt("cb_" + i + "_x", pieceArray[i].x);
                PlayerPrefs.SetInt("cb_" + i + "_y", pieceArray[i].y);
            }
        }
    }

    struct Piece
    {
        public int piece;
        public int x;
        public int y;
    }
}
