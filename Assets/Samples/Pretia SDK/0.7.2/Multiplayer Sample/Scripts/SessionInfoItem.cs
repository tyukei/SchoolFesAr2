using System;
using PretiaArCloud.Multiplayer.Matchmaking;
using UnityEngine;
using UnityEngine.UI;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class SessionInfoItem : MonoBehaviour
    {
        [SerializeField]
        private Text _sessionNameLabel;

        [SerializeField]
        private Button _joinSessionButton;

        private SessionInfo _sessionInfo;

        private Action<SessionInfo> _joinButtonClickedCallback;

        public void SetSession(SessionInfo sessionInfo)
        {
            _sessionNameLabel.text = sessionInfo.Name;
            _sessionInfo = sessionInfo;
        }

        private void OnEnable()
        {
            _joinSessionButton.onClick.AddListener(InvokeCallback);
        }

        private void OnDisable()
        {
            _joinSessionButton.onClick.RemoveListener(InvokeCallback);
        }

        private void InvokeCallback()
        {
            _joinButtonClickedCallback?.Invoke(_sessionInfo);
        }

        public event Action<SessionInfo> JoinButtonClicked
        {
            add => _joinButtonClickedCallback += value;
            remove => _joinButtonClickedCallback -= value;
        }

    }
}
