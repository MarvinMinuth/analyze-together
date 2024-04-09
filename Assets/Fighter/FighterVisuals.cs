using UnityEngine;

public class FighterVisuals : MonoBehaviour
{
    [SerializeField] private MeshRenderer hmdMeshRenderer;
    [SerializeField] private MeshRenderer[] headMeshRenderers;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    [SerializeField] private MeshRenderer[] leftHandMeshRenderers;
    [SerializeField] private MeshRenderer[] rightHandMeshRenderers;
    [SerializeField] private FighterBodyPartVisuals headVisuals, leftHandVisuals, rightHandVisuals;

    private FighterCoordinator fighterCoordinator;

    private void Start()
    {
        fighterCoordinator = FighterCoordinator.Instance;
        if (fighterCoordinator == null) Debug.LogError("No FighterCoordinator found");

        //Hide();
    }

    public void Show()
    {
        headVisuals.Show();
        leftHandVisuals.Show();
        rightHandVisuals.Show();
    }

    public void Hide()
    {
        headVisuals.Hide();
        leftHandVisuals.Hide();
        rightHandVisuals.Hide();
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

    /*
    public void SetHeadByTransformLog(TransformLog transformLog)
    {
        headVisuals.position = transformLog.Position;
        headVisuals.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void SetLeftHandByTransformLog(TransformLog transformLog)
    {
        leftHandVisuals.position = transformLog.Position;
        leftHandVisuals.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void SetRightHandByTransformLog(TransformLog transformLog)
    {
        rightHandVisuals.position = transformLog.Position;
        rightHandVisuals.rotation = Quaternion.Euler(transformLog.Rotation);
    }

    public void ResetVisuals()
    {
        headVisuals.localPosition = Vector3.zero;
        headVisuals.localRotation = Quaternion.identity;

        leftHandVisuals.localPosition = Vector3.zero;
        leftHandVisuals.localRotation = Quaternion.identity;

        rightHandVisuals.localPosition = Vector3.zero;
        rightHandVisuals.localRotation = Quaternion.identity;
    }
    */
}
