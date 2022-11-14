using UnityEngine;
using PretiaArCloud;

public class ARContentHandler : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private GameObject _arContents;

    void OnEnable()
    {
        _relocManager.OnRelocalized += OnRelocalized;
    }

    void OnDisable()
    {
        _relocManager.OnRelocalized -= OnRelocalized;
    }

    private void OnRelocalized()
    {
        _arContents.SetActive(true);
    }
}