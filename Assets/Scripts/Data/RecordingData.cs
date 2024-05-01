using System.Collections.Generic;
using UnityEngine;

public class RecordingData
{
    private SaveFile saveFile;
    private int maxFrame;
    private List<TransformLog> headTransformLogs;
    private List<TransformLog> leftHandTransformLogs;
    private List<TransformLog> rightHandTransformLogs;

    private List<ArmLog> bottomArmLogs;
    private List<ArmLog> middleArmLogs;
    private List<ArmLog> topArmLogs;

    private List<int> bottomArmHighlights;
    private List<int> middleArmHighlights;
    private List<int> topArmHighlights;
    private Dictionary<int, ArmCollisionLog[]> armCollisionLogDic;
    private Dictionary<int, ArmCollisionLog[]> succsessfulArmCollisionLogDic;
    private Dictionary<int, ArmCollisionLog[]> unsuccsessfulArmCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> fightCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> succsessfulFightCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> unsuccsessfulFightCollisionLogDic;

    private Dictionary<int, HRLog> hrLogDic;

    public RecordingData(SaveFile saveFile, List<TransformLog> headTransformLogs, List<TransformLog> leftHandTransformLogs, List<TransformLog> rightHandTransformLogs, List<ArmLog> bottomArmLogs, List<ArmLog> middleArmLogs, List<ArmLog> topArmLogs, List<int> bottomArmHighlights, List<int> middleArmHighlights, List<int> topArmHighlights, Dictionary<int, ArmCollisionLog[]> armCollisionLogDic, Dictionary<int, ArmCollisionLog[]> succsessfulArmCollisionLogDic, Dictionary<int, ArmCollisionLog[]> unsuccsessfulArmCollisionLogDic, Dictionary<int, FightCollisionLog[]> fightCollisionLogDic, Dictionary<int, FightCollisionLog[]> succsessfulFightCollisionLogDic, Dictionary<int, FightCollisionLog[]> unsuccsessfulFightCollisionLogDic, Dictionary<int, HRLog> hrLogDic)
    {
        this.saveFile = saveFile;
        this.headTransformLogs = headTransformLogs;
        this.leftHandTransformLogs = leftHandTransformLogs;
        this.rightHandTransformLogs = rightHandTransformLogs;
        this.bottomArmLogs = bottomArmLogs;
        this.middleArmLogs = middleArmLogs;
        this.topArmLogs = topArmLogs;
        this.bottomArmHighlights = bottomArmHighlights;
        this.middleArmHighlights = middleArmHighlights;
        this.topArmHighlights = topArmHighlights;
        this.armCollisionLogDic = armCollisionLogDic;
        this.succsessfulArmCollisionLogDic = succsessfulArmCollisionLogDic;
        this.unsuccsessfulArmCollisionLogDic = unsuccsessfulArmCollisionLogDic;
        this.fightCollisionLogDic = fightCollisionLogDic;
        this.succsessfulFightCollisionLogDic = succsessfulFightCollisionLogDic;
        this.unsuccsessfulFightCollisionLogDic = unsuccsessfulFightCollisionLogDic;
        this.hrLogDic = hrLogDic;

        maxFrame = Mathf.Min(headTransformLogs.Count - 1, leftHandTransformLogs.Count - 1, rightHandTransformLogs.Count - 1);
    }

    public SaveFile GetSaveFile()
    {
        return saveFile;
    }

    public int GetMaxFrame()
    {
        return maxFrame;
    }
    public List<TransformLog> GetHeadTransformLogs()
    {
        return headTransformLogs;
    }
    public List<TransformLog> GetLeftHandTransformLogs()
    {
        return leftHandTransformLogs;
    }
    public List<TransformLog> GetRightHandTransformLogs()
    {
        return rightHandTransformLogs;
    }
    public List<ArmLog> GetBottomArmLogs()
    {
        return bottomArmLogs;
    }
    public List<ArmLog> GetMiddleArmLogs()
    {
        return middleArmLogs;
    }
    public List<ArmLog> GetTopArmLogs()
    {
        return topArmLogs;
    }
    public List<int> GetBottomArmHighlights()
    {
        return bottomArmHighlights;
    }
    public List<int> GetMiddleArmHighlights()
    {
        return middleArmHighlights;
    }

    public List<int> GetTopArmHighlights()
    {
        return topArmHighlights;
    }

    public Dictionary<int, ArmCollisionLog[]> GetArmCollisionLogs()
    {
        return armCollisionLogDic;
    }

    public Dictionary<int, ArmCollisionLog[]> GetSuccsessfulArmCollisionLogs()
    {
        return succsessfulArmCollisionLogDic;
    }

    public Dictionary<int, ArmCollisionLog[]> GetUnsuccsessfulArmCollisionLogs()
    {
        return unsuccsessfulArmCollisionLogDic;
    }

    public Dictionary<int, FightCollisionLog[]> GetFightCollisionLogs()
    {
        return fightCollisionLogDic;
    }

    public Dictionary<int, FightCollisionLog[]> GetSuccsessfulFightCollisionLogs()
    {
        return succsessfulFightCollisionLogDic;
    }
    public Dictionary<int, FightCollisionLog[]> GetUnsuccsessfulFightCollisionLogs()
    {
        return unsuccsessfulFightCollisionLogDic;
    }
    public Dictionary<int, HRLog> GetHRLogs()
    {
        return hrLogDic;
    }
}
