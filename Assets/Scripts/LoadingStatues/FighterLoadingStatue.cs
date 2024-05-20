using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FighterLoadingStatue : MonoBehaviour
{
    private bool isFinished = false;

    [SerializeField] private LoadingStatueSO loadingStatueSO;
    [SerializeField] private LoadingStatueFighterVisuals loadingStatueFighterVisuals;
    [SerializeField] private LoadingStatueFighter loadingStatueFighter;
    [SerializeField] private LoadingStatueUI loadingStatueUI;
    //[SerializeField] private Transform spotLight;
    [SerializeField] private LoadingStatueCube loadingStatueCube;

    private FighterLoader fighterLoader;
    [SerializeField] private XRSimpleInteractable interactable;

    private void Start()
    {
        fighterLoader = FighterLoader.Instance;
        if (fighterLoader == null)
        {
            Debug.LogError("No FighterLoader found!");
        }
    }

    public void ClickStatue()
    {
        fighterLoader.ShowMessage(this);
    }

    public void HideStatue()
    {
        HideMessage();
        interactable.enabled = false;
        //spotLight.gameObject.SetActive(false);
        loadingStatueFighterVisuals.Hide();
    }

    public void ShowStatue()
    {
        interactable.enabled = true;
        //spotLight.gameObject.SetActive(true);
        loadingStatueFighterVisuals.Show();
    }
    public void LoadReplay()
    {
        fighterLoader.LoadReplay(loadingStatueSO.saveFile);
        DisableInteractable();
    }

    public void Finish()
    {
        isFinished = true;
        HideStatue();
        loadingStatueCube.SetFinished();
    }

    public bool IsFinished()
    {
        return isFinished;
    }

    public void InteractionsAllowed(bool allowed)
    {
        if (interactable != null)
        {
            if (!allowed) { interactable.enabled = false; }
            else { interactable.enabled = true; }
        }
    }

    public void ShowMessage()
    {
        loadingStatueUI.Show();
    }

    public void HideMessage()
    {
        loadingStatueUI.Hide();
    }

    public LoadingStatueSO GetLoadingStatueSO()
    {
        return loadingStatueSO;
    }

    public void MoveLoadingStatueToStartPosition(float time)
    {
        loadingStatueFighter.MoveToStartPosition(time);
    }

    public void ResetLoadingStatueFighter()
    {
        loadingStatueFighter.ResetPositions();
    }

    public void DisableInteractable()
    {
        interactable.enabled = false;
    }

    public void ActivateInteractable()
    {
        interactable.enabled = true;
    }

}
