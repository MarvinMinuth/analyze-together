using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;
using System.Diagnostics.Tracing;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }


    private Lobby hostLobby;
    private float heartbeatInterval;

    private Dictionary<Lobby, string> lobbies = new Dictionary<Lobby, string>();

    private NetworkManager networkManager;

    private async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        networkManager = NetworkManager.Singleton;
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeatInterval -= Time.deltaTime;
            if (heartbeatInterval <= 0)
            {
                float heartbeatIntervalMax = 15;
                heartbeatInterval = heartbeatIntervalMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void CreateRelay(Lobby lobby)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                {"joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode)}
            }
            });

            lobbies[lobby] = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "udp");

            networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            networkManager.StartHost();

        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "Analyze Together";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                {"joinCode", new DataObject(DataObject.VisibilityOptions.Public, "0")}
            }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            Debug.Log("Lobby created: " + lobbyName + " " + maxPlayers);

            CreateRelay(lobby);



        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    public async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            Debug.Log("Joining lobby: " + lobbyId);
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

            Debug.Log("Joined lobby: " + joinedLobby.Name + " " + joinedLobby.Data["joinCode"].Value);
            JoinRelay(joinedLobby.Data["joinCode"].Value);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined relay: " + joinAllocation.AllocationId);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log("Starting client");
            networkManager.StartClient();

        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (hostLobby != null)
        {
            LeaveLobby();
        }
    }

    private void Destroy()
    {
        if (hostLobby != null)
        {
            LeaveLobby();
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            Debug.Log("Lobby left due to application quit.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error leaving lobby: " + e.Message);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer && clientId == NetworkManager.Singleton.LocalClientId)
        {
            LeaveLobby();
        }
    }
}
