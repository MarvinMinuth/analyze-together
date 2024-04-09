using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class JoinLobbyButtons : MonoBehaviour
{
    [SerializeField] private Transform joinLobbyButtonPrefab;
    [SerializeField] private Transform joinLobbyButtonParent;
    [SerializeField] private LobbyManager lobbyManager;

    private List<Transform> joinLobbyButtons = new List<Transform>();

    public async void RefreshList()
    {
        // Clear the existing buttons
        foreach (Transform button in joinLobbyButtons)
        {
            Destroy(button.gameObject);
        }
        joinLobbyButtons.Clear();

        // Create new buttons for each lobby
        try{
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        foreach (Lobby lobby in queryResponse.Results){
            CreateJoinLobbyButton(lobby.Name, lobby.Id);
        }
        }
        catch (System.Exception e){
            Debug.Log("Error: " + e.Message);
        }
    }

    public void CreateJoinLobbyButton(string lobbyName, string lobbyId)
    {
        Transform button = Instantiate(joinLobbyButtonPrefab, joinLobbyButtonParent);
        button.GetComponent<JoinLobbyButton>().SetLobbyManager(lobbyManager);
        button.GetComponent<JoinLobbyButton>().SetLobbyName(lobbyName);
        button.GetComponent<JoinLobbyButton>().SetLobbyId(lobbyId);
        joinLobbyButtons.Add(button);
    }
}
