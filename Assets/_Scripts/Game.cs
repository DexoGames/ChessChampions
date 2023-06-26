using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    //Reference from Unity IDE
    public GameObject board;
    public Chessman chesspiece;
    public Grave grave;

    public string[] pawnOps;
    public string[] bishopOps;
    public string[] knightOps;
    public string[] rookOps;

    public string[] nobleSwapPieces;

    //Matrices needed, positions of each of the GameObjects
    //Also separate arrays for the players in order to easily keep track of them all
    //Keep in mind that the same objects are going to be in "positions" and "playerBlack"/"playerWhite"
    public Chessman[,] positions = new Chessman[10, 10];
    public GraveStat[,] deadPositions = new GraveStat[10, 10];
    private Chessman[] playerBlack = new Chessman[16];
    private Chessman[] playerWhite = new Chessman[16];
    private Chessman[] playerNeutral;
    private Grave[,] graves = new Grave[10, 10];
    //current turn
    private string currentPlayer = "white";
    public string isDodge { get; set; }

    //Game Ending
    private bool gameOver = false;

    const float ROTATE_GAP = 0.5f;
    float rotateAngle;
    [SerializeField] Text winnerText;
    [SerializeField] Text restartText;
    Sprite normalBoard;
    [SerializeField] Sprite extraBoard;

    [Header("Settings")]
    public Setting gameMode;
    public Setting variation;
    public Setting localMode;
    public Setting neutralPieces;
    public Setting chessPreset;

    [HideInInspector] public bool pieceInteractable;
    [HideInInspector] public bool testOnly;
    [HideInInspector] public bool customBoard;
    [HideInInspector] public int currentTestPiece;
    public const int CUSTOM_BOARD_SPACE = 4;
    public const int NEGATIVE_INFINITY = -999999;
    public const int POSITIVE_INFINITY = 999999;
    public ChangePieceButton changePieceButton;

    GameNetworking network;

    Chessman pieceToMove;
    bool aiMoved;

    public struct GraveStat
    {
        public int piece;
        public int player;
    }

    public void Start()
    {
        for(int x = 0; x < deadPositions.GetLength(0); x++)
        {
            for (int y = 0; y < deadPositions.GetLength(1); y++)
            {
                deadPositions[x, y].piece = -1;
            }
        }

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (FindObjectOfType<MenuManager>().menuType == MenuType.Info)
            {
                CreateTestPieces();
            }
            else
            {
                CreateCustomBoardPieces();
            }
            return;
        }

        ScaleBoard();

        if (gameMode.currentOption == 1)
        {
            network = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();

            network.SetBoardRotation();

            if (network.NetworkManager.IsHost)
            {
                SetupBoard();
            }
        }
        else
        {
            SetupBoard();
            ShowAllThreats(currentPlayer);
        }

        SetupText();

        if (gameMode.currentOption == 1)
        {
            if (network.NetworkManager.IsHost)
            {
                Chessman[] chessmen = board.GetComponentsInChildren<Chessman>();
                GameNetworking.NetPiece[] pieceArray = new GameNetworking.NetPiece[chessmen.Length];
                for (int i = 0; i < chessmen.Length; i++)
                {
                    pieceArray[i].piece = Chess.PieceToInt(chessmen[i].piece);
                    pieceArray[i].player = Chess.PlayerToInt(chessmen[i].player);
                    pieceArray[i].x = chessmen[i].GetXBoard();
                    pieceArray[i].y = chessmen[i].GetYBoard();
                    pieceArray[i].moved = chessmen[i].moved;
                    pieceArray[i].capturedPieces = Chess.PackCapturedPieces(chessmen[i].capturedPieces);
                }
                Debug.Log("SAVEBOARD START " + network.NetworkManager.ConnectedClients.Count);
                Debug.Log("SETTING TING 2");
                network.SaveBoardServerRpc(pieceArray);
            }
        }

        pieceInteractable = true;
    }

    public Chessman OldCreate(string name, int x, int y, bool moved)
    {
        x++;
        y++;
        Chessman cm = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity, board.transform);
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.SetMoved(moved);
        cm.SetOldX(x);
        cm.SetOldY(y);
        cm.transform.localPosition = new Vector3(Chessman.BoardToPos(x), Chessman.BoardToPos(y), -1);
        cm.capturedPieces = new List<string>();
        cm.Activate();

        if (Chess.IsExtraBoard(cm.piece))
        {
            ScaleExtraBoard();
        }

        return cm;
    }

    public Chessman OldCreateWithPlayer(string player, string piece, int x, int y, bool moved)
    {
        return OldCreate(player + "_" + piece, x, y, moved);
    }

    public Chessman CreateWithPlayer(string player, string piece, int x, int y, bool moved)
    {
        return OldCreate(player + "_" + piece, x-1, y-1, moved);
    }

    public void SetPosition(Chessman obj)
    {
        //Overwrites either empty space or whatever was there
        positions[obj.GetXBoard(), obj.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public Chessman GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 1 || y < 1 || x >= positions.GetLength(0)-1 || y >= positions.GetLength(1)-1)
        {
            return false;
        }
        return true;
    }

    public bool PositionOnExtraBoard(int x, int y, bool extra)
    {
        if (extra)
        {
            if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1))
            {
                return false;
            }
            return true;
        }
        else
        {
            return PositionOnBoard(x, y);
        }
    }

    public void AddGrave(int piece, int player, int x, int y)
    {
        deadPositions[x, y].piece = piece;
        deadPositions[x, y].player = player;
        Grave newGrave = SpawnGrave(piece, player, x, y);
    }
    public void RemoveGrave(int x, int y)
    {
        deadPositions[x, y].piece = -1;
        Destroy(graves[x, y].gameObject);
    }

    public Grave SpawnGrave(int piece, int player, int x, int y)
    {
        if (SearchForPieces("necromancer").Count == 0) return null;

        Grave g = Instantiate(grave, board.transform);
        g.transform.localPosition = new Vector2(Chessman.BoardToPos(x), Chessman.BoardToPos(y));
        g.piece = piece;
        g.player = player;
        graves[x, y] = g;

        return g;
    }

    public List<Chessman> SearchForPieces(string piece)
    {
        List<Chessman> pieceList = new List<Chessman>();

        foreach (Chessman cm in positions)
        {
            if (cm == null) continue;

            if (cm.piece == piece)
            {
                pieceList.Add(cm);
            }
        }
        return pieceList;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void SetCurrentPlayer(string input)
    {
        currentPlayer = input;
        SetupText();
    }

    public int GetVariation()
    {
        int setting;

        if (gameMode.currentOption == 1 || testOnly)
        {
            setting = 0;
        }
        else
        {
            setting = variation.currentOption;
        }
        return setting;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn(bool won)
    {
        StartCoroutine(RotateAndEnd(won));
    }

    IEnumerator RotateAndEnd(bool won)
    {
        if (currentPlayer == "white")
        {
            currentPlayer = "black";
        }
        else
        {
            currentPlayer = "white";
        }

        if (gameMode.currentOption == 1)
        {
            network.SetCurrentPlayerRequestServerRpc(Chess.PlayerToInt(currentPlayer));
        }

        if (gameMode.currentOption == 0 && localMode.currentOption == 0 && !won)
        {
            rotateAngle = 180;
        }
        else
        {
            rotateAngle = 0;
        }

        Transform boardTrans = board.transform;
        float targetAngle = boardTrans.eulerAngles.z + rotateAngle;

        if (boardTrans != null)
        {
            boardTrans.DORotate(new Vector3(0, 0, targetAngle), ROTATE_GAP).SetEase(Ease.InOutSine);
        }

        yield return new WaitForSeconds(ROTATE_GAP);

        if (rotateAngle == 0)
        {
            EndTurnCode(gameMode.currentOption);
            if (!won)
            {
                winnerText.text = currentPlayer.ToUpper() + "'S TURN";
                if (gameMode.currentOption == 0)
                {
                    restartText.text = currentPlayer.ToUpper() + "'S TURN";
                }
            }
        }
        else
        {
            EndTurnCode(gameMode.currentOption);
            if (!won)
            {
                winnerText.text = currentPlayer.ToUpper() + "'S TURN";
            }
        }

        if(gameMode.currentOption == 2 && !aiMoved && !gameOver)
        {
            StartCoroutine(PlayComputerMove());
        }
        else if (aiMoved)
        {
            aiMoved = false;
        }
    }

    public void ShowAllThreats(string player)
    {
        Chessman[] myPieces = GetMyPieces(currentPlayer);
        bool[,] attacks = CheckAllThreats(player, board.GetComponentsInChildren<Chessman>());

        foreach (Chessman p in myPieces)
        {
            if (p != null)
            {
                if (p.GetXBoard() >= 0 && p.GetXBoard() < attacks.GetLength(0) && p.GetYBoard() >= 0 && p.GetYBoard() < attacks.GetLength(1))
                {
                    if (attacks[p.GetXBoard(), p.GetYBoard()])
                    {
                        p.glow.SetActive(true);
                    }
                }
            }
        }
    }

    public bool[,] CheckAllThreats(string player, Chessman[] pieces)
    {
        if (gameMode.currentOption == 1)
        {
            player = network.player;
        }

        Chessman[] enemyPieces = new Chessman[pieces.Length];
        bool[,] attacks = new bool[10, 10];

        int count = 0;
        foreach (Chessman p in pieces)
        {
            if (p.player != player)
            {
                enemyPieces[count] = p;
                count++;
            }
        }
        count = 0;

        foreach (Chessman p in enemyPieces)
        {
            if (p != null)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        if (p.CalcMoves(this)[x, y])
                        {
                            attacks[x, y] = true;
                        }
                    }
                }
            }
        }

        return attacks;
    }


    public Chessman[] GetMyPieces(string player)
    {
        Chessman[] pieces = board.GetComponentsInChildren<Chessman>();

        int count = 0;
        foreach (Chessman p in pieces)
        {
            if (p.player == player)
            {
                count++;
            }
        }

        Chessman[] myPieces = new Chessman[count];
        count = 0;
        foreach (Chessman p in pieces)
        {
            if (p.player == player)
            {
                myPieces[count] = p;
                count++;
            }
        }

        return myPieces;
    }

    public void ClearThreatIndicators()
    {
        Chessman[] pieces = FindObjectsOfType<Chessman>();
        foreach(Chessman p in pieces)
        {
            p.glow.SetActive(false);
        }
    }

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            if (gameMode.currentOption != 1)
            {
                gameOver = false;

                SceneManager.LoadScene("Game");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!gameOver && gameMode.currentOption == 2)
            {
                MakeBestMove();
            }
        }
    }
    
    public void Winner(string playerWinner)
    {
        gameOver = true;

        winnerText.enabled = true;
        winnerText.text = playerWinner.ToUpper() + " IS THE WINNER";
        if (restartText.enabled)
        {
            restartText.text = playerWinner.ToUpper() + " IS THE WINNER";
        }
    }

    public void SetupText()
    {
        winnerText.text = currentPlayer.ToUpper() + "'S TURN";

        if (localMode.currentOption == 1 && gameMode.currentOption == 0) // If mirrored and local multiplayer
        {
            winnerText.transform.localScale = new Vector2(-winnerText.transform.localScale.x, -winnerText.transform.localScale.y);
            restartText.text = currentPlayer.ToUpper() + "'S TURN";
        }
        else if(gameMode.currentOption == 1) // If online
        {
            StartCoroutine(SetYouAreText());
        }
    }

    IEnumerator SetYouAreText()
    {
        yield return new WaitUntil(() => network.player != null);
        restartText.text = "YOU ARE " + network.player.ToUpper();
    }

    public void Menu()
    {
        if(gameMode.currentOption == 1)
        {
            network.Disconnect();
        }

        ButtonCommands.Menu();
    }

    void EndTurnCode(int mode)
    {
        Chessman[] chessmen = board.GetComponentsInChildren<Chessman>();

        foreach (Chessman cm in chessmen)
        {
            cm.OnInvincible(false);
        }

        foreach (Chessman cm in chessmen)
        {
            cm.CheckInvincible();
        }

        if(mode == 1)
        {
            GameNetworking.NetPiece[] pieceArray = new GameNetworking.NetPiece[chessmen.Length];
            for (int i = 0; i < chessmen.Length; i++)
            {
                pieceArray[i].piece = Chess.PieceToInt(chessmen[i].piece);
                pieceArray[i].player = Chess.PlayerToInt(chessmen[i].player);
                pieceArray[i].x = chessmen[i].GetXBoard();
                pieceArray[i].y = chessmen[i].GetYBoard();
                pieceArray[i].moved = chessmen[i].moved;
                pieceArray[i].oldX = chessmen[i].GetOldX();
                pieceArray[i].oldY = chessmen[i].GetOldY();
                pieceArray[i].capturedPieces = Chess.PackCapturedPieces(chessmen[i].capturedPieces);
            }
            network.SaveBoardServerRpc(pieceArray);
        }
        else
        {
            ShowAllThreats(currentPlayer);

            if(gameMode.currentOption == 2 && aiMoved)
            {
                pieceInteractable = true;
            }
            else
            {
                if(gameMode.currentOption != 2)
                {
                    pieceInteractable = true;
                }
            }
        }
    }

    void PlacePieces(Vector4[] piecesInfo, bool moved)
    {
        for(int i = 0; i < piecesInfo.Length; i++)
        {
            Chessman p = OldCreateWithPlayer(Chess.IntToPlayer((int)piecesInfo[i].x), Chess.IntToPiece((int)piecesInfo[i].y), (int)piecesInfo[i].z, (int)piecesInfo[i].w, moved);

            SetPosition(p);
        }
    }

    void ScaleBoard()
    {
        Vector3 boundary = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float multiplier = ((2f * boundary.x) / (16f / 3f)) * 0.98f;
        board.transform.localScale = new Vector3(multiplier, multiplier, 1);
    }

    public void ScaleExtraBoard()
    {
        Vector3 boundary = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float multiplier = ((2f * boundary.x) / (20f / 3f)) * 0.98f;
        board.transform.localScale = new Vector3(multiplier, multiplier, 1);
    }

    void SetupBoard()
    {
        playerNeutral = new Chessman[neutralPieces.options.Length];
        int preset = chessPreset.currentOption;
        string pawn, bishop, knight, rook;

        if(gameMode.currentOption == 1 && preset == 4)
        {
            preset = 0;
        }

        if (preset == 0)
        {
            pawn = pawnOps[Random.Range(0, pawnOps.Length)];
            bishop = bishopOps[Random.Range(0, bishopOps.Length)];
            knight = knightOps[Random.Range(0, knightOps.Length)];
            rook = rookOps[Random.Range(0, rookOps.Length)];
        }
        else if(preset == 5)
        {
            playerWhite = SpawnSavedBoard("white");
            playerBlack = SpawnSavedBoard("black");

            for (int i = 0; i < playerBlack.Length; i++)
            {
                SetPosition(playerBlack[i]);
                SetPosition(playerWhite[i]);
            }

            SpawnNeutralPieces();

            return;
        }
        else
        {
            pawn = pawnOps[preset - 1];
            bishop = bishopOps[preset - 1];
            knight = knightOps[preset - 1];
            rook = rookOps[preset - 1];
        }

        string bPawn = "black_" + pawn;
        string bBishop = "black_" + bishop;
        string bKnight = "black_" + knight;
        string bRook = "black_" + rook;

        string p = "white";

        playerWhite = new Chessman[] {
            OldCreateWithPlayer(p, rook, 0, 0, false), OldCreateWithPlayer(p, knight, 1, 0, false), OldCreateWithPlayer(p, bishop, 2, 0, false), OldCreateWithPlayer(p, "queen", 3, 0, false), OldCreateWithPlayer(p, "king", 4, 0, false), OldCreateWithPlayer(p, bishop, 5, 0, false), OldCreateWithPlayer(p, knight, 6, 0, false), OldCreateWithPlayer(p, rook, 7, 0, false),

            OldCreateWithPlayer(p, pawn, 0, 1, false), OldCreateWithPlayer(p, pawn, 1, 1, false), OldCreateWithPlayer(p, pawn, 2, 1, false), OldCreateWithPlayer(p, pawn, 3, 1, false), OldCreateWithPlayer(p, pawn, 4, 1, false), OldCreateWithPlayer(p, pawn, 5, 1, false), OldCreateWithPlayer(p, pawn, 6, 1, false), OldCreateWithPlayer(p, pawn, 7, 1, false), };

        p = "black";
        playerBlack = new Chessman[] {
            OldCreateWithPlayer(p, rook, 0, 7, false), OldCreateWithPlayer(p, knight, 1, 7, false), OldCreateWithPlayer(p, bishop, 2, 7, false), OldCreateWithPlayer(p, "queen", 3, 7, false), OldCreateWithPlayer(p, "king", 4, 7, false), OldCreateWithPlayer(p, bishop, 5, 7, false), OldCreateWithPlayer(p, knight, 6, 7, false), OldCreateWithPlayer(p, rook, 7, 7, false),

            OldCreateWithPlayer(p, pawn, 0, 6, false), OldCreateWithPlayer(p, pawn, 1, 6, false), OldCreateWithPlayer(p, pawn, 2, 6, false), OldCreateWithPlayer(p, pawn, 3, 6, false), OldCreateWithPlayer(p, pawn, 4, 6, false), OldCreateWithPlayer(p, pawn, 5, 6, false), OldCreateWithPlayer(p, pawn, 6, 6, false), OldCreateWithPlayer(p, pawn, 7, 6, false), };

        SpawnNeutralPieces();

        //Set all piece positions on the positions board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }

        if(gameMode.currentOption == 1)
        {
            foreach(OnlineSelect.NetChamp champion in network.whiteChamps)
            {
                MovePlate.SetPiece(GetPosition(champion.x+1, champion.y+1), Chess.IntToPlayer(champion.player), Chess.IntToPiece(champion.piece));
            }
            foreach (OnlineSelect.NetChamp champion in network.blackChamps)
            {
                MovePlate.SetPiece(GetPosition(champion.x+1, 8-champion.y), Chess.IntToPlayer(champion.player), Chess.IntToPiece(champion.piece));
            }
        }
    }

    void SpawnNeutralPieces()
    {
        for (int i = 0; i < neutralPieces.currentOption; i++)
        {
            int x = Random.Range(0, 8);
            int y = Random.Range(3, 5);
            playerNeutral[i] = OldCreate("peasant", x, y, false);

            if (GetPosition(x, y) != null)
            {
                Destroy(playerNeutral[i]);
            }
            else
            {
                SetPosition(playerNeutral[i]);
            }
        }
    }

    public string CheckForWinner()
    {
        bool white = false;
        bool black = false;
        Chessman[] chessmen = board.GetComponentsInChildren<Chessman>();
        foreach(Chessman cm in chessmen)
        {
            switch (cm.name)
            {
                case "white_king":
                    white = true;
                    break;
                case "black_king":
                    black = true;
                    break;
                case "white_hikaru":
                    white = true;
                    break;
                case "black_hikaru":
                    black = true;
                    break;
            }
        }

        if(white && !black)
        {
            return "white";
        }
        else if(!white && black)
        {
            return "black";
        }
        return null;
    }


    public void CreateTestPieces()
    {
        testOnly = true;
        pieceInteractable = true;
        gameMode.currentOption = 0;
        currentPlayer = "white";
        if(board == null)
        {
            board = GameObject.FindGameObjectWithTag("Board");
            changePieceButton = FindObjectOfType<ChangePieceButton>();
            changePieceButton.SetGame(this.gameObject.GetComponent<Game>());
        }

        ScaleBoard();

        GameObject[] oldMovePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject obj in oldMovePlates)
        {
            Destroy(obj);
        }

        GameObject[] oldPieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject obj in oldPieces)
        {
            Destroy(obj);
        }

        Chessman[] pieces = { OldCreate("white_" + Chess.IntToPiece(currentTestPiece), 3, 1, false), OldCreate("white_" + Chess.IntToPiece(currentTestPiece), 4, 1, false), OldCreate("white_queen", 3, 0, false), OldCreate("white_king", 4, 0, false) };
        Chessman[] blackPieces = { OldCreate("black_pawn", 2, 6, false), OldCreate("black_pawn", 3, 6, false), OldCreate("black_pawn", 4, 6, false), OldCreate("black_pawn", 5, 6, false) };

        for (int i = 0; i < pieces.Length; i++)
        {
            SetPosition(pieces[i]);
        }

        for (int i = 0; i < blackPieces.Length; i++)
        {
            SetPosition(blackPieces[i]);
        }
    }

    public void CreateCustomBoardPieces()
    {
        testOnly = true;
        customBoard = true;
        pieceInteractable = true;
        gameMode.currentOption = 0;
        currentPlayer = "white";
        if (board == null)
        {
            board = GameObject.FindGameObjectWithTag("Board");
            changePieceButton = FindObjectOfType<ChangePieceButton>();
            changePieceButton.SetDestoryPiece(null);
            changePieceButton.SetGame(this.gameObject.GetComponent<Game>());
        }

        ScaleBoard();

        GameObject[] oldMovePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject obj in oldMovePlates)
        {
            Destroy(obj);
        }

        GameObject[] oldPieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject obj in oldPieces)
        {
            Destroy(obj);
        }

        if (PlayerPrefs.GetInt("cb_0_p") == 0)
        {
            Chessman[] pieces = { OldCreate("white_king", 4, 0, false) };

            for (int i = 0; i < pieces.Length; i++)
            {
                SetPosition(pieces[i]);
            }
        }
        else
        {
            Chessman[] pieces = SpawnSavedBoard("white");

            for (int i = 0; i < pieces.Length; i++)
            {
                SetPosition(pieces[i]);
            }
        }
    }

    Chessman[] SpawnSavedBoard(string player)
    {
        List<Chessman> pieces = new List<Chessman>();

        for(int i = 0; i < 64; i++)
        {
            if(PlayerPrefs.GetInt("cb_" + i + "_p") != 0)
            {
                int p = PlayerPrefs.GetInt("cb_" + i + "_p") - 1;
                int x = PlayerPrefs.GetInt("cb_" + i + "_x");
                int y = PlayerPrefs.GetInt("cb_" + i + "_y");

                if(player != "white")
                {
                    y = 9 - y;
                }

                pieces.Add(CreateWithPlayer(player, Chess.IntToPiece(p), x, y, false));
            }
        }

        return pieces.ToArray();
    }

    public void AddPiece(string player)
    {
        for(int y = 0; y < positions.GetLength(1); y++)
        {
            for (int x = 0; x < positions.GetLength(0); x++)
            {
                if (positions[x, y] == null && y < CUSTOM_BOARD_SPACE && y > 0 && x > 0 && x < 9)
                {
                    Chessman p = CreateWithPlayer("white", player, x, y, false);
                    SetPosition(p);
                    Chessman.DestroyMovePlates();

                    x = positions.GetLength(0);
                    y = positions.GetLength(1);
                }
            }
        }
    }

    public void MakeBestMove()
    {
        string otherPlayer = Chess.IntToPlayer( ((Chess.PlayerToInt(currentPlayer) + 1) % 2) );

        Chessman[] allPieces = board.GetComponentsInChildren<Chessman>();
        Chessman[] myPieces = GetMyPieces(currentPlayer);
        List<PieceMove> allMoves = new List<PieceMove>();

        Chessman[,] boardPos = positions;

        foreach (Chessman cm in myPieces)
        {
            bool[,] pieceAllMoves;
            pieceAllMoves = cm.CalcMoves(this);

            for(int x = 0; x < pieceAllMoves.GetLength(0); x++)
            {
                for (int y = 0; y < pieceAllMoves.GetLength(1); y++)
                {
                    if (pieceAllMoves[x, y])
                    {
                        PieceMove move = new PieceMove { piecePos = new Vector2Int(cm.GetXBoard(), cm.GetYBoard()), targetPos = new Vector2Int(x, y) };

                        allMoves.Add(move);
                    }
                }
            }
        }

        PieceMove bestMove = allMoves[Random.Range(0, allMoves.Count)];
        List<PieceMove> jointBestMoves = new List<PieceMove>();
        int bestPoints = NEGATIVE_INFINITY;
        Debug.Log("CAN DO " + allMoves.Count + " MOVES!!!");

        Chess.Piece[,] pieceBoard = Chess.ChessmanToPieces(positions);

        int i = 0;
        foreach(PieceMove move in allMoves)
        {
            i++;
            Debug.Log("MOVE " + i + ": " + move.piecePos.x + ", " + move.piecePos.y + " TO " + move.targetPos.x + ", " + move.targetPos.y);
        }

        int count = 0;
        foreach(PieceMove move in allMoves)
        {
            boardPos = positions;

            Debug.Log("FIRST THING OF ALL, PIECE IS: " + move.piecePos.x + ", " +  move.piecePos.y + " | " + (count + 1));
            Debug.Log("SECOND THING OF ALL, PIECE IS: " + boardPos[move.targetPos.x, move.targetPos.y] + " | " + (count + 1));

            Chess.Piece[,] changedBoard = pieceBoard;
            Chessman[,] changedPositions = positions;

            int movePoints = 0;

            Chessman getPos = boardPos[move.targetPos.x, move.targetPos.y];

            if (changedPositions[move.piecePos.x, move.piecePos.y] != null)
            {
                Debug.Log("PIECE IS " + changedBoard[move.piecePos.x, move.piecePos.y].piece + " TO " + move.targetPos.x + ", " + move.targetPos.y);
            }
            else
            {
                Debug.Log(move.piecePos.x + ", " + move.piecePos.y + " PIECE IS NULL");
            }

            if(count < allMoves.Count - 1)
            {
                Debug.Log("NEXT PIECE IS " + changedBoard[allMoves[count + 1].piecePos.x, allMoves[count + 1].piecePos.y].piece);
            }

            if (getPos != null)
            {
                Chessman getPiece = getPos;

                if(getPiece.player == otherPlayer)
                {
                    Debug.Log("CAPTURING " + getPiece.piece + " FOR " + Chess.GetPoints()[Chess.PieceToInt(getPiece.piece)]);
                    movePoints += Chess.GetPoints()[Chess.PieceToInt(getPiece.piece)];
                }
            }

            Vector2Int oldMovingPiecePos = new Vector2Int();
            Vector2Int oldTargetPiecePos = new Vector2Int();

            Chessman movingPiece = boardPos[move.piecePos.x, move.piecePos.y];
            Chessman targetPiece = null;
            if(changedPositions[move.targetPos.x, move.targetPos.y] != null)
            {
                targetPiece = changedPositions[move.targetPos.x, move.targetPos.y];
                oldTargetPiecePos = new Vector2Int(targetPiece.GetXBoard(), targetPiece.GetYBoard());
                targetPiece.SetXBoard(-2);
                targetPiece.SetYBoard(-2);
            }
            oldMovingPiecePos = new Vector2Int(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            movingPiece.SetXBoard(move.targetPos.x);
            movingPiece.SetYBoard(move.targetPos.y);

            changedPositions[move.targetPos.x, move.targetPos.y] = movingPiece;
            changedPositions[move.piecePos.x, move.piecePos.y] = null;

            bool[,] threats = CheckAllThreats(currentPlayer, myPieces);

            int worstThreatPoints = 0;
            for (int y = 0; y < threats.GetLength(1); y++)
            {
                for (int x = 0; x < threats.GetLength(0); x++)
                {
                    if (threats[x, y])
                    {
                        if (changedPositions[x, y] != null)
                        {
                            if (changedPositions[x, y].player == currentPlayer)
                            {
                                int points = Chess.GetPoints()[Chess.PieceToInt(changedPositions[x, y].piece)];

                                Debug.Log(changedPositions[x, y].name + " IS AT THREAT FOR " + points);

                                if (points > worstThreatPoints)
                                {
                                    worstThreatPoints = points;
                                }
                            }
                        }
                    }
                }
            }
            movePoints -= worstThreatPoints;


            Debug.Log("THIS MOVE HAS A SCORE OF " + movePoints);

            if (movePoints > bestPoints)
            {
                bestMove = move;
                bestPoints = movePoints;
                jointBestMoves.Clear();
                jointBestMoves.Add(move);

                Debug.Log("NEW BEST MOVE WITH " + bestPoints);
            }
            else if(movePoints == bestPoints)
            {
                jointBestMoves.Add(move);
            }

            Debug.Log(" ");

            movingPiece.SetXBoard(oldMovingPiecePos.x);
            movingPiece.SetYBoard(oldMovingPiecePos.y);
            if(targetPiece != null)
            {
                targetPiece.SetXBoard(oldTargetPiecePos.x);
                targetPiece.SetYBoard(oldTargetPiecePos.y);
            }

            count++;
        }

        bestMove = jointBestMoves[Random.Range(0, jointBestMoves.Count)];

        Debug.Log("BEST MOVE PIECE IS " + positions[bestMove.piecePos.x, bestMove.piecePos.y] + "WITH A SCORE OF " + bestPoints);

        pieceToMove = positions[bestMove.piecePos.x, bestMove.piecePos.y];
        pieceToMove.MakeMovePlates(pieceToMove.piece);

        foreach(MovePlate mp in pieceToMove.movePlates)
        {
            if(mp.GetCoords() == bestMove.targetPos)
            {
                mp.OnMouseUp();
                return;
            }
        }
    }

    public void MakeBetterMove()
    {
        string otherPlayer = Chess.IntToPlayer(((Chess.PlayerToInt(currentPlayer) + 1) % 2));

        Chessman[] allPieces = board.GetComponentsInChildren<Chessman>();
        Chessman[] myPieces = GetMyPieces(currentPlayer);
        List<PieceMove> allMoves = new List<PieceMove>();

        Chessman[,] boardPos = positions;

        foreach (Chessman cm in myPieces)
        {
            bool[,] pieceAllMoves;
            pieceAllMoves = cm.CalcMoves(this);

            for (int x = 0; x < pieceAllMoves.GetLength(0); x++)
            {
                for (int y = 0; y < pieceAllMoves.GetLength(1); y++)
                {
                    if (pieceAllMoves[x, y])
                    {
                        PieceMove move = new PieceMove { piecePos = new Vector2Int(cm.GetXBoard(), cm.GetYBoard()), targetPos = new Vector2Int(x, y) };

                        allMoves.Add(move);
                    }
                }
            }
        }

        PieceMove bestMove = allMoves[Random.Range(0, allMoves.Count)];
        List<PieceMove> jointBestMoves = new List<PieceMove>();
        int bestPoints = NEGATIVE_INFINITY;
        Debug.Log("CAN DO " + allMoves.Count + " MOVES!!!");

        Chess.Piece[,] pieceBoard = Chess.ChessmanToPieces(positions);

        int z = 0;
        foreach (PieceMove move in allMoves)
        {
            z++;
            Debug.Log("MOVE " + z + ": " + move.piecePos.x + ", " + move.piecePos.y + " TO " + move.targetPos.x + ", " + move.targetPos.y);
        }

        int count = 0;
        for(int i = 0; i < allMoves.Count; i++)
        {
            PieceMove move = allMoves[i];
            boardPos = positions;

            Debug.Log("FIRST THING OF ALL, PIECE IS: " + move.piecePos.x + ", " + move.piecePos.y + " | " + (count + 1));
            Debug.Log("SECOND THING OF ALL, PIECE IS: " + boardPos[move.targetPos.x, move.targetPos.y] + " | " + (count + 1));

            Chess.Piece[,] changedBoard = pieceBoard;
            Chessman[,] changedPositions = positions;

            int movePoints = 0;

            Chessman getPos = boardPos[move.targetPos.x, move.targetPos.y];

            if (changedPositions[move.piecePos.x, move.piecePos.y] != null)
            {
                Debug.Log("PIECE IS " + changedBoard[move.piecePos.x, move.piecePos.y].piece + " TO " + move.targetPos.x + ", " + move.targetPos.y);
            }
            else
            {
                Debug.Log(move.piecePos.x + ", " + move.piecePos.y + " PIECE IS NULL");
            }

            if (count < allMoves.Count - 1)
            {
                Debug.Log("NEXT PIECE IS " + changedBoard[allMoves[count + 1].piecePos.x, allMoves[count + 1].piecePos.y].piece);
            }

            if (getPos != null)
            {
                Chessman getPiece = getPos;

                if (getPiece.player == otherPlayer)
                {
                    Debug.Log("CAPTURING " + getPiece.piece + " FOR " + Chess.GetPoints()[Chess.PieceToInt(getPiece.piece)]);
                    movePoints += Chess.GetPoints()[Chess.PieceToInt(getPiece.piece)];
                }
            }

            //Vector2Int oldMovingPiecePos = new Vector2Int();
            //Vector2Int oldTargetPiecePos = new Vector2Int();

            //Chessman movingPiece = boardPos[move.piecePos.x, move.piecePos.y].;
            //Chessman targetPiece = null;
            //if (changedPositions[move.targetPos.x, move.targetPos.y] != null)
            //{
            //    targetPiece = changedPositions[move.targetPos.x, move.targetPos.y].;
            //    oldTargetPiecePos = new Vector2Int(targetPiece.GetXBoard(), targetPiece.GetYBoard());
            //    targetPiece.SetXBoard(-2);
            //    targetPiece.SetYBoard(-2);
            //}
            //oldMovingPiecePos = new Vector2Int(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            //movingPiece.SetXBoard(move.targetPos.x);
            //movingPiece.SetYBoard(move.targetPos.y);

            //changedPositions[move.targetPos.x, move.targetPos.y] = movingPiece.gameObject;
            //changedPositions[move.piecePos.x, move.piecePos.y] = null;

            //bool[,] threats = CheckAllThreats(currentPlayer, myPieces);

            //int worstThreatPoints = 0;
            //for (int y = 0; y < threats.GetLength(1); y++)
            //{
            //    for (int x = 0; x < threats.GetLength(0); x++)
            //    {
            //        if (threats[x, y])
            //        {
            //            if (changedPositions[x, y] != null)
            //            {
            //                if (changedPositions[x, y]..player == currentPlayer)
            //                {
            //                    int points = Chess.GetPoints()[Chess.PieceToInt(changedPositions[x, y]..piece)];

            //                    Debug.Log(changedPositions[x, y].name + " IS AT THREAT FOR " + points);

            //                    if (points > worstThreatPoints)
            //                    {
            //                        worstThreatPoints = points;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //movePoints -= worstThreatPoints;


            Debug.Log("THIS MOVE HAS A SCORE OF " + movePoints);

            if (movePoints > bestPoints)
            {
                bestMove = move;
                bestPoints = movePoints;
                jointBestMoves.Clear();
                jointBestMoves.Add(move);

                Debug.Log("NEW BEST MOVE WITH " + bestPoints);
            }
            else if (movePoints == bestPoints)
            {
                jointBestMoves.Add(move);
            }

            Debug.Log(" ");

            //movingPiece.SetXBoard(oldMovingPiecePos.x);
            //movingPiece.SetYBoard(oldMovingPiecePos.y);
            //if (targetPiece != null)
            //{
            //    targetPiece.SetXBoard(oldTargetPiecePos.x);
            //    targetPiece.SetYBoard(oldTargetPiecePos.y);
            //}

            count++;
        }

        bestMove = jointBestMoves[Random.Range(0, jointBestMoves.Count)];

        Debug.Log("BEST MOVE PIECE IS " + positions[bestMove.piecePos.x, bestMove.piecePos.y] + "WITH A SCORE OF " + bestPoints);

        pieceToMove = positions[bestMove.piecePos.x, bestMove.piecePos.y];
        pieceToMove.MakeMovePlates(pieceToMove.piece);

        foreach (MovePlate mp in pieceToMove.movePlates)
        {
            if (mp.GetCoords() == bestMove.targetPos)
            {
                mp.OnMouseUp();
                return;
            }
        }
    }

    public struct AllPiecesMoves
    {
        public bool[,] moves;
        public Vector2Int piecePos;
    }
    public struct PieceMove
    {
        public Vector2Int targetPos;
        public Vector2Int piecePos;
    }

    IEnumerator PlayComputerMove()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

        aiMoved = true;

        MakeBetterMove();

        pieceInteractable = true;
    }
}
