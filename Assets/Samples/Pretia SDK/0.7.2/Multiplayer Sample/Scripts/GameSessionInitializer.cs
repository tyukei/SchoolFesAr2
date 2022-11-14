using System.Collections.Generic;
using PretiaArCloud.Multiplayer.Matchmaking;
using PretiaArCloud.Networking;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class GameSessionInitializer : MonoBehaviour
    {

        [SerializeField]
        private ARSharedAnchorManager _arSharedAnchorManager;

        [SerializeField]
        private GameObject _sessionListPanel;

        [SerializeField]
        private Transform _sessionListContentRoot;

        [SerializeField]
        private SessionInfoItem _sessionItemPrefab;

        [SerializeField]
        private InputField _sessionNameInput;

        private NetworkManager _networkManager => NetworkManager.Instance;
        private IMatchmaker _matchmaker = Matchmaker.Instance;

        private string _token;
        private Dictionary<string, string> _sessionFilter;

        private IGameSession _gameSession = default;

        private async void Start()
        {
            if (false == await _networkManager.ConnectAsync())
            {
                throw new System.Exception("Unable to connect to the network");
            }
            
            
            string guest = $"{SystemInfo.deviceUniqueIdentifier}_{System.DateTime.Now.Second}";
            var (statusCode, token, displayName) = await _networkManager.GuestLoginAsync(guest);

            if (statusCode == NetworkStatusCode.Success)
            {
                _token = StringEncoder.Instance.GetString(token);
            }
            else
            {
                throw new System.Exception($"Failed to login: {statusCode}");
            }


#if UNITY_EDITOR || UNITY_STANDALONE
            await GetExistingSessionsAsync("");
#endif
        }

        private void OnEnable ()
        {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
            _arSharedAnchorManager.OnMapRelocalized += GetExistingSessionsAsync;
#endif
        }

        private void OnDisable ()
        {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
            _arSharedAnchorManager.OnMapRelocalized -= GetExistingSessionsAsync;
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private async Task GetExistingSessionsAsync(string mapKey)
#else
        private async void GetExistingSessionsAsync(string mapKey)
#endif
        {
            _sessionFilter = new Dictionary<string, string>();
            _sessionFilter["map_key"] = mapKey;

            var sessions = await _matchmaker.GetSessionInfoAsync(_token, _sessionFilter);
            foreach (var session in sessions)
            {
                var item = Instantiate(_sessionItemPrefab, Vector3.zero, Quaternion.identity, _sessionListContentRoot);
                item.SetSession(session);
                item.JoinButtonClicked += JoinSessionAsync;
            }

            _sessionListPanel.SetActive(true);
        }

        private async void JoinSessionAsync(SessionInfo sessionInfo)
        {
            var ipEndPoint = await _matchmaker.GetMatchIPAsync(_token, sessionInfo);
            _gameSession = await _networkManager.CreateGameSessionAsync(ipEndPoint, _token, sessionInfo.MatchKey);
            _sessionListPanel.SetActive(false);
        }

        public async void CreateNewSessionAsync()
        {
            string sessionName = _sessionNameInput.text;
            var (sessionInfo, ipEndPoint) = await _matchmaker.CreateSessionAsync(_token, sessionName, priv: false, _sessionFilter);
            _gameSession = await _networkManager.CreateGameSessionAsync(ipEndPoint, _token, sessionInfo.MatchKey);
            _sessionListPanel.SetActive(false);
        }

        /// <summary>Called on application out.</summary>
        private void OnApplicationQuit()
        {
            if (!_gameSession.Disposed)
            {
                _gameSession.Dispose();
            }
        }
        
    }
}