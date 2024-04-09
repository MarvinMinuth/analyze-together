using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkReplayControllerServer : NetworkBehaviour
{
    public static NetworkReplayControllerServer Instance;
    private ReplayController replayController;

    public struct ReplayData : INetworkSerializable{
        public int loadedReplay;
        public int frame;
        public bool play;
        public bool pause;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref loadedReplay);
            serializer.SerializeValue(ref frame);
            serializer.SerializeValue(ref play);
            serializer.SerializeValue(ref pause);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        replayController = ReplayController.Instance;
    }

    public void LoadTutorialRequest(ulong ownerClientId)
    {
        LoadTutorialClientRpc();
    }

    public void PlayRequest()
    {
        PlayClientRpc();
    }

    public void SetFrameRequest(int frame)
    {
        SetFrameClientRpc(frame);
    }

    public void PauseRequest()
    {
        PauseClientRpc();
    }

    public void SendState(ulong clientId)
    {
        ReplayData data = new ReplayData();
        if (replayController.IsReplayReady())
        {
            data.loadedReplay = 1;
            data.frame = replayController.GetFrame();
        }
        else
        {
            data.loadedReplay = 0;
            data.frame = 0;
        }


        if (replayController.IsPlaying())
        {
            data.play = true;
            data.pause = false;
        }
        else
        {
            data.pause = true;
            data.play = false;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        SendStateClientRpc(data, clientRpcParams);
    }

    [ClientRpc]
    private void SendStateClientRpc(ReplayData replayData, ClientRpcParams clientRpcParams = default) 
    {
        if(replayData.loadedReplay != 0)
        {
            replayController.LoadOnStartReplay();
            replayController.SetFrame(replayData.frame);
        }

        if (replayData.play)
        {
            replayController.Play();
        }
        else if (replayData.pause)
        {
            replayController.Pause();
        }
    }

    [ClientRpc]
    private void LoadTutorialClientRpc()
    {
        replayController.LoadOnStartReplay();
    }

    [ClientRpc]
    private void PlayClientRpc()
    {
        replayController.Play();
    }

    [ClientRpc]
    private void SetFrameClientRpc(int frame)
    {
        replayController.SetFrame(frame);
    }

    [ClientRpc]
    private void PauseClientRpc()
    {
        replayController.Pause();
    }
}
