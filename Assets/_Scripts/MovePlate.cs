using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    //Some functions will need reference to the controller
    public Game controller;

    //List of the different menus for the pieces
    [SerializeField] MoveMenu[] moveMenus;

    //The Chesspiece that was tapped to create this MovePlate
    Chessman reference = null;

    //Location on the board
    int matrixX;
    int matrixY;

    //false: movement, true: attacking
    public bool attack = false;
    public bool ranged = false;
    public bool swap = false;
    public int castle = 0;
    public int menuType = 0;
    public string spawnPiece = null;
    public bool dodge;
    public Vector2Int attackCoord;
    bool won;

    public void Start()
    {
        if (reference.isMenuOption)
        {
            reference.isMenuOption = false;
            OnMouseUp();
            controller.pieceInteractable = true;
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        controller.pieceInteractable = false;
        Chessman cp = controller.GetPosition(matrixX, matrixY);

        if (menuType > 0)
        {
            OnMoveMenu();
            return;
        }

        Chessman otherPiece = null;
        if (cp != null)
        {
            otherPiece = cp;
        }

        Chessman movingPiece = reference;

        won = false;

        if(spawnPiece != null)
        {
            Chessman spawned = controller.CreateWithPlayer(reference.player, spawnPiece, matrixX, matrixY, false);
            spawned.SetCoords();
            controller.SetPosition(spawned);

            AfterMove();
            return;
        }

        if (attack)
        {
            if (cp.name == "white_king" || cp.name == "white_hikaru") { controller.Winner("black"); won = true; }
            if (cp.name == "black_king" || cp.name == "black_hikaru") { controller.Winner("white"); won = true; }

            RemovePieceOrDodge(otherPiece);

            if(controller.GetVariation() == 1 && movingPiece.piece != "king")
            {
                SetPiece(movingPiece, movingPiece.player, otherPiece.piece);
            }
        }

        if (!swap)
        {
            controller.SetPositionEmpty(movingPiece.GetXBoard(), movingPiece.GetYBoard());
        }
        else if(!attack)
        {
            otherPiece.SetXBoard(movingPiece.GetXBoard());
            otherPiece.SetYBoard(movingPiece.GetYBoard());
            otherPiece.SetCoords();

            controller.SetPosition(cp);
        }

        if (attackCoord.x >= 0 && attackCoord.y >= 0)
        {
            RemovePieceOrDodge(controller.GetPosition(attackCoord.x, attackCoord.y));
        }

        if (castle > 0)
        {
            Debug.Log("CASTLE " + castle);

            if (castle == 1)
            {
                Chessman pieceObject = controller.GetPosition(matrixX + 1, matrixY);
                
                if(pieceObject != null)
                {
                    Chessman castlePiece = pieceObject;

                    castlePiece.SetXBoard(matrixX - 1);
                    castlePiece.SetYBoard(matrixY);
                    castlePiece.SetCoords();

                    controller.SetPosition(pieceObject);
                }
            }
            else
            {
                Chessman pieceObject = controller.GetPosition(matrixX - 2, matrixY);

                if (pieceObject != null)
                {
                    Chessman castlePiece = pieceObject;

                    castlePiece.SetXBoard(matrixX + 1);
                    castlePiece.SetYBoard(matrixY);
                    castlePiece.SetCoords();

                    controller.SetPosition(pieceObject);
                }
            }
        }

        //Move reference chess piece to this position
        if (!ranged)
        {
            movingPiece.SetXBoard(matrixX);
            movingPiece.SetYBoard(matrixY);
            movingPiece.SetCoords();
            movingPiece.moved = true;

            if(movingPiece.piece == "pawn" || movingPiece.piece == "minstrel")
            {
                EndPieceConversion(movingPiece, "queen");
            }
            else if(movingPiece.piece == "crusader")
            {
                EndPieceConversion(movingPiece, "christ");
            }
        }

        AfterMove();
    }

    void AfterMove()
    {
        controller.ClearThreatIndicators();

        controller.SetPosition(reference);

        if(reference.piece == "necromancer")
        {
            if(controller.deadPositions[matrixX, matrixY].piece > -1)
            {
                Vector2Int revivePos = DecideBestRevive(reference);

                Chessman revived = controller.CreateWithPlayer(Chess.IntToPlayer(controller.deadPositions[matrixX, matrixY].player), Chess.IntToPiece(controller.deadPositions[matrixX, matrixY].piece), revivePos.x, revivePos.y, true);
                controller.SetPosition(revived);

                controller.RemoveGrave(matrixX, matrixY);
            }
        }

        reference.CheckInvincible();

        //Switch Current Player
        if (!dodge)
        {
            if (!controller.testOnly)
            {
                controller.NextTurn(won);
            }
            else
            {
                controller.pieceInteractable = true;
            }
        }

        //Destroy the move plates including self
        Chessman.DestroyMovePlates();
    }

    void OnMoveMenu()
    {
        MoveMenu menu = Instantiate(moveMenus[menuType], GameObject.FindGameObjectWithTag("TopCanvas").transform);

        menu.transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
        menu.pos = new Vector2Int(matrixX, matrixY);
        menu.player = reference.player;
        menu.SetCreator(reference);
        menu.SetButtons(menuType);
    }

    void RemovePieceOrDodge(Chessman piece)
    {
        if (IsDodge(piece))
        {
            Debug.Log("DODGE");

            bool success = DodgePiece(piece);

            if (!success)
            {
                piece.DestoryPiece();
            }
        }
        else
        {
            reference.capturedPieces.Add(piece.piece);
            piece.DestoryPiece();
        }
    }

    bool IsDodge(Chessman piece)
    {
        if (Chess.IsDodging(piece.piece))
        {
            Debug.Log("Dodgable Piece");

            if(Random.Range(0, 100) < 80)
            {
                return true;
            }
        }
        return false;
    }

    bool DodgePiece(Chessman piece)
    {
        Vector2Int bestMove = DecideBestDodge(piece);
        if (bestMove.x < 0) return false;

        piece.MakeMovePlates(piece.piece);

        foreach (MovePlate mp in piece.movePlates)
        {
            if (mp.GetCoords() == bestMove)
            {
                mp.dodge = true;
                mp.OnMouseUp();
                return true;
            }
        }

        return false;
    }

    Vector2Int DecideBestDodge(Chessman piece)
    {
        bool[,] moves = piece.CalcMoves(controller);
        bool[,] threats = controller.CheckAllThreats(piece.player, controller.board.GetComponentsInChildren<Chessman>());
        Vector2Int bestMove = new Vector2Int(-1, -1);
        for(int x = 0; x < controller.positions.GetLength(0); x++)
        {
            for (int y = 0; y < controller.positions.GetLength(1); y++)
            {
                if(moves[x, y])
                {
                    if(!threats[x, y] || bestMove.x < 0)
                    {
                        bestMove = new Vector2Int(x, y);
                    }
                }
            }
        }

        return bestMove;
    }

    Vector2Int DecideBestRevive(Chessman piece)
    {
        bool[,] moves = piece.CalcRevive(controller);

        bool[,] threats = controller.CheckAllThreats(piece.player, controller.board.GetComponentsInChildren<Chessman>());
        Vector2Int bestMove = new Vector2Int(-1, -1);
        for (int x = 0; x < controller.positions.GetLength(0); x++)
        {
            for (int y = 0; y < controller.positions.GetLength(1); y++)
            {
                if (moves[x, y])
                {
                    if (!threats[x, y] || bestMove.x < 0)
                    {
                        bestMove = new Vector2Int(x, y);
                    }
                }
            }
        }

        return bestMove;
    }

    public Vector2Int GetCoords()
    {
        return new Vector2Int(matrixX, matrixY);
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(Chessman obj)
    {
        reference = obj;
    }

    public Chessman GetReference()
    {
        return reference;
    }

    public static void SetPiece(Chessman piece, string targetPlayer, string targetPiece)
    {
        if (targetPiece == "peasant")
        {
            piece.name = targetPiece;
        }
        else
        {
            piece.name = targetPlayer + "_" + targetPiece;
        }

        piece.Activate();
    }

    void EndPieceConversion(Chessman movingPiece, string piece)
    {
        if (movingPiece.player == "white")
        {
            if (movingPiece.GetYBoard() >= 8)
            {
                SetPiece(movingPiece, movingPiece.player, piece);
            }
        }
        else if (movingPiece.player == "black")
        {
            if (movingPiece.GetYBoard() <= 1)
            {
                SetPiece(movingPiece, movingPiece.player, piece);
            }
        }
    }
}
