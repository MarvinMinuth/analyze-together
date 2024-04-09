using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailViewProvider : MonoBehaviour
{
    [SerializeField] private Transform markerPrefab;
    [SerializeField] private Timeline overviewTimeline;
    [SerializeField] private Timeline detailviewTimeline;
    [SerializeField] private bool useDescriptionLines;
}
