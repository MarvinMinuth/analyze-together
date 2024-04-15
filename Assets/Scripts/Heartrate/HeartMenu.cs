using UnityEngine;

public class HeartMenu : MonoBehaviour
{
    public bool isActivated;
    [SerializeField] private bool stayOnActivated;
    [SerializeField] private GameObject canvas;
    [SerializeField] private HeartControlButtons buttons;
    [SerializeField] private Transform anchor;

    private Transform anchorParent;

    private void Awake()
    {
        Deactivate();
    }
    public void Deactivate()
    {
        isActivated = false;
        canvas.SetActive(false);
        if (anchorParent != null)
        {
            anchor.SetParent(anchorParent);
        }
    }

    public void Activate()
    {
        anchorParent = anchor.parent;
        anchor.SetParent(HeartrateCoordinator.Instance.transform);
        isActivated = true;
        canvas.SetActive(true);
        buttons.CheckAllButtonStatus();
    }
}
