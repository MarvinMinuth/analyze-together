using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingStatueFighterVisuals : MonoBehaviour
{
    [SerializeField] private FighterLoadingStatue loadingStatue;
    [SerializeField] private MeshRenderer hmdMesh;

    private void Start()
    {
        ChangeMaterial(loadingStatue.GetLoadingStatueSO().material);

        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ChangeMaterial(Material material)
    {
        foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer == hmdMesh) continue;
            renderer.material = material;
        }
    }
}
