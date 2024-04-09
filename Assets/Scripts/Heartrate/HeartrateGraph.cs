using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeartrateGraph : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TimelineNew timeline;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float overheight = 2f;
    private RecordingManager recordingManager;
    private RectTransform sliderRectTransform;
    private bool logsLoaded = false;

    void Start()
    {
        recordingManager = RecordingManager.Instance;
        recordingManager.OnRecordingLoaded += RecordingManager_OnRecordingLoaded;
        recordingManager.OnRecordingUnloaded += RecordingManager_OnRecordingUnloaded;

        timeline.OnTimelineChanged += Timeline_OnTimelineChanged;

        sliderRectTransform = timeline.GetComponent<RectTransform>();
    }

    private void RecordingManager_OnRecordingUnloaded(object sender, System.EventArgs e)
    {
        logsLoaded = false;
        DeleteGraph();
    }

    private void RecordingManager_OnRecordingLoaded(object sender, RecordingManager.OnRecordingLoadedEventArgs e)
    {
        logsLoaded = true;
    }

    private void Timeline_OnTimelineChanged(object sender, System.EventArgs e)
    {
        if (!logsLoaded)
        {
            return;
        }
        DeleteGraph();
        SetupHeartrateGraph(recordingManager.GetHRLog());
    }

    public void SetupHeartrateGraph(Dictionary<int, HRLog> hrLogDic)
    {
        lineRenderer.startWidth = lineWidth;
        int minFrame = (int)timeline.GetMinValue();
        int maxFrame = (int)timeline.GetMaxValue();

        float width = sliderRectTransform.rect.width;

        // Anpassen des normalisierten X-Wertes, um von minFrame zu starten
        float normalizedXValue = width / (maxFrame - minFrame);

        lineRenderer.positionCount = hrLogDic.Count;
        int point = 0;

        foreach (KeyValuePair<int, HRLog> log in hrLogDic)
        {
            if (log.Value.heartRate < 20 || log.Key < minFrame || log.Key > maxFrame)
            {
                continue; // Überspringe niedrige Herzfrequenzen
            }

            float positionX = ((log.Key - minFrame) * normalizedXValue) - (width / 2);
            float positionY = GetNormalizedHeartRatePosition(log.Value.heartRate);

            Vector3 position = new Vector3(positionX, positionY, 0);
            lineRenderer.SetPosition(point, position);
            point++;
        }

        // Optional: Farben und Vereinfachung einstellen
        lineRenderer.Simplify(0.01f);
    }

    private float GetNormalizedHeartRatePosition(int heartRate)
    {
        int minHR = 40, maxHR = 200;

        // minY und maxY basieren auf der tatsächlichen Höhe der Timeline
        float height = sliderRectTransform.rect.height;
        float baseMinY = -height / 2;  // Grundlegende minY-Position ohne overheight-Anpassung
        float baseMaxY = height / 2;   // Grundlegende maxY-Position ohne overheight-Anpassung

        // Normalisiere die Herzfrequenz im gewünschten Y-Bereich
        float normalizedHR = (float)(heartRate - minHR) / (maxHR - minHR);

        // Berechne die Position basierend auf dem normalisierten Herzfrequenzwert und der Höhe
        float positionY = normalizedHR * (baseMaxY - baseMinY) + baseMinY;

        // Anpassen der Y-Position basierend auf overheight
        // Der Mittelpunkt (0) bleibt gleich, aber die Spreizung um diesen Punkt wird durch overheight angepasst
        positionY *= overheight;

        return positionY;
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
