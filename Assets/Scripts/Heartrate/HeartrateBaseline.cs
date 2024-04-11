using UnityEngine;

public class HeartrateBaseline : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    public void SetupBaseline(float lineWidth, float width, float baselineHeartRate, HeartrateGraph heartrateGraph)
    {
        if (!lineRenderer)
        {
            Debug.LogError("LineRenderer is not assigned.");
            return;
        }

        // Setze die Breite der Linie
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Die Y-Position der Linie basierend auf der festgelegten Herzfrequenz und dem overheight-Faktor
        float positionY = heartrateGraph.GetNormalizedHeartRatePosition((int)baselineHeartRate);

        // Stelle sicher, dass die Linie sichtbar ist
        lineRenderer.positionCount = 2;

        // Setze die beiden Endpunkte der Linie
        Vector3 startPosition = new Vector3(-width / 2, positionY, 0);
        Vector3 endPosition = new Vector3(width / 2, positionY, 0);
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);


    }
}
