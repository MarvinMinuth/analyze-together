using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTarget : NetworkBehaviour
{
    private bool networkSpawned = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkSpawned = true;
    }

    private void Update()
    {
        if (!networkSpawned || !IsServer)
        {
            return;
        }
        transform.position = EstimateNewTargetPosition();
    }

    private Vector3 EstimateNewTargetPosition()
    {
        return CalculateCentroid(GameObject.FindGameObjectsWithTag("NetworkPlayer"));
    }

    private Vector3 CalculateCentroid(GameObject[] gameObjects)
    {
        if (gameObjects == null || gameObjects.Length == 0)
        {
            Debug.LogError("Die übergebene Liste ist leer oder null.");
            return Vector3.zero; // Rückgabe eines Standardvektors, wenn die Liste leer oder null ist
        }

        Vector3 sumOfPositions = Vector3.zero;
        foreach (GameObject go in gameObjects)
        {
            if (go != null) // Sicherstellen, dass das GameObject nicht null ist
            {
                sumOfPositions += go.transform.position; // Addiere die Position jedes GameObjects
            }
        }
        return sumOfPositions / gameObjects.Length; // Teile durch die Anzahl der GameObjects, um den Durchschnitt zu berechnen
    }
}
