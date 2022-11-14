using UnityEngine;
using UnityEngine.UI;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class UIShooterController : MonoBehaviour
    {
        [SerializeField]
        private ARSharedAnchorManager _relocManager;
        [SerializeField]
        private Text _relocalizationStatusLabel;
        [SerializeField]
        private Transform _progressBar;

        private void OnEnable()
        {
            _relocManager.OnSharedAnchorStateChanged += OnSharedAnchorStateChanged;
            _relocManager.OnScoreUpdated += UpdateProgressBar;
        }

        private void OnDisable()
        {
            _relocManager.OnSharedAnchorStateChanged -= OnSharedAnchorStateChanged;
            _relocManager.OnScoreUpdated -= UpdateProgressBar;
        }

        private void OnSharedAnchorStateChanged(SharedAnchorState state)
        {
            _relocalizationStatusLabel.text = state.ToString();
        }

        private void UpdateProgressBar(float score)
        {
            _progressBar.localScale = new Vector3(score, transform.localScale.y, transform.localScale.z);
        }

    }
}