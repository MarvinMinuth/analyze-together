using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LoadingStatueSO : ScriptableObject
{
    public GameObject statuePrefab;
    public Material material;
    public Material transparentMaterial;
    public Material hmdMaterial;
    public SaveFile saveFile;
    public string messageText;
}
