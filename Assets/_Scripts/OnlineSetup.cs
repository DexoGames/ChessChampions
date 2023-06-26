using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
using UnityEngine.SceneManagement;

public class OnlineSetup : NetworkBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button menuButton;

    QueryResponse lobbies;
    [SerializeField] OnlineStatusScript status;
    const float HEARTBEAT_LENGTH = 12;
    const float INITIALIZE_LENGTH = 3;
    float heatbeatTimer;
    float initializeTimer;
    Lobby hostLobby;
    Lobby joinedLobby;

    void Start()
    {
        Initialize();
        heatbeatTimer = HEARTBEAT_LENGTH;
        initializeTimer = INITIALIZE_LENGTH;
    }

    void Update()
    {
        HandleLobbyHeartbeat();
        CheckInitialize();
    }

    public void SetButtons(bool input)
    {
        playButton.interactable = input;
        hostButton.interactable = input;
        menuButton.interactable = input;

        if (!input)
        {
            StartCoroutine(SetMenuButton());
        }
    }

    IEnumerator SetMenuButton()
    {
        yield return new WaitForSeconds(3);

        menuButton.interactable = true;
    }

    async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heatbeatTimer -= Time.deltaTime;
            if (heatbeatTimer <= 0)
            {
                heatbeatTimer = HEARTBEAT_LENGTH;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    void CheckInitialize()
    {
        initializeTimer -= Time.deltaTime;
        if(initializeTimer <= 0)
        {
            initializeTimer = INITIALIZE_LENGTH;

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Initialize();
            }
        }
    }

    public void HostGame()
    {
        StartCoroutine(IHostGame());
    }
    public void JoinGame()
    {
        StartCoroutine(IJoinGame());
    }

    IEnumerator IHostGame()
    {
        GameNetworking network = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();
        network.player = Chess.IntToPlayer(Random.Range(0, 2));
        NetworkManager.StartHost();
        yield return new WaitUntil(() => NetworkManager.IsHost && NetworkManager.ConnectedClientsList.Count > 1);
        Debug.Log("Waiting for client to be ready");
    }
    IEnumerator IJoinGame()
    {
        GameNetworking network = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();
        NetworkManager.StartClient();
        yield return new WaitUntil(() => NetworkManager.IsConnectedClient);
        GetPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void GetPlayerServerRpc()
    {
        GameNetworking network = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();
        Debug.Log("Got other player (" + network.player + ")");
        SetPlayerAndStartClientRpc(Chess.PlayerToInt(network.player));
    }

    [ClientRpc]
    void SetPlayerAndStartClientRpc(int serverPlayer)
    {
        GameNetworking network = GameObject.FindGameObjectWithTag("GameNetworking").GetComponent<GameNetworking>();

        if (!IsHost)
        {
            Debug.Log("Got other player (" + serverPlayer + ")");
            network.player = Chess.IntToPlayer(1 - serverPlayer);
        }
        Debug.Log("About to join room + (" + network.player + ")");
        StartCoroutine(ButtonCommands.OnlineMultiplayer());
    }

    async void Initialize()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed In"); };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch
        {

        }

        status.SetConnectedStatus();
    }

    public async void CreateLobby()
    {
        SetButtons(false);

        try
        {
            string lobbyName = "Lobby" + Random.Range(100, 999) + "-" + System.DateTime.Now.Second;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { "ready", new DataObject(DataObject.VisibilityOptions.Public, "0") }

            }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2);

            joinedLobby = lobby;

            Debug.Log("Created Lobby! " + lobby.Name);

            StartGame();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void CheckToJoinLobbies()
    {
        SetButtons(false);

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.EQ) },
                Order = new List<QueryOrder> { new QueryOrder(false, QueryOrder.FieldOptions.Created) },
                };


            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach(Lobby lobby in queryResponse.Results)
            {
                if(lobby.Data["ready"].Value == "0")
                {
                    queryResponse.Results.Remove(lobby);
                }
            }

            Debug.Log("Lobbies Found (" + queryResponse.Results.Count + ")");

            if(queryResponse.Results.Count == 1)
            {
                status.SetText("1 PLAYER FOUND");
            }
            else
            {
                status.SetText(queryResponse.Results.Count + " PLAYERS FOUND");
            }

            lobbies = queryResponse;

            CreateOrJoinLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void CreateOrJoinLobby()
    {
        if(lobbies.Results.Count > 0)
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbies.Results[0].Id);

            status.SetText("JOINING GAME");

            JoinRelay(lobby.Data["joinCode"].Value);
        }
        else
        {
            CreateLobby();
        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Relay Created (" + joinCode + ")");

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            status.SetText("WAITING FOR OTHER PLAYER");

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            JoinGame();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
        try
        {
            string relayCode = await CreateRelay();

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
            Data = new Dictionary<string, DataObject>
            {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                { "ready", new DataObject(DataObject.VisibilityOptions.Public, "1") }
            } } );;

            joinedLobby = lobby;

            HostGame();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void StopConnecting()
    {
        DisconnectLobby();
    }

    async void DisconnectLobby()
    {
        NetworkManager.Singleton.Shutdown();

        if (joinedLobby != null)
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            //await Lobbies.Instance.DeleteLobbyAsync(joinedLobby.Id);

            joinedLobby = null;
        }

        SetButtons(true);
        status.SetConnectedStatus();
        ButtonCommands.ChangeMenuType(MenuType.Main);
    }

    public async void LeaveLobby() // REPLACED BY DISCONNECTLOBBY() 
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        NetworkManager.Shutdown();
    }
}