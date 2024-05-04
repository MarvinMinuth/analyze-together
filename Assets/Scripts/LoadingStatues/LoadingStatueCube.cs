using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingStatueCube : MonoBehaviour
{
    [SerializeField] private GameObject finishedSymbol;
    [SerializeField] private GameObject remainingSymbol;

    private void Awake()
    {
        finishedSymbol.SetActive(false);
        remainingSymbol.SetActive(true);
    }

    public void SetFinished()
    {
        remainingSymbol.SetActive(false);
        finishedSymbol.SetActive(true);
    }
}
