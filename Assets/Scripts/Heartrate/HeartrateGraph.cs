using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class HeartrateGraph : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TimelineNew timeline;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float baselineWidth = 0.15f;
    [SerializeField] private float overheight = 2f;

    [SerializeField] private int minHR = 40;
    [SerializeField] private int maxHR = 200;
    [SerializeField] private GameObject heartrateBaselinePrefab;
    [SerializeField] private int baselineHeartrate = 80;
    private ReplayController replayController;
    private RectTransform sliderRectTransform;
    private bool baselineSet = false;
    private bool timelineSet = false;

    void Start()
    {
        replayController = ReplayController.Instance;

        timeline.OnTimelineLoaded += Timeline_OnTimelineLoaded;
        timeline.OnTimelineChanged += Timeline_OnTimelineChanged;
        timeline.OnTimelineReset += Timeline_OnTimelineReset;

        sliderRectTransform = timeline.GetComponent<RectTransform>();
    }

    private void Timeline_OnTimelineLoaded(object sender, System.EventArgs e)
    {
        timelineSet = true;
        SetupHeartrateGraph(replayController.GetRecordingData().GetHRLogs());
    }

    private void Timeline_OnTimelineReset(object sender, System.EventArgs e)
    {
        DeleteGraph();
        timelineSet = false;
    }

    private void Timeline_OnTimelineChanged(object sender, System.EventArgs e)
    {
        if (!timelineSet)
        {
            return;
        }
        DeleteGraph();
        SetupHeartrateGraph(replayController.GetRecordingData().GetHRLogs());
    }

    public void SetupHeartrateGraph(Dictionary<int, HRLog> hrLogDic)
    {
        lineRenderer.startWidth = lineWidth;
        int minFrame = (int)timeline.GetMinValue();
        int maxFrame = (int)timeline.GetMaxValue();
        float width = sliderRectTransform.rect.width;

        if (!baselineSet)
        {
            GameObject baseline = Instantiate(heartrateBaselinePrefab, transform);
            baseline.GetComponent<HeartrateBaseline>().SetupBaseline(baselineWidth, width, baselineHeartrate, this);
            baselineSet = true;
        }

        // Anpassen des normalisierten X-Wertes, um von minFrame zu starten
        float normalizedXValue = width / (maxFrame - minFrame);

        List<Vector3> positions = new List<Vector3>();

        // Initialize with start and end positions at minY (default or some base value)
        float baseY = GetNormalizedHeartRatePosition(hrLogDic[FindClosestKey(minFrame)].heartRate);
        positions.Add(new Vector3(-(width / 2), baseY, 0)); // Startpunkt

        foreach (KeyValuePair<int, HRLog> log in hrLogDic)
        {
            if (log.Value.heartRate < 20 || log.Key < minFrame || log.Key > maxFrame)
            {
                continue; // Überspringe niedrige Herzfrequenzen
            }

            float positionX = ((log.Key - minFrame) * normalizedXValue) - (width / 2);
            float positionY = GetNormalizedHeartRatePosition(log.Value.heartRate);
            Vector3 position = new Vector3(positionX, positionY, 0);
            positions.Add(position);
        }

        baseY = GetNormalizedHeartRatePosition(hrLogDic[FindClosestKey(maxFrame)].heartRate);
        positions.Add(new Vector3(width / 2, baseY, 0)); // Endpunkt

        // Set positions to the line renderer
        lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }

        // Optional: Simplify the line to reduce complexity
        lineRenderer.Simplify(0.01f);
    }

    public float GetNormalizedHeartRatePosition(int heartRate)
    {
        float height = sliderRectTransform.rect.height;

        float baseMinY = -height / 2 * overheight;
        float baseMaxY = height / 2 * overheight;

        float normalizedHR = (float)(heartRate - minHR) / (maxHR - minHR);
        float positionY = normalizedHR * (baseMaxY - baseMinY) + baseMinY;

        return positionY * overheight;
    }



    public int FindClosestKey(int targetValue)
    {
        Dictionary<int, HRLog> hrLog = replayController.GetRecordingData().GetHRLogs();
        // Verwende LINQ, um den Schlüssel zu finden, der dem gegebenen Wert am nächsten ist
        var closestKey = hrLog.Keys.OrderBy(key => Math.Abs(key - targetValue)) // Sortiere die Schlüssel nach ihrer Differenz zum Zielwert
                                   .First(); // Nimm den ersten, also den nächsten Schlüssel

        return closestKey;
    }




    private Color GetLineColorBasedOnHeartRate(int heartRate)
    {
        if (heartRate < 60)
        {
            return Color.blue;
        }
        else if (heartRate >= 60 && heartRate < 100)
        {
            return Color.green;
        }
        else if (heartRate >= 100 && heartRate < 140)
        {
            return Color.yellow;
        }
        else if (heartRate >= 140 && heartRate < 170)
        {
            return new Color(241, 90, 34);
        }
        else
        {
            return Color.red;
        }
    }

    public void DeleteGraph()
    {
        lineRenderer.positionCount = 0;
    }

}
