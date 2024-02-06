using System.Collections.Generic;
using UnityEngine;

public class HitSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource punchAudioSource, swordAudioSource;
    

    private ReplayManager replayManager;
    private ReplayController replayController;
    private Dictionary<int, FightCollisionLog[]> fightCollisionDic;

    private void Start()
    {
        replayManager = ReplayManager.Instance;
        replayController = ReplayController.Instance;

        replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
        replayController.OnFrameChanged += ReplayController_OnFrameChanged;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        fightCollisionDic = null;
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnFrameChangedEventArgs e)
    {
        if(!replayController.IsRunning()) { return; }
        if (fightCollisionDic.ContainsKey(e.frame))
        {
            foreach (FightCollisionLog fightCollisionLog in fightCollisionDic[e.frame])
            {
                if (fightCollisionLog != null)
                {
                    if (fightCollisionLog.ExpectedCollisionType == 2) swordAudioSource.Play();
                    else punchAudioSource.Play();
                }
            }
        }
    }

    private void ReplayController_OnReplayDataReady(object sender, System.EventArgs e)
    {
        fightCollisionDic = replayManager.GetFightCollisionDic();
    }
}
