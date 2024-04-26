using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TimelineMarkers : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private TimelineNew timeline;
    [SerializeField] private float width = 5f;
    public static TimelineMarkers Instance;
    private RecordingManager recordingManager;
    private Dictionary<int, FightCollisionLog[]> unsuccessfulFightCollisionLogDic;

    private List<GameObject> shownMarkers = new List<GameObject>();
    private List<GameObject> markerPool = new List<GameObject>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        recordingManager = RecordingManager.Instance;

        timeline.OnTimelineChanged += Timeline_OnTimelineChanged;
        timeline.OnTimelineLoaded += Timeline_OnTimelineLoaded;
        timeline.OnTimelineReset += Timeline_OnTimelineReset;

        if (timeline.IsTimelineSet())
        {
            Timeline_OnTimelineLoaded(this, System.EventArgs.Empty);
        }
    }
    private void Timeline_OnTimelineReset(object sender, System.EventArgs e)
    {
        ClearMarkers();
        unsuccessfulFightCollisionLogDic = null;
    }

    private void Timeline_OnTimelineLoaded(object sender, System.EventArgs e)
    {
        unsuccessfulFightCollisionLogDic = recordingManager.GetUnsuccsessfulFightCollisionDic();
        if (unsuccessfulFightCollisionLogDic == null)
        {
            StartCoroutine(WaitForLogs());
        }
        else
        {
            Timeline_OnTimelineChanged(sender, e);
        }
    }

    IEnumerator WaitForLogs()
    {
        while (!recordingManager.FileIsLoaded())
        {
            yield return null;
        }

        unsuccessfulFightCollisionLogDic = recordingManager.GetUnsuccsessfulFightCollisionDic();
        Timeline_OnTimelineChanged(this, System.EventArgs.Empty);
    }

    private void Timeline_OnTimelineChanged(object sender, System.EventArgs e)
    {
        if (!timeline.IsTimelineSet())
        {
            return;
        }
        if (unsuccessfulFightCollisionLogDic == null)
        {
            unsuccessfulFightCollisionLogDic = recordingManager.GetUnsuccsessfulFightCollisionDic();
        }
        ClearMarkers();
        CreateMarkers();
    }


    public void CreateMarkers()
    {
        foreach (KeyValuePair<int, FightCollisionLog[]> log in unsuccessfulFightCollisionLogDic)
        {
            SetMarker(timeline, log.Key);
        }
    }

    public void SetMarker(TimelineNew timeline, int frame)
    {
        if (frame < timeline.GetMinValue() || frame > timeline.GetMaxValue())
        {
            return;
        }
        Vector2 markerPosition = GetMarkerPosition(timeline, frame);

        GameObject marker = GetPooledMarker();
        marker.transform.SetParent(timeline.transform, false);

        RectTransform markerRect = marker.GetComponent<RectTransform>();
        markerRect.anchoredPosition = markerPosition; // Verwendung von anchoredPosition statt localPosition

        float oversize = 10f;
        float height = timeline.GetComponent<RectTransform>().rect.height + oversize;
        markerRect.sizeDelta = new Vector2(width, height);
        marker.SetActive(true);

        shownMarkers.Add(marker);
    }


    private GameObject GetPooledMarker()
    {
        foreach (GameObject marker in markerPool)
        {
            if (!marker.activeInHierarchy)
            {
                return marker;
            }
        }

        // no inactive marker found, create a new one
        GameObject newMarker = Instantiate(markerPrefab);
        markerPool.Add(newMarker);
        return newMarker;
    }


    private Vector2 GetMarkerPosition(TimelineNew timeline, int frame)
    {
        if ((float)(timeline.GetMaxValue() - timeline.GetMinValue()) == 0)
        {
            return new Vector2(0, 0);
        }

        float normalizedValue = (frame - timeline.GetMinValue()) / (float)(timeline.GetMaxValue() - timeline.GetMinValue());
        float timelineWidth = timeline.GetComponent<RectTransform>().rect.width;

        float positionX = normalizedValue * timelineWidth - timelineWidth / 2f;
        float positionY = 0;

        return new Vector2(positionX, positionY);
    }

    private void ClearMarkers()
    {
        foreach (GameObject marker in shownMarkers)
        {
            marker.SetActive(false);
        }
        shownMarkers.Clear();
    }

    private void OnDestroy()
    {
        timeline.OnTimelineChanged -= Timeline_OnTimelineChanged;
    }
}
