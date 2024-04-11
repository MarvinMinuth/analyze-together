using System.Collections;
using System.Collections.Generic;
using ES3Internal;
using TMPro;
using UnityEngine;

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
    private RecordingManager recordingManager;
    private RectTransform sliderRectTransform;
    private bool logsLoaded = false;
    private bool baselineSet = false;

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

        if (!baselineSet)
        {
            GameObject baseline = Instantiate(heartrateBaselinePrefab, transform);
            baseline.GetComponent<HeartrateBaseline>().SetupBaseline(baselineWidth, width, baselineHeartrate, this);
            baselineSet = true;
        }

        // Anpassen des normalisierten X-Wertes, um von minFrame zu starten
        float normalizedXValue = width / (maxFrame - minFrame);

        lineRenderer.positionCount = hrLogDic.Count;

        List<Vector3> positions = new List<Vector3>();

        int count = 0;
        foreach (KeyValuePair<int, HRLog> log in hrLogDic)
        {
            count++;
            if (log.Value.heartRate < 20 || log.Key < minFrame || log.Key > maxFrame)
            {
                if (log.Key > maxFrame && positions[positions.Count - 1].x != width / 2)
                {
                    Vector3 endPosition = new Vector3(width / 2, positions[positions.Count - 1].y, 0);
                    positions.Add(endPosition);
                }
                continue; // Überspringe niedrige Herzfrequenzen
            }

            float positionX = ((log.Key - minFrame) * normalizedXValue) - (width / 2);
            float positionY = GetNormalizedHeartRatePosition(log.Value.heartRate);

            Vector3 position = new Vector3(positionX, positionY, 0);

            if (positionX != -(width / 2) && positions.Count == 0)
            {
                positionX = -(width / 2);
                Vector3 startPosition = new Vector3(positionX, positionY, 0);
                positions.Add(startPosition);
            }

            positions.Add(position);

            if (count == hrLogDic.Count)
            {
                Vector3 endPosition = new Vector3(width / 2, positionY, 0);
                positions.Add(endPosition);
            }
        }

        lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }

        // Optional: Farben und Vereinfachung einstellen
        lineRenderer.Simplify(0.01f);
    }

    public float GetNormalizedHeartRatePosition(int heartRate)
    {
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
