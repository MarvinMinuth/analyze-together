using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkServerSetup : NetworkBehaviour
{
    public static NetworkServerSetup Instance { get; private set; }
    [Header("Data")]
    [SerializeField] private Transform replayDataPrefab;

    [Header("Fighting Scene")]
    [SerializeField] private Transform fightingScenePrefab;

    [Header("Testing")]
    public LoadButton loadButton;

    public event EventHandler OnServerSetupComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one NetworkServerSetup found");
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //Instantiate(replayDataPrefab, Vector3.zero, Quaternion.identity);

            Transform fightingScene = Instantiate(fightingScenePrefab, Vector3.zero, Quaternion.identity);
            fightingScene.GetComponent<NetworkObject>().Spawn();

            loadButton.Setup();

            OnServerSetupComplete?.Invoke(this, EventArgs.Empty);
        }
    }
}
