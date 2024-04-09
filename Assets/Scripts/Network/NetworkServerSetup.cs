using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkServerSetup : NetworkBehaviour
{
    [Header("Data")]
    [SerializeField] private Transform replayDataPrefab;

    [Header("Fighting Scene")]
    [SerializeField] private Transform fightingScenePrefab;

    [Header("Testing")]
    public LoadButton loadButton;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //Instantiate(replayDataPrefab, Vector3.zero, Quaternion.identity);

            Transform fightingScene = Instantiate(fightingScenePrefab, Vector3.zero, Quaternion.identity);
            fightingScene.GetComponent<NetworkObject>().Spawn();

            loadButton.Setup();
        }
    }
}
