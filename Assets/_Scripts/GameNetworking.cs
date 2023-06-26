using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameNetworking : NetworkBehaviour
{
    public string player;

    float playerCheckTimer;
    const float PLAYER_CHECK_TICK = 0.6f;

    public OnlineSelect.NetChamp[] whiteChamps;
    public OnlineSelect.NetChamp[] blackChamps;

    public struct NetPiece : INetworkSerializeByMemcpy
    {
        public int x;
        public int y;
        public int player;
        public int piece;
        public bool moved;
        public int oldX;
        public int oldY;
        public int capturedPieces;
        
        // REMEMBER TO SERIALIZE EACH VARIABLE

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T: IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref player);
            serializer.SerializeValue(ref piece);
            serializer.SerializeValue(ref moved);
            serializer.SerializeValue(ref oldX);
            serializer.SerializeValue(ref oldY);
            serializer.SerializeValue(ref capturedPieces);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        playerCheckTimer += Time.deltaTime;
        if(playerCheckTimer > PLAYER_CHECK_TICK)
        {
            playerCheckTimer -= PLAYER_CHECK_TICK;
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                RequestPlayerCountServerRpc();
            }
        }
    }

    void CheckForOtherPlayer(int count)
    {
        if (NetworkManager.IsClient)
        {
            if (count < 2)
            {
                GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>().Menu();
            }
        }
    }

    public override void OnNetworkSpawn()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void SaveBoardServerRpc(NetPiece[] cmPieces)
    {
        SetBoardClientRpc(cmPieces);
    }

    [ClientRpc]
    public void SetBoardClientRpc(NetPiece[] pieces)
    {
        Debug.Log("SETTING BOARD CLIENT");

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Piece");
        int e = 0;
        foreach(GameObject cm in objects)
        {
            Destroy(cm);
            e++;
        }
        foreach(NetPiece piece in pieces)
        {
            Chessman obj;

            int x = piece.x - 1;
            int y = piece.y - 1;

            if (Chess.IntToPlayer(piece.player) == "white" || Chess.IntToPlayer(piece.player) == "black")
            {
                obj = game.OldCreate(Chess.IntToPlayer(piece.player) + "_" + Chess.IntToPiece(piece.piece), x, y, piece.moved);
            }
            else
            {
                obj = game.OldCreate(Chess.IntToPiece(piece.piece), x, y, piece.moved);
            }
            obj.capturedPieces = Chess.UnpackCapturedPieces(piece.capturedPieces);
            game.SetPosition(obj);
        }

        string winner = game.CheckForWinner();
        Debug.Log("Winner: " + winner);
        if (winner != null)
        {
            game.Winner(winner);
        }

        game.ShowAllThreats(game.GetCurrentPlayer());

        game.pieceInteractable = true;
    }

    public void SetBoardRotation()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        if(player == "black")
        {
            game.board.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            game.board.transform.eulerAngles = Vector3.zero;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCurrentPlayerRequestServerRpc(int currentPlayer)
    {
        SetCurrentPlayerClientRpc(currentPlayer);
    }

    [ClientRpc]
    public void SetCurrentPlayerClientRpc(int currentPlayer)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.SetCurrentPlayer(Chess.IntToPlayer(currentPlayer));
    }

    public void Disconnect()
    {
        NetworkManager.Shutdown();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerCountServerRpc()
    {
        CheckForOtherPlayerClientRpc(NetworkManager.ConnectedClients.Count);
    }

    [ClientRpc]
    public void CheckForOtherPlayerClientRpc(int count)
    {
        CheckForOtherPlayer(count);
    }
}