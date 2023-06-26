using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;

public class OnlineSelect : NetworkBehaviour
{
    bool active;
    [SerializeField] Setting gameMode;
    public SelectManager manager;
    public Text timer;
    public Button conformation;
    bool waiting;
    bool clientReady;
    int count;
    float i;

    public struct NetChamp : INetworkSerializeByMemcpy
    {
        public int x;
        public int y;
        public int player;
        public int piece;

        // REMEMBER TO SERIALIZE EACH VARIABLE

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref player);
            serializer.SerializeValue(ref piece);
        }
    }

    NetChamp SetChamp(Vector2Int pos, string champ)
    {
        NetChamp champion = new NetChamp();
        champion.x = pos.x;
        champion.y = pos.y;
        champion.piece = Chess.PieceToInt(champ);
        champion.player = Chess.PlayerToInt(GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>().player);

        Debug.Log("CHAMPION MADE: " + champion.player + "_" + champion.piece + ", " + champion.x + ", " + champion.y);
        return champion;
    }

    public void EndSelection()
    {
        NetChamp champion1 = SetChamp(manager.pos1, manager.champ1);
        NetChamp champion2 = SetChamp(manager.pos2, manager.champ2);
        waiting = true;

        if (!NetworkManager.IsHost)
        {
            Debug.Log("I'm Client");
            ClientReadyServerRpc();

            RecieveClientChampionsServerRpc(champion1, champion2);
        }
        else
        {
            GameNetworking gameNetworking = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();

            Debug.Log("Host set " + Chess.IntToPlayer(champion1.player) + " champions");
            
            if(champion1.player == Chess.PlayerToInt("white"))
            {
                gameNetworking.whiteChamps = new NetChamp[] { champion1, champion2 };
            }
            else
            {
                gameNetworking.blackChamps = new NetChamp[] { champion1, champion2 };
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RecieveClientChampionsServerRpc(NetChamp champion1, NetChamp champion2)
    {
        GameNetworking gameNetworking = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();

        Debug.Log("Client set " + Chess.IntToPlayer(champion1.player) + " champions");

        if (champion1.player == Chess.PlayerToInt("white"))
        {
            gameNetworking.whiteChamps = new NetChamp[] { champion1, champion2 };
        }
        else
        {
            gameNetworking.blackChamps = new NetChamp[] { champion1, champion2 };
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientReadyServerRpc()
    {
        Debug.Log("Client waiting for host");
        clientReady = true;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Setup()
    {
        if (gameMode.currentOption != 1)
        {
            Destroy(timer.gameObject);
            Destroy(gameObject);
        }

        clientReady = false;

        manager = FindObjectOfType<SelectManager>();
        timer = manager.timer;
        conformation = manager.finishButton;

        count = 30;
        timer.text = count.ToString();
        active = true;
    }

    void Update()
    {
        if (!active) return;

        if(!waiting) i += Time.deltaTime;

        if (i > 1)
        {
            i--;
            count--;
            timer.text = count.ToString();
        }

        if(waiting && clientReady)
        {
            waiting = false;
            StartGameClientRpc();
        }
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        SceneManager.LoadScene("Game");
        Destroy(gameObject);
    }
}
