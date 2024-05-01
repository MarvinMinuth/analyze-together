using System.Collections.Generic;
using UnityEngine;

public class HitSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource punchAudioSource, swordAudioSource;

    private ReplayController replayController;
    private Dictionary<int, FightCollisionLog[]> fightCollisionDic;

    private void Start()
    {
        replayController = ReplayController.Instance;

        replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
        replayController.OnActiveFrameChanged += ReplayController_OnFrameChanged;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        fightCollisionDic = null;
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        if (!replayController.IsPlaying()) { return; }
        if (fightCollisionDic.ContainsKey(e.newActiveFrame))
        {
            foreach (FightCollisionLog fightCollisionLog in fightCollisionDic[e.newActiveFrame])
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
        fightCollisionDic = replayController.GetRecordingData().GetFightCollisionLogs();
    }
}
