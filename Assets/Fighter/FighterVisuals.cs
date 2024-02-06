using UnityEngine;

public class FighterVisuals : MonoBehaviour
{
    [SerializeField] private MeshRenderer hmdMeshRenderer;
    [SerializeField] private MeshRenderer[] headMeshRenderers;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    [SerializeField] private MeshRenderer[] leftHandMeshRenderers;
    [SerializeField] private MeshRenderer[] rightHandMeshRenderers;
    [SerializeField] private Transform headTransform, leftHandTransform, rightHandTransform;

    private FighterCoordinator fighterCoordinator;

    private void Start()
    {
        fighterCoordinator = FighterCoordinator.Instance;
        if (fighterCoordinator == null) Debug.LogError("No FighterCoordinator found");
    }
    public void Show()
    {
        headTransform.gameObject.SetActive(true);
        leftHandTransform.gameObject.SetActive(true);
        rightHandTransform.gameObject.SetActive(true);
    }

    public void Hide()
    {
        headTransform.gameObject.SetActive(false);
        leftHandTransform.gameObject.SetActive(false);
        rightHandTransform.gameObject.SetActive(false);
    }

    public void ChangeMaterial(Material material, bool changeHMDMaterial)
    {
        ChangeHeadMaterial(material, changeHMDMaterial);
        ChangeLeftHandMaterial(material);
        ChangeRightHandMaterial(material);
        ChangeBodyMaterial(material);
    }

    public void ChangeHeadMaterial(Material material, bool changeHMDMaterial)
    {
        foreach (MeshRenderer renderer in headMeshRenderers)
        {
            renderer.material = material;
        }
        if (changeHMDMaterial) ChangeHMDMaterial(material);
    }

    public void ChangeLeftHandMaterial(Material material)
    {
        foreach (MeshRenderer renderer in leftHandMeshRenderers)
        {
            renderer.material = material;
        }
    }
    public void ChangeRightHandMaterial(Material material)
    {
        foreach (MeshRenderer renderer in rightHandMeshRenderers)
        {
            renderer.material = material;
        }
    }
    public void ChangeHMDMaterial(Material material)
    {
        hmdMeshRenderer.material = material;
    }

    public void ChangeBodyMaterial(Material material)
    {
        bodyMeshRenderer.material = material;
    }

    public void SetHeadByTransformLog(TransformLog transformLog)
    {
        headTransform.position = transformLog.Position;
        headTransform.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void SetLeftHandByTransformLog(TransformLog transformLog)
    {
        leftHandTransform.position = transformLog.Position;
        leftHandTransform.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void SetRightHandByTransformLog(TransformLog transformLog)
    {
        rightHandTransform.position = transformLog.Position;
        rightHandTransform.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void ResetVisuals()
    {
        headTransform.localPosition = Vector3.zero;
        headTransform.localRotation = Quaternion.identity;

        leftHandTransform.localPosition = Vector3.zero;
        leftHandTransform.localRotation = Quaternion.identity;

        rightHandTransform.localPosition = Vector3.zero;
        rightHandTransform.localRotation = Quaternion.identity;
    }
}
