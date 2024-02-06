using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [SerializeField] private Renderer[] meshToDisable;

    public override void OnNetworkSpawn()
    {

        // Disable visualisation of own network-visuals
        if (!IsOwner) { return; }
        foreach (var mesh in meshToDisable)
        {
            mesh.enabled = false;
        }
    }

    private void Update()
    {
        // Update position and rotation of own network-visuals by referencing the VR-Rig
        if (!IsOwner)
        {
            return;
        }

        root.position = VRRigReferences.Instance.root.position;
        root.rotation = VRRigReferences.Instance.root.rotation;

        head.position = VRRigReferences.Instance.head.position;
        head.rotation = VRRigReferences.Instance.head.rotation;

        leftHand.position = VRRigReferences.Instance.leftHand.position;
        leftHand.rotation = VRRigReferences.Instance.leftHand.rotation;

        rightHand.position = VRRigReferences.Instance.rightHand.position;
        rightHand.rotation = VRRigReferences.Instance.rightHand.rotation;
    }
}
