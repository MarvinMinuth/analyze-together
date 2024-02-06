using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuTimeline : Timeline
{
    // Slider
    [Header("Slider")]
    [SerializeField] private float threshold = 60f;
    [SerializeField] private Slider timelineSlider;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image handleImage;

    //Replay
    private ReplayController replayController;

    // Markers
    /*
    [Header("Marker")]
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private Color markerColor = Color.white;
    [SerializeField] private float highlightThreshold = 120f;
    private List<int> shownHighlights = new List<int>();
    private Dictionary<int, GameObject> shownMarkers = new Dictionary<int, GameObject>();
    [SerializeField] private bool showUnsuccsessfulFightCollisionHighlights;
    
    private int activatedButton;
    [SerializeField] private int windowSize = 180;
    */

    private int minFrame, maxFrame;
    private bool isDragged = false;
    private bool wasRunning;

    private int activeFrame;
    protected override void Start()
    {
        base.Start();
        replayController = ReplayController.Instance;

        replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
        replayController.OnStop += ReplayController_OnStop;

        timelineSlider.maxValue = 1;
    }


    private void ReplayController_OnStop(object sender, EventArgs e)
    {
    }

    protected override void ReplayController_OnReplayControllerUnload(object sender, EventArgs e)
    {
        ResetTimeline();
    }

    private void ReplayController_OnReplayControllerLoaded(object sender, EventArgs e)
    {
        SetupTimeline();
    }

    private void SetupTimeline()
    {
        timelineSlider.maxValue = (replayController.GetReplayLength()) - 2;

        maxFrame = (int)timelineSlider.maxValue;
        minFrame = (int)timelineSlider.minValue;
        activeFrame = minFrame;
    }

    private void ResetTimeline()
    {
        timelineSlider.value = 0;
        timelineSlider.maxValue = 0;
    }

    public override void StartDrag()
    {
        Debug.Log("Start Drag");
        TriggerOnTimelineUsed(this);

        wasRunning = replayController.IsRunning();
        replayController.Pause();
        isDragged = true;
        replayController.SetReceivingInput(true);
    }

    public void HandleSliderInteraction(BaseEventData eventData)
    {
        if (!isDragged)
        {
            StartDrag();
        }
    }

    public override void EndDrag()
    {
        Debug.Log("End Drag");
        isDragged = false;
        if (wasRunning)
        {
            replayController.Play();
        }
        replayController.SetReceivingInput(false);
        TriggerOnTimelineFreed();
    }

    public void OnTimelineValueChanged()
    {
        float value = timelineSlider.value;
        if (!isDragged) { return; }
        if (value < minFrame) { value = minFrame; }
        if (value > maxFrame) { value = maxFrame; }

        // Bestimme die Differenz zwischen dem aktuellen Wert und dem neuen Wert
        float valueDifference = Mathf.Abs(activeFrame - value);

        // Überprüfe, ob die Differenz größer als eine bestimmte Schwelle ist
        if (valueDifference > threshold)
        {
            // Setze den Replay-Zeitpunkt auf den Wert des Timelines
            activeFrame = (int)value;
            replayController.SetFrame(activeFrame);
        }

        /*
        int closestHighlight = minFrame;
        if(shownHighlights.Count != 0) { closestHighlight = shownHighlights.OrderBy(x => Mathf.Abs((long)x - value)).First(); }


        // Überprüfe, ob ein Highlight in der Nähe des neuen Werts existiert
        if (Mathf.Abs(closestHighlight - value) <= highlightThreshold)
        {
            // Setze den Replay-Zeitpunkt auf den nächsten Highlight-Wert
            activeFrame = closestHighlight;
            replayController.SetFrame(activeFrame);
        }
        else
        {
            // Bestimme die Differenz zwischen dem aktuellen Wert und dem neuen Wert
            float valueDifference = Mathf.Abs(activeFrame - value);

            // Überprüfe, ob die Differenz größer als eine bestimmte Schwelle ist
            if (valueDifference > threshold)
            {
                // Setze den Replay-Zeitpunkt auf den Wert des Timelines
                activeFrame = (int)value;
                replayController.SetFrame(activeFrame);
            }
        }
        */
    }


    void Update()
    {
        if (!replayController.IsReplayReady()) { timelineSlider.value = 0; }
        else if(isDragged) { return; }
        else { timelineSlider.value = replayController.GetFrame(); }
    }

    public float GetMaxValue()
    {
        return timelineSlider.maxValue;
    }

    public float GetMinValue()
    {
        return timelineSlider.minValue;
    }

    protected override void Timeline_OnTimelineUsed(object sender, OnTimelineUsedEventArgs e)
    {
        if (e.usedTimeline != this)
        {
            timelineSlider.interactable = false;
            backgroundImage.raycastTarget = false;
            fillImage.raycastTarget = false;
            handleImage.raycastTarget = false;
        }

    }
    protected override void Timeline_OnTimelineFreed(object sender, EventArgs e)
    {
        timelineSlider.interactable = true;
        backgroundImage.raycastTarget = true;
        fillImage.raycastTarget = true;
        handleImage.raycastTarget = true;
    }

    /*
    public Vector2 GetMarkerPosition(float value)
    {
        float normalizedValue = (value - timelineSlider.minValue) / (timelineSlider.maxValue - timelineSlider.minValue);
        float timelineWidth = timelineSlider.GetComponent<RectTransform>().rect.width;
        float positionX = timelineSlider.transform.localPosition.x - timelineWidth / 2f + normalizedValue * timelineWidth;

        Vector3 position = new Vector3(positionX, timelineSlider.transform.localPosition.y, timelineSlider.transform.localPosition.z);
        Vector2 markerPosition = new Vector2(positionX, timelineSlider.transform.localPosition.y);

        return markerPosition;
    }

    public void SetMark(int frame, Color color)
    {
        if (frame < 6) { return; }
        if (shownHighlights.Contains(frame))
        {
            shownHighlights.Add(frame);
            return;
        }
        else
        {
            shownHighlights.Add(frame);
            Vector2 markerPosition = GetMarkerPosition(frame);
            GameObject mark = Instantiate(markerPrefab, transform, false);
            mark.GetComponent<Marker>().frame = frame;
            mark.GetComponent<RectTransform>().localPosition = markerPosition;
            mark.GetComponent<Image>().color = color;
            shownMarkers.Add(frame, mark);
        }
    }

    public void OnMarkerButtonClick(int frame)
    {
        int oldActivatedButton = activatedButton;
        // deactivate active marker
        DeactivateActiveMarker();

        if (frame != oldActivatedButton)
            // if a new marker is selected, activate it
        {
            activatedButton = frame;
            SetButtonActive(shownMarkers[activatedButton].GetComponent<Button>());
            SetButtonActive(shownMarkers[activatedButton].transform.Find("Top Button").GetComponent<Button>());
            minFrame = frame - (windowSize / 2);
            if (minFrame < timelineSlider.minValue) { minFrame = (int)timelineSlider.minValue; }
            maxFrame = frame + (windowSize / 2);
            if (maxFrame > timelineSlider.maxValue) { maxFrame = (int)timelineSlider.maxValue; }
            replayController.ChangeReplayWindow(minFrame, maxFrame);
            replayController.SetFrame(minFrame);
        }    
    }

    public void DeactivateActiveMarker()
    {
        if (shownMarkers.ContainsKey(activatedButton))
        {
            SetButtonInactive(shownMarkers[activatedButton].GetComponent<Button>());
            SetButtonInactive(shownMarkers[activatedButton].transform.Find("Top Button").GetComponent<Button>());
            replayController.ResetReplayWindow();
        }
        minFrame = (int)timelineSlider.minValue;
        maxFrame = (int)timelineSlider.maxValue;
        activatedButton = 0;
    }

    public void RemoveMark(int frame)
    {
        if (!shownHighlights.Contains(frame)) return;
        shownHighlights.Remove(frame);
        if (!shownHighlights.Contains(frame))
        {
            Destroy(shownMarkers[frame]);
            if (frame == activatedButton)
            {
                minFrame = (int)timelineSlider.minValue;
                maxFrame = (int)timelineSlider.maxValue;
                replayController.ResetReplayWindow();
            }
            shownMarkers.Remove(frame);
        }

    }

    public void RemoveAllMarkers()
    {
        DeactivateActiveMarker();
        foreach (KeyValuePair <int, GameObject> marker in shownMarkers)
        {
            Destroy(marker.Value);
        }
        shownHighlights.Clear();
        shownMarkers.Clear();
    }

    public void RemoveFightCollisionHighlights(Dictionary<int, FightCollisionLog[]> fightCollisionDic)
    {
        foreach (KeyValuePair<int, FightCollisionLog[]> log in fightCollisionDic)
        {
            RemoveMark(log.Key);
        }
    }

    public void CreateFightCollisionHighlights(Dictionary<int, FightCollisionLog[]> fightCollisionDic)
    {
        foreach (KeyValuePair<int, FightCollisionLog[]> log in fightCollisionDic)
        {
            SetMark(log.Key, markerColor);
        }
    }

    public void SwitchUnsuccsessfulFightCollisionHighlights()
    {
        if (showUnsuccsessfulFightCollisionHighlights)
        {
            CreateFightCollisionHighlights(ReplayManager.Instance.GetUnsuccsessfulFightCollisionDic());
        }
        else
        {
            RemoveFightCollisionHighlights(ReplayManager.Instance.GetUnsuccsessfulFightCollisionDic());
        }

    }

    */


}