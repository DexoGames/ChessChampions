using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class Chessman : MonoBehaviour
{
    public bool moved;
    public string piece;
    public string player;
    public bool invincible;
    public List<string> capturedPieces = new List<string>();

    public GameObject board;
    public GameObject controller;
    Game controllerGame;
    public GameObject movePlate;
    public GameObject glow;
    public List<MovePlate> movePlates = new List<MovePlate>();

    int xBoard = -1;
    int yBoard = -1;

    int oldX;
    int oldY;


    bool[,] possibleMoves;

    const float TRAVEL_TIME = 0.25f;

    Setting local;
    Setting gameMode;

    public bool isMenuOption;

    void Update()
    {
        RotatePiece();
    }

    void RotatePiece()
    {
        if (local.currentOption == 0 || gameMode.currentOption == 1)
        {
            transform.localEulerAngles = new Vector3(0, 0, -board.transform.localEulerAngles.z);
        }
    }

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        controllerGame = controller.GetComponent<Game>();
        board = GameObject.FindGameObjectWithTag("Board");
        SpriteRenderer render = this.GetComponent<SpriteRenderer>();

        if (!controllerGame.testOnly)
        {
            local = controllerGame.localMode;
            gameMode = controllerGame.gameMode;
        }
        else
        {
            local = ScriptableObject.CreateInstance<Setting>();
            gameMode = ScriptableObject.CreateInstance<Setting>();
        }

        RotatePiece();

        SetCoords();

        piece = this.name.Substring(6);

        //Choose correct sprite based on piece's name

        if (name[0] == 'b')
        {
            player = "black";
        }
        else if (name[0] == 'w')
        {
            player = "white";
        }
        else
        {
            player = "neutral";
            piece = this.name;
        }

        render.sprite = FindPieceSprite();

        if (controllerGame.localMode.currentOption == 1 && player == "black" && !controllerGame.testOnly)
        {
            transform.localEulerAngles = new Vector3(0, 0, 180);
        }
    }

    public Sprite FindPieceSprite()
    {
        if (controllerGame.GetVariation() != 2 || piece == "peasant")
        {
            if (Resources.LoadAll<Sprite>("Art/Pieces/" + this.name).Length != 0)
            {
                return Resources.Load<Sprite>("Art/Pieces/" + this.name);
            }
            else
            {
                return Resources.Load<Sprite>("Art/Pieces/" + player + "_blank");
            }
        }
        else
        {
            return Resources.Load<Sprite>("Art/Pieces/" + player + "_blank");
        }
    }

    public static Sprite NameToSprite(string name, string player)
    {
        if (Resources.LoadAll<Sprite>("Art/Pieces/" + player + "_" + name).Length != 0)
        {
            return Resources.Load<Sprite>("Art/Pieces/" + player + "_" + name);
        }
        else
        {
            return Resources.Load<Sprite>("Art/Pieces/" + player + "_blank");
        }
    }

    public void SetCoords()
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        x = BoardToPos(x);
        y = BoardToPos(y);

        transform.localPosition = new Vector3(BoardToPos(oldX), BoardToPos(oldY), 1);

        if (transform != null)
        {
            transform.DOLocalMove(new Vector3(x, y, transform.localPosition.z), TRAVEL_TIME);
        }

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1);
    }

    public static float BoardToPos(float input)
    {
        input += -4.5f;

        input *= 2f / 3f;

        return input;
    }

    public void DestoryPiece()
    {
        if(piece == "pheonix" && SearchForPiece("phe_dummy", true))
        {
            PheonixRebirth();
        }
        else
        {
            if (!controllerGame.testOnly) controllerGame.AddGrave(Chess.PieceToInt(piece), Chess.PlayerToInt(player), xBoard, yBoard);
        }

        if (controllerGame.gameMode.currentOption == 1)
        {
            StartCoroutine(DestroyWithDelay());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void PheonixRebirth()
    {
        foreach (Chessman cm in controllerGame.positions)
        {
            if (cm == null) continue;
            if (cm.piece != "phe_dummy" || cm.player != player) continue;

            MovePlate.SetPiece(cm, player, "pheonix");

            break;
        }
    }

    IEnumerator DestroyWithDelay()
    {
        yield return new WaitForSeconds(TRAVEL_TIME * 0.95f);

        Destroy(gameObject);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        oldX = xBoard;
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        oldY = yBoard;
        yBoard = y;
    }

    public int GetOldX()
    {
        return oldX;
    }

    public int GetOldY()
    {
        return oldY;
    }

    public void SetOldX(int input)
    {
        oldX = input;
    }

    public void SetOldY(int input)
    {
        oldY = input;
    }


    public void SetMoved(bool input)
    {
        moved = input;
    }

    public void OnInvincible(bool i)
    {
        invincible = i;
        if (i)
        {
            this.GetComponent<SpriteRenderer>().color = Color.cyan;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void CheckInvincible()
    {
        if(piece == "hikaru")
        {
            bool onSpace = false;

            int x = GetXBoard();
            int y = GetYBoard();

            if (player == "white" && x == 5 && y == 2) onSpace = true;

            if (player == "black" && x == 5 && y == 7) onSpace = true;

            if (onSpace && AdjacentPieceCheck(x, y, null))
            {
                OnInvincible(true);
            }
        }
        if(piece == "jesus")
        {
            bool insideBoard = false;

            if (controllerGame.PositionOnBoard(xBoard + 1, yBoard + 1) && controllerGame.PositionOnBoard(xBoard - 1, yBoard - 1)) insideBoard = true;

            if (insideBoard && AdjacentJesusPieceCheck(xBoard, yBoard, null))
            {
                OnInvincible(true);

                Debug.Log("E");

                for(int x = -1; x <=1; x++) { for(int y = -1; y <= 1; y++)
                    {
                        if (Mathf.Abs(x) == Mathf.Abs(y)) continue;

                        Debug.Log(x + ", " + y);

                        Chessman cm = controllerGame.GetPosition(xBoard + x, yBoard + y);
                        cm.OnInvincible(true);
                    } }
            }
        }
    }

    private void OnMouseUp()
    {
        if (!controllerGame.pieceInteractable) return;
        if (gameMode.currentOption == 1)
        {
            GameNetworking network = FindObjectOfType<GameNetworking>();
            if ((network.player != player && player != "neutral") || controllerGame.GetCurrentPlayer() != network.player) return;
        }
        if (!controllerGame.IsGameOver() && (controllerGame.GetCurrentPlayer() == player || player == "neutral"))
        {
            MakeMovePlates(piece);
        }
    }

    public void MakeMovePlates(string wantedPiece)
    {
        DestroyMovePlates();
        InitiateMovePlates(false, wantedPiece);
    }

    public bool[,] CalcMoves(Game game)
    {
        controllerGame = game;
        possibleMoves = new bool[10, 10];
        InitiateMovePlates(true, piece);
        return possibleMoves;
    }

    public bool[,] CalcRevive(Game game)
    {
        controllerGame = game;
        possibleMoves = new bool[10, 10];
        InitiateRevivePlates(true);
        return possibleMoves;
    }

    public static void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }

        MoveMenu[] moveMenus = FindObjectsOfType<MoveMenu>();
        for (int i = 0; i < moveMenus.Length; i++)
        {
            Destroy(moveMenus[i].gameObject);
        }

    }

    int CheckForKings()
    {
        int count = 0;
        Chessman[] pieces = FindObjectsOfType<Chessman>();
        foreach(Chessman p in pieces)
        {
            if(p.piece == "king")
            {
                count++;
            }
        }
        return count;
    }

    void CustomBoardSetup()
    {
        for (int x = 0; x < controllerGame.positions.GetLength(0); x++)
        {
            for (int y = 0; y < controllerGame.positions.GetLength(1); y++)
            {
                if (controllerGame.positions[x, y] == null && y < Game.CUSTOM_BOARD_SPACE && y > 0 && x > 0 && x < 9)
                {
                    MovePlateSpawn(x, y, false, false);
                }
            }
        }

        if (piece != "king")
        {
            controllerGame.changePieceButton.SetDestoryPiece(this.gameObject);
        }
        else
        {
            if (CheckForKings() > 1)
            {
                controllerGame.changePieceButton.SetDestoryPiece(this.gameObject);
            }
        }
    }

    public void InitiateMovePlates(bool calcMoves, string wantedPiece)
    {
        movePlates.Clear();

        if (controllerGame.customBoard)
        {
            CustomBoardSetup();

            return;
        }

        int y = 1;
        if(player == "black")
        {
            y = -1;
        }

        //Debug.Log(Chess.PieceToInt(wantedPiece));
        Action<bool, int> ShowMoves = IntToMoves(Chess.PieceToInt(wantedPiece));
        ShowMoves(calcMoves, y);


        //controllerGame.OnMovePlatesFinished();
    }

    public void InitiateRevivePlates(bool calcMoves)
    {
        movePlates.Clear();

        if (controllerGame.customBoard)
        {
            CustomBoardSetup();

            return;
        }

        SurroundMovePlate(false, calcMoves);

        //controllerGame.OnMovePlatesFinished();
    }

    public Action<bool, int> IntToMoves(int i)
    {
        Action<bool, int>[] actions = { 
                                        PawnMovePlate, BishopMovePlate, KnightMovePlate, RookMovePlate, QueenMovePlate, KingMovePlate,
                                        JousterMovePlate, FoolMovePlate, NobleMovePlate, SquireMovePlate, StewardMovePlate, ArcherMovePlate,
                                        PeasantMovePlate, MinstrelMovePlate, PrinceMovePlate,
                                        ArchbishopMovePlate, ClericMovePlate, PrincessMovePlate, CrusaderMovePlate, ChristMovePlate,

                                        JesusMovePlate, SniperMovePlate, ImposterMovePlate, NoMoves, NinjaMovePlate, MimicMovePlate, KingMovePlate, WizardMovePlate, NecromancerMovePlate, PheonixMovePlate, NoMoves, PirateMovePlate
                                      };

        if(i < actions.Length)
        {
            return actions[i];
        }
        else
        {
            Debug.LogError("Index of move requested isn't in array");
            return null;
        }
    }


    public void LineMovePlate(int xIncrement, int yIncrement, bool calcMoves)
    {
        Game sc = controllerGame;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y, false, calcMoves);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).player != player)
        {
            MovePlateAttackSpawn(x, y, false, calcMoves);
        }
    }

    public void ExtraLineMovePlate(int xIncrement, int yIncrement, bool calcMoves)
    {
        Game sc = controllerGame;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnExtraBoard(x, y, true) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y, false, calcMoves);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnExtraBoard(x, y, true) && sc.GetPosition(x, y).player != player)
        {
            MovePlateAttackSpawn(x, y, false, calcMoves);
        }
    }

    public void LineNoAttackMovePlate(int xIncrement, int yIncrement, bool calcMoves)
    {
        Game sc = controllerGame;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y, false, calcMoves);
            x += xIncrement;
            y += yIncrement;
        }
    }


    public void LMovePlate(bool calcMoves)
    {
        PointMovePlate(1, 2, true, calcMoves);
        PointMovePlate(-1, 2, true, calcMoves);
        PointMovePlate(2, 1, true, calcMoves);
        PointMovePlate(2, -1, true, calcMoves);
        PointMovePlate(1, -2, true, calcMoves);
        PointMovePlate(-1, -2, true, calcMoves);
        PointMovePlate(-2, 1, true, calcMoves);
        PointMovePlate(-2, -1, true, calcMoves);
    }

    public void SurroundMovePlate(bool kills, bool calcMoves)
    {
        PointMovePlate(0, 1, kills, calcMoves);
        PointMovePlate(0, -1, kills, calcMoves);
        PointMovePlate(-1, 0, kills, calcMoves);
        PointMovePlate(-1, -1, kills, calcMoves);
        PointMovePlate(-1, 1, kills, calcMoves);
        PointMovePlate(1, 0, kills, calcMoves);
        PointMovePlate(1, -1, kills, calcMoves);
        PointMovePlate(1, 1, kills, calcMoves);
    }

    public void PointMovePlate(int rawX, int rawY, bool kills, bool calcMoves)
    {
        int x = xBoard + rawX;
        int y = yBoard + rawY;

        Game sc = controllerGame;
        if (sc.PositionOnBoard(x, y))
        {
            Chessman cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y, false, calcMoves);
            }
            else if (cp.player != player && kills)
            {
                MovePlateAttackSpawn(x, y, false, calcMoves);
            }
        }
    }

    public void NobleMovePoint(int rawX, int rawY, bool calcMoves)
    {
        int x = xBoard + rawX;
        int y = yBoard + rawY;

        Game sc = controllerGame;
        if (sc.PositionOnBoard(x, y))
        {
            Chessman cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y, false, calcMoves);
            }
            else
            {
                Chessman piece = cp;

                if (piece.player != player)
                {
                    MovePlateAttackSpawn(x, y, false, calcMoves);
                }
                else if (piece.player == player && IsOkayForNoble(piece))
                {
                    MovePlateSpawn(x, y, true, calcMoves);
                }
            }
        }
    }

    void NinjaMovePoint(int rawX, int rawY, int attackX, int attackY, bool calcMoves)
    {
        int x = xBoard + rawX;
        int y = yBoard + rawY;

        Vector2Int attack = new Vector2Int(xBoard + attackX, yBoard + attackY);

        if (!controllerGame.PositionOnBoard(x, y)) return;

        if (controllerGame.GetPosition(x, y) != null) return;

        if (controllerGame.GetPosition(attack.x, attack.y) != null)
        {
            if(controllerGame.GetPosition(attack.x, attack.y).player == player)
            {
                MovePlateSpawn(x, y, false, calcMoves);
            }
            else
            {
                Debug.Log("WILL ATTACK AT " + attack);
                MovePlateAttackSeperateSpawn(x, y, false, calcMoves, attack);
            }
        }
        else
        {
            MovePlateSpawn(x, y, false, calcMoves);
        }
    }

    bool IsOkayForNoble(Chessman piece)
    {
        Game sc = controllerGame;
        foreach(string name in sc.nobleSwapPieces)
        {
            if(name == piece.piece)
            {
                return true;
            }
        }
        return false;
    }

    bool SearchForPiece(string piece, bool onlyFriendly)
    {
        foreach(Chessman cm in controllerGame.positions)
        {
            if (cm == null) continue;

            if (cm.piece == piece)
            {
                if (onlyFriendly)
                {
                    if (cm.player == player) return true;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    void ArcherLine(int xIncrement, int yIncrement, Game sc, bool calcMoves, bool ranged)
    {
        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        int count = 1;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            x += xIncrement;
            y += yIncrement;
            count++;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).player != player && count % 2 == 0)
        {
            MovePlateAttackSpawn(x, y, ranged, calcMoves);
        }
    }

    void JousterLine(int xIncrement, int yIncrement, Game sc, bool calcMoves)
    {
        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        int count = 1;

        while (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                if(count % 2 == 0)
                {
                    MovePlateSpawn(x, y, false, calcMoves);
                }
            }
            else
            {
                if (sc.GetPosition(x, y).player != player && count % 2 == 0)
                {
                    MovePlateAttackSpawn(x, y, false, calcMoves);
                }
                break;
            }

            x += xIncrement;
            y += yIncrement;
            count++;
        }
    }

    void SniperLine(int xIncrement, int yIncrement, bool calcMoves)
    {
        Game sc = controllerGame;

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            x += xIncrement;
            y += yIncrement;
        }

        if (Mathf.Abs(x - xBoard) > 1 && Mathf.Abs(y - yBoard) > 1)
        {
            if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).player != player)
            {
                MovePlateAttackSpawn(x, y, true, calcMoves);
            }
        }
    }

    public bool AdjacentPieceCheck(int xPos, int yPos, string piece)
    {
        Game sc = controllerGame;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!(x == 0 && y == 0))
                {
                    if (sc.PositionOnBoard(xPos + x, yPos + y))
                    {
                        if (sc.GetPosition(xPos + x, yPos + y) != null)
                        {
                            if(piece != null)
                            {
                                if (sc.GetPosition(xPos + x, yPos + y).piece == piece) return true;
                            }
                            else
                            {
                                if (sc.GetPosition(xPos + x, yPos + y).player == player) return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public List<string> AdjacentPieces(int xPos, int yPos)
    {
        Game sc = controllerGame;

        List<string> pees = new List<string>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!(x == 0 && y == 0))
                {
                    if (sc.PositionOnBoard(xPos + x, yPos + y))
                    {
                        if (sc.GetPosition(xPos + x, yPos + y) != null)
                        {
                            pees.Add(sc.GetPosition(xPos + x, yPos + y).piece);
                        }
                    }
                }
            }
        }
        return pees;
    }

    public bool AdjacentJesusPieceCheck(int xPos, int yPos, string piece)
    {
        Game sc = controllerGame;

        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) != Mathf.Abs(y))
                {
                    if (sc.PositionOnBoard(xPos + x, yPos + y))
                    {
                        if (sc.GetPosition(xPos + x, yPos + y) != null)
                        {
                            if (piece != null)
                            {
                                if (sc.GetPosition(xPos + x, yPos + y).piece == piece) count++;
                            }
                            else
                            {
                                if (sc.GetPosition(xPos + x, yPos + y).player == player) count++;
                            }
                        }
                    }
                }
            }
        }
        if (count >= 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool[] CastleCheck()
    {
        bool[] castles = new bool[2];

        if(piece == "king" && !moved && xBoard == 4)
        {
            if (controllerGame.GetPosition(xBoard + 1, yBoard) == null && controllerGame.GetPosition(xBoard + 2, yBoard) == null)
            {
                Chessman pieceObject = controllerGame.GetPosition(xBoard + 3, yBoard);
                if(pieceObject != null)
                {
                    Chessman otherPiece = pieceObject;

                    if ((otherPiece.piece == "rook" || otherPiece.piece == "steward") && !otherPiece.moved)
                    {
                        castles[0] = true;
                    }
                }
            }

            if (controllerGame.GetPosition(xBoard - 1, yBoard) == null && controllerGame.GetPosition(xBoard - 2, yBoard) == null && controllerGame.GetPosition(xBoard - 3, yBoard) == null)
            {
                Chessman pieceObject = controllerGame.GetPosition(xBoard - 4, yBoard);
                if (pieceObject != null)
                {
                    Chessman otherPiece = pieceObject;

                    if ((otherPiece.piece == "rook" || otherPiece.piece == "steward") && !otherPiece.moved)
                    {
                        castles[1] = true;
                    }
                }
            }
        }

        return castles;
    }

    public void PawnMovePlate(bool calcMoves, int y)
    {
        PawnMove(0, y, calcMoves);
    }

    public void PawnMove(int rawX, int rawY, bool calcMoves)
    {
        Game sc = controllerGame;

        int x = xBoard + rawX;
        int y = yBoard + rawY;

        if (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                MovePlateSpawn(x, y, false, calcMoves);
            }

            if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null && sc.GetPosition(x + 1, y).player != player)
            {
                MovePlateAttackSpawn(x + 1, y, false, calcMoves);
            }

            if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null && sc.GetPosition(x - 1, y).player != player)
            {
                MovePlateAttackSpawn(x - 1, y, false, calcMoves);
            }

            if (!moved && sc.GetPosition(x, y) == null && sc.GetPosition(x, yBoard + (rawY * 2)) == null)
            {
                MovePlateSpawn(x, yBoard + (rawY * 2), false, calcMoves);
            }
        }
    }

    bool SquireCheck(int rawX, int rawY, Game sc)
    {
        int x = rawX + xBoard;
        int y = rawY + yBoard;

        if (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                return true;
            }
        }
        return false;
    }

    public void BishopMovePlate(bool calcMoves, int y)
    {
        LineMovePlate(1, 1, calcMoves);
        LineMovePlate(1, -1, calcMoves);
        LineMovePlate(-1, 1, calcMoves);
        LineMovePlate(-1, -1, calcMoves);
    }

    public void QueenMovePlate(bool calcMoves, int y)
    {
        LineMovePlate(1, 0, calcMoves);
        LineMovePlate(0, 1, calcMoves);
        LineMovePlate(1, 1, calcMoves);
        LineMovePlate(-1, 0, calcMoves);
        LineMovePlate(0, -1, calcMoves);
        LineMovePlate(-1, -1, calcMoves);
        LineMovePlate(-1, 1, calcMoves);
        LineMovePlate(1, -1, calcMoves);
    }

    public void RookMovePlate(bool calcMoves, int y)
    {
        LineMovePlate(1, 0, calcMoves);
        LineMovePlate(0, 1, calcMoves);
        LineMovePlate(-1, 0, calcMoves);
        LineMovePlate(0, -1, calcMoves);
    }

    public void KnightMovePlate(bool calcMoves, int y)
    {
        LMovePlate(calcMoves);
    }

    public void SquireMovePlate(bool calcMoves, int y)
    {
        SurroundMovePlate(true, calcMoves);

        PointMovePlate(0, 2, true, calcMoves);
        PointMovePlate(0, -2, true, calcMoves);
        PointMovePlate(2, 0, true, calcMoves);
        PointMovePlate(-2, 0, true, calcMoves);
    }

    public void StewardMovePlate(bool calcMoves, int y)
    {
        if (AdjacentPieceCheck(xBoard, yBoard, "king"))
        {
            LineMovePlate(1, 0, calcMoves);
            LineMovePlate(0, 1, calcMoves);
            LineMovePlate(1, 1, calcMoves);
            LineMovePlate(-1, 0, calcMoves);
            LineMovePlate(0, -1, calcMoves);
            LineMovePlate(-1, -1, calcMoves);
            LineMovePlate(-1, 1, calcMoves);
            LineMovePlate(1, -1, calcMoves);
        }
        else
        {
            PointMovePlate(0, 1, true, calcMoves);
            PointMovePlate(0, -1, true, calcMoves);
            PointMovePlate(1, 1, true, calcMoves);
            PointMovePlate(1, -1, true, calcMoves);
            PointMovePlate(-1, 1, true, calcMoves);
            PointMovePlate(-1, -1, true, calcMoves);

            LineMovePlate(1, 0, calcMoves);
            LineMovePlate(-1, 0, calcMoves);
        }
    }

    public void JousterMovePlate(bool calcMoves, int y)
    {
        Game sc = controllerGame;

        JousterLine(1, 0, sc, calcMoves);
        JousterLine(-1, 0, sc, calcMoves);
        JousterLine(0, 1, sc, calcMoves);
        JousterLine(0, -1, sc, calcMoves);

        PointMovePlate(1, 1, true, calcMoves);
        PointMovePlate(1, -1, true, calcMoves);
        PointMovePlate(-1, 1, true, calcMoves);
        PointMovePlate(-1, -1, true, calcMoves);
    }

    public void FoolMovePlate(bool calcMoves, int y)
    {
        PointMovePlate(2, 0, true, calcMoves);
        PointMovePlate(2, 2, true, calcMoves);
        PointMovePlate(2, -2, true, calcMoves);
        PointMovePlate(-2, 0, true, calcMoves);
        PointMovePlate(-2, 2, true, calcMoves);
        PointMovePlate(-2, -2, true, calcMoves);
        PointMovePlate(0, 2, true, calcMoves);
        PointMovePlate(0, -2, true, calcMoves);

        PointMovePlate(1, 1, true, calcMoves);
        PointMovePlate(1, -1, true, calcMoves);
        PointMovePlate(-1, 1, true, calcMoves);
        PointMovePlate(-1, -1, true, calcMoves);
    }

    public void ArcherMovePlate(bool calcMoves, int y)
    {
        Game sc = controllerGame;

        PointMovePlate(-1, -1, false, calcMoves);
        PointMovePlate(-1, 1, false, calcMoves);
        PointMovePlate(1, -1, false, calcMoves);
        PointMovePlate(1, 1, false, calcMoves);

        ArcherLine(1, 0, sc, calcMoves, true);
        ArcherLine(-1, 0, sc, calcMoves, true);
        ArcherLine(0, 1, sc, calcMoves, true);
        ArcherLine(0, -1, sc, calcMoves, true);
    }

    public void NobleMovePlate(bool calcMoves, int y)
    {
        NobleMovePoint(1, 0, calcMoves);
        NobleMovePoint(1, 1, calcMoves);
        NobleMovePoint(1, -1, calcMoves);
        NobleMovePoint(-1, 0, calcMoves);
        NobleMovePoint(-1, 1, calcMoves);
        NobleMovePoint(-1, -1, calcMoves);
        NobleMovePoint(0, 1, calcMoves);
        NobleMovePoint(0, -1, calcMoves);
    }

    public void PeasantMovePlate(bool calcMoves, int y)
    {
        SurroundMovePlate(false, calcMoves);
    }

    public void MinstrelMovePlate(bool calcMoves, int yMove)
    {
        PointMovePlate(1, yMove, true, calcMoves);
        PointMovePlate(-1, yMove, true, calcMoves);
        if(controllerGame.PositionOnBoard(xBoard, yBoard + yMove))
        {
            if(controllerGame.GetPosition(xBoard, yBoard + yMove) == null)
            {
                PointMovePlate(0, 2*yMove, false, calcMoves);
            }
        }
    }

    public void KingMovePlate(bool calcMoves, int y)
    {
        SurroundMovePlate(true, calcMoves);

        bool[] castles = CastleCheck();

        if (castles[0])
        {
            MovePlateCastleSpawn(2, 0, calcMoves, 1);
        }
        if (castles[1])
        {
            MovePlateCastleSpawn(-2, 0, calcMoves, 2);
        }
    }

    public void PrinceMovePlate(bool calcMoves, int y)
    {
        if (AdjacentPieceCheck(xBoard, yBoard, null))
        {
            LineMovePlate(1, 0, calcMoves);
            LineMovePlate(-1, 0, calcMoves);
            LineMovePlate(0, 1, calcMoves);
            LineMovePlate(0, -1, calcMoves);

            PointMovePlate(1, 1, true, calcMoves);
            PointMovePlate(1, -1, true, calcMoves);
            PointMovePlate(-1, 1, true, calcMoves);
            PointMovePlate(-1, -1, true, calcMoves);
        }
        else
        {
            SurroundMovePlate(true, calcMoves);
        }
    }

    public void ArchbishopMovePlate(bool calcMoves, int yMove)
    {
        LineMovePlate(1, 1, calcMoves);
        LineMovePlate(1, -1, calcMoves);
        LineMovePlate(-1, 1, calcMoves);
        LineMovePlate(-1, -1, calcMoves);
        PointMovePlate(0, yMove, false, calcMoves);
    }

    public void ClericMovePlate(bool calcMoves, int y)
    {
        PointMovePlate(1, 1*y, true, calcMoves);
        PointMovePlate(1, 2*y, true, calcMoves);

        PointMovePlate(-1, 1*y, true, calcMoves);
        PointMovePlate(-1, 2*y, true, calcMoves);

        PointMovePlate(1, -1*y, true, calcMoves);
        PointMovePlate(2, -1*y, true, calcMoves);

        PointMovePlate(-1, -1*y, true, calcMoves);
        PointMovePlate(-2, -1*y, true, calcMoves);
    }

    public void PrincessMovePlate(bool calcMoves, int y)
    {
        if (AdjacentPieceCheck(xBoard, yBoard, null))
        {
            LineMovePlate(1, 0, calcMoves);
            LineMovePlate(-1, 0, calcMoves);
            LineMovePlate(0, 1, calcMoves);
            LineMovePlate(0, -1, calcMoves);
        }
        else
        {
            LineMovePlate(1, 1, calcMoves);
            LineMovePlate(-1, 1, calcMoves);
            LineMovePlate(1, -1, calcMoves);
            LineMovePlate(-1, -1, calcMoves);
        }
    }

    public void CrusaderMovePlate(bool calcMoves, int yMove)
    {
        PointMovePlate(1, 0, false, calcMoves);
        PointMovePlate(-1, 0, false, calcMoves);
        PointMovePlate(1, yMove, false, calcMoves);
        PointMovePlate(0, yMove, true, calcMoves);
        PointMovePlate(-1, yMove, false, calcMoves);
        if (controllerGame.PositionOnBoard(xBoard, yBoard + yMove))
        {
            if (controllerGame.GetPosition(xBoard, yBoard + yMove) == null)
            {
                PointMovePlate(0, 2 * yMove, true, calcMoves);
            }
        }
    }

    public void ChristMovePlate(bool calcMoves, int yMove)
    {
        for(int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                if(!(x == 0 && y == 0))
                {
                    PointMovePlate(x, y, true, calcMoves);
                }
            }
        }
    }

    public void NoMoves(bool calcMoves, int yMove)
    {

    }


    //CHAMPION PIECES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


    public void JesusMovePlate(bool calcMoves, int y)
    {
        LMovePlate(calcMoves);
    }

    public void SniperMovePlate(bool calcMoves, int y)
    {
        SniperLine(1, 1, calcMoves);
        SniperLine(1, -1, calcMoves);
        SniperLine(-1, 1, calcMoves);
        SniperLine(-1, -1, calcMoves);

        SurroundMovePlate(false, calcMoves);
    }

    public void ImposterMovePlate(bool calcMoves, int yIncrement)
    {
        //if(!calcMoves) controllerGame.ScaleExtraBoard();

        bool dummyMade = SearchForPiece("imp_dummy", true);

        if (dummyMade)
        {
            ExtraLineMovePlate(1, 0, calcMoves);
            ExtraLineMovePlate(0, 1, calcMoves);
            ExtraLineMovePlate(1, 1, calcMoves);
            ExtraLineMovePlate(-1, 0, calcMoves);
            ExtraLineMovePlate(0, -1, calcMoves);
            ExtraLineMovePlate(-1, -1, calcMoves);
            ExtraLineMovePlate(-1, 1, calcMoves);
            ExtraLineMovePlate(1, -1, calcMoves);

            return;
        }

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (!(x == 0 && y == 0) && controllerGame.PositionOnExtraBoard(xBoard + x, yBoard + y, true))
                {
                    if(controllerGame.positions[xBoard + x, yBoard + y] == null)
                    {
                        MovePlateMenuSpawn(xBoard + x, yBoard + y, 1, calcMoves);
                    }
                    else if(controllerGame.positions[xBoard + x, yBoard + y].player != player)
                    {
                        MovePlateAttackSpawn(xBoard + x, yBoard + y, false, calcMoves);
                    }
                }
            }
        }
    }

    public void NinjaMovePlate(bool calcMoves, int yIncrement)
    {
        NinjaMovePoint(1, 2, 0, 2, calcMoves);
        NinjaMovePoint(-1, 2, 0, 2, calcMoves);
        NinjaMovePoint(2, 1, 2, 0, calcMoves);
        NinjaMovePoint(2, -1, 2, 0, calcMoves);
        NinjaMovePoint(1, -2, 0, -2, calcMoves);
        NinjaMovePoint(-1, -2, 0, -2, calcMoves);
        NinjaMovePoint(-2, 1, -2, 0, calcMoves);
        NinjaMovePoint(-2, -1, -2, 0, calcMoves);
    }

    public void MimicMovePlate(bool calcMoves, int yIncrement)
    {
        if(capturedPieces.Count > 0)
        {
            if (capturedPieces[capturedPieces.Count - 1] != "mimic")
            {
                Action<bool, int> ShowMoves = IntToMoves(Chess.PieceToInt(capturedPieces[capturedPieces.Count - 1]));
                ShowMoves(calcMoves, yIncrement);

                return;
            }
        }

        KnightMovePlate(calcMoves, yIncrement);
    }

    public void WizardMovePlate(bool calcMoves, int yIncrement)
    {
        for(int x = -2; x <= 2; x++)
        {
            for(int y = -2; y <= 2; y++)
            {
                if (x == 0 && y == 0) continue;

                NobleMovePoint(x, y, calcMoves);
            }
        }
    }

    public void NecromancerMovePlate(bool calcMoves, int yIncrement)
    {
        LineNoAttackMovePlate(1, 0, calcMoves);
        LineNoAttackMovePlate(-1, 0, calcMoves);
        LineNoAttackMovePlate(0, 1, calcMoves);
        LineNoAttackMovePlate(0, -1, calcMoves);

        LineNoAttackMovePlate(1, 1, calcMoves);
        LineNoAttackMovePlate(-1, 1, calcMoves);
        LineNoAttackMovePlate(1, -1, calcMoves);
        LineNoAttackMovePlate(-1, -1, calcMoves);
    }

    public void PheonixMovePlate(bool calcMoves, int yIncrement)
    {
        bool dummyMade = SearchForPiece("phe_dummy", true);

        KnightMovePlate(calcMoves, yIncrement);

        if (dummyMade) return;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!(x == 0 && y == 0) && controllerGame.PositionOnExtraBoard(xBoard + x, yBoard + y, true))
                {
                    if (controllerGame.positions[xBoard + x, yBoard + y] == null)
                    {
                        SpawnPiecePlateSpawn(xBoard + x, yBoard + y, calcMoves, "phe_dummy");
                    }
                }
            }
        }
    }

    public void PirateMovePlate(bool calcMoves, int yIncrement)
    {
        isMenuOption = true;
        MovePlateMenuSpawn(xBoard, yBoard, 2, calcMoves);
    }


    public void CheckAttackPlate(int rawX, int rawY, Game sc, bool ranged, bool calcMoves)
    {
        int x = xBoard + rawX;
        int y = yBoard + rawY;

        Chessman piece = sc.GetPosition(x, y);

        if (piece == null) return;

        Chessman cm = piece;

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) != null && cm.player != player && !cm.invincible)
        {
            MovePlateAttackSpawn(x, y, ranged, calcMoves);
        }
    }

    public MovePlate MovePlateSpawn(int matrixX, int matrixY, bool swap, bool calcMoves)
    {
        //Get the board value in order to convert to xy coords
        if (!calcMoves)
        {
            float x = matrixX;
            float y = matrixY;

            x = BoardToPos(x);
            y = BoardToPos(y);

            //Set actual unity values
            GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity, board.transform);
            mp.transform.localPosition = new Vector3(x, y, -3.0f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.swap = swap;
            mpScript.spawnPiece = null;
            mpScript.attackCoord = new Vector2Int(-1, -1);
            mpScript.dodge = false;
            mpScript.SetReference(this);
            mpScript.SetCoords(matrixX, matrixY);
            movePlates.Add(mpScript);

            //mpScript.MenuOption();

            return mpScript;
        }
        else
        {
            possibleMoves[matrixX, matrixY] = true;

            return null;
        }
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY, bool ranged, bool calcMoves)
    {
        if (controllerGame.GetPosition(matrixX, matrixY).invincible) return;

        if (!calcMoves)
        {
            //Get the board value in order to convert to xy coords
            float x = matrixX;
            float y = matrixY;

            x = BoardToPos(x);
            y = BoardToPos(y);

            //Set actual unity values
            GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity, board.transform);
            mp.transform.localPosition = new Vector3(x, y, -3.0f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.attack = true;
            mpScript.ranged = ranged;
            mpScript.attackCoord = new Vector2Int(-1, -1);
            mpScript.spawnPiece = null;
            mpScript.dodge = false;
            mpScript.SetReference(this);
            mpScript.SetCoords(matrixX, matrixY);
            movePlates.Add(mpScript);

            //mpScript.MenuOption();
        }
        else
        {
            possibleMoves[matrixX, matrixY] = true;
        }
    }

    public void MovePlateCastleSpawn(int rawX, int rawY, bool calcMoves, int castleType)
    {
        int x = rawX + xBoard;
        int y = rawY + yBoard;

        if (!calcMoves)
        {
            float worldX = BoardToPos(x);
            float worldY = BoardToPos(y);

            //Set actual unity values
            GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity, board.transform);
            mp.transform.localPosition = new Vector3(worldX, worldY, -3.0f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.castle = castleType;
            mpScript.spawnPiece = null;
            mpScript.SetReference(this);
            Debug.Log("Castle at " + x + ", " + y);
            mpScript.SetCoords(x, y);
            movePlates.Add(mpScript);

            //mpScript.MenuOption();
        }
        else
        {
            possibleMoves[x, y] = true;
        }
    }

    public void MovePlateMenuSpawn(int matrixX, int matrixY, int menu, bool calcMoves)
    {
        //Get the board value in order to convert to xy coords
        if (!calcMoves)
        {
            float x = matrixX;
            float y = matrixY;

            x = BoardToPos(x);
            y = BoardToPos(y);

            //Set actual unity values
            GameObject mp = Instantiate(movePlate, Vector3.zero, Quaternion.identity, board.transform);
            mp.transform.localPosition = new Vector3(x, y, -3.0f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.menuType = menu;
            mpScript.spawnPiece = null;
            mpScript.attackCoord = new Vector2Int(-1, -1);
            mpScript.dodge = false;
            mpScript.SetReference(this);
            mpScript.SetCoords(matrixX, matrixY);
            movePlates.Add(mpScript);

            //mpScript.MenuOption();
        }
        else
        {
            possibleMoves[matrixX, matrixY] = true;
        }
    }

    public void SpawnPiecePlateSpawn(int rawX, int rawY, bool calcMoves, string piece)
    {
        MovePlate plate = MovePlateSpawn(rawX, rawY, false, calcMoves);

        if (!calcMoves)
        {
            plate.spawnPiece = piece;
        }
    }

    public void MovePlateAttackSeperateSpawn(int rawX, int rawY, bool ranged, bool calcMoves, Vector2Int pos)
    {
        MovePlate plate = MovePlateSpawn(rawX, rawY, ranged, calcMoves);

        if (!calcMoves)
        {
            plate.attackCoord = pos;
        }
    }
}

public static class Chess
{
    public static string[] GetNames()
    {
        string[] names = { "pawn", "bishop", "knight", "rook", "queen", "king", "jouster", "fool", "noble", "squire", "steward", "archer", "peasant", "minstrel", "prince", "archbishop", "cleric", "princess", "crusader", "christ",
            
                            "jesus", "sniper", "imposter", "imp_dummy", "ninja", "mimic", "hikaru", "wizard", "necromancer", "pheonix", "phe_dummy", "pirate" };
        return names;
    }
    public static int[] GetPoints()
    {
        int[] names = { 1, 3, 3, 5, 9, 260, 4, 3, 2, 3, 4, 5, 1, 1, 5, 4, 3, 4, 1, 7, 9, 4, 9, 1, 4, 5, 260, 5, 6, 8, 1, 9 };
        return names;
    }

    public static bool IsExtraBoard(string name)
    {
        List<string> extraBoardPieces = new List<string>();
        extraBoardPieces.Add("imposter");

        if (extraBoardPieces.Contains(name))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool IsDodging(string name)
    {
        List<string> extraBoardPieces = new List<string>();
        extraBoardPieces.Add("ninja");

        if (extraBoardPieces.Contains(name))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string[] GetPlayers()
    {
        string[] names = { "white", "black", "neutral" };
        return names;
    }

    public static string IntToPiece(int i)
    {
        string[] names = GetNames();
        if (i < names.Length)
        {
            return names[i];
        }
        else
        {
            Debug.LogError("Outside of piece range (" + i + ")");
            return names[0];
        }
    }
    public static int PieceToInt(string piece)
    {
        string[] names = GetNames();

        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == piece)
            {
                return i;
            }
        }
        Debug.LogError("No Piece in the list with that name (" + piece + ")");
        return 0;
    }

    public static string IntToPlayer(int i)
    {
        string[] names = GetPlayers();
        if (i < names.Length)
        {
            return names[i];
        }
        else
        {
            Debug.LogError("Outside of player range (" + i + ")");
            return names[0];
        }
    }

    public static int PlayerToInt(string player)
    {
        string[] names = GetPlayers();

        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == player)
            {
                return i;
            }
        }
        Debug.LogError("No Player in the list with that name (" + player + ")");
        return 0;
    }


    public struct Piece
    {
        public bool onSquare;
        public int x;
        public int y;
        public string player;
        public string piece;
        public bool moved;
    }

    public static Piece[,] ChessmanToPieces(Chessman[,] board)
    {
        Piece[,] newBoard = new Piece[board.GetLength(0), board.GetLength(1)];

        for(int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if(board[x, y] != null)
                {
                    Chessman cm = board[x, y];
                    Piece piece = new Piece();
                    piece.onSquare = true;
                    piece.x = x;
                    piece.y = y;
                    piece.player = cm.player;
                    piece.piece = cm.piece;
                    piece.moved = cm.moved;

                    newBoard[x, y] = piece;
                }
            }
        }

        return newBoard;
    }
    public static GameObject[,] PiecesToGameObjects(Piece[,] board)
    {
        GameObject[,] newBoard = new GameObject[board.GetLength(0), board.GetLength(1)];

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y].onSquare)
                {
                    Piece piece = board[x, y];
                    GameObject obj = new GameObject();
                    Chessman cm = obj.AddComponent<Chessman>();
                    cm.SetXBoard(x);
                    cm.SetYBoard(y);
                    cm.player = piece.player;
                    cm.piece = piece.piece;
                    cm.moved = piece.moved;

                    newBoard[x, y] = obj;
                }
            }
        }

        return newBoard;
    }

    public static List<string> UnpackCapturedPieces(int input)
    {
        List<string> output = new List<string>();

        string strinput = input.ToString();

        for (int i = 0; i < strinput.Length; i++)
        {
            if(i % 2 == 1)
            {
                string piece = strinput[i].ToString() + strinput[i + 1].ToString();

                output.Add(Chess.IntToPiece(int.Parse(piece)));
            }
        }
        return output;
    }

    public static int PackCapturedPieces(List<string> input)
    {
        string output = "9";

        foreach (string piece in input)
        {
            string newP = Chess.PieceToInt(piece).ToString();

            if(newP.Length == 1)
            {
                newP = "0" + newP;
            }

            output += newP;
        }

        return int.Parse(output);
    }
}