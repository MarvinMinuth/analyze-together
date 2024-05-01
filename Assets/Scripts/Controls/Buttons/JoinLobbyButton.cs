using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyButton : MonoBehaviour
{
    private string lobbyName;
    private string lobbyId;
    private LobbyManager lobbyManager;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    public void SetLobbyName(string name){
        text.text = "Join " + name;
        lobbyName = name;
    }

    public void SetLobbyId(string id){
        lobbyId = id;
        button.onClick.AddListener(JoinLobby);
    }

    public void JoinLobby(){
        lobbyManager.JoinLobbyById(lobbyId);
    }

    public void SetLobbyManager(LobbyManager manager){
        lobbyManager = manager;
    }
}
