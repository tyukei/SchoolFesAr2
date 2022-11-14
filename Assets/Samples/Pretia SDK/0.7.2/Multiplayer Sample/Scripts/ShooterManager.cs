using System.Collections.Generic;
using PretiaArCloud.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class ShooterManager : MonoBehaviour
    {
        private const float SPAWN_FORWARD_DISTANCE = 2.5f;

        [SerializeField]
        private ARSharedAnchorManager _relocManager;
        [SerializeField]
        private NetworkIdentity _characterPrefab;
        [SerializeField]
        private NetworkCameraManager _networkCameraManager;

        [SerializeField]
        private Material[] _materials;

        [SerializeField]
        private PlayerIdManager _playerIdManager;

        [Header("UI")]
        [SerializeField]
        private Button _shootButton;
        [SerializeField]
        private Button _jumpButton;
        [SerializeField]
        private Button _logoutButton;
        [SerializeField]
        private Button _backButton;

        private IGameSession _gameSession;
        private Dictionary<Player, ShooterFacade> _ownerToShooterMap 
            = new Dictionary<Player, ShooterFacade>();

        private async void OnEnable()
        {
            _networkCameraManager.OnLocalProxyReady += SpawnCharacter;

            _gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
            _gameSession.OnPlayerJoined += AssignPlayerId;
            _gameSession.NetworkSpawner.OnInstantiated += SetupCharacter;
            _gameSession.OnDisconnected += RemoveFromMap;
            _gameSession.OnDisconnected += ReturnPlayerId;
            _gameSession.PlayerMsg.Register<NetworkShootMsg>(InvokeShootOnCharacter);

            _logoutButton.onClick.AddListener(Logout);
        }

        private void Start()
        {
            _relocManager.OnRelocalized += ConnectSession;
        }

        private void OnDisable()
        {
            _networkCameraManager.OnLocalProxyReady -= SpawnCharacter;

            if (_gameSession != null)
            {
                _gameSession.OnPlayerJoined -= AssignPlayerId;
                _gameSession.NetworkSpawner.OnInstantiated -= SetupCharacter;
                _gameSession.PlayerMsg.Unregister<NetworkShootMsg>(InvokeShootOnCharacter);
                _gameSession.OnDisconnected -= RemoveFromMap;
                _gameSession.OnDisconnected -= ReturnPlayerId;
            }

            _jumpButton.onClick.RemoveAllListeners();
            _shootButton.onClick.RemoveAllListeners();

            _logoutButton.onClick.RemoveListener(Logout);
        }

        private void AssignPlayerId(Player player)
        {
            _playerIdManager.Rent(player.UserNumber);
        }

        private void ReturnPlayerId(Player player)
        {
            _playerIdManager.Return(player.UserNumber);
        }

        private void RemoveFromMap(Player player)
        {
            _ownerToShooterMap.Remove(player);
        }

        private void InvokeShootOnCharacter(NetworkShootMsg msg, Player sender)
        {
            if (_ownerToShooterMap.TryGetValue(sender, out ShooterFacade shooter))
            {
                shooter.AnimController.ShootAnimation();
            }
        }

        private void SpawnCharacter(Transform localProxy)
        {
            var spawnPosition = localProxy.position + (localProxy.forward * SPAWN_FORWARD_DISTANCE);
            spawnPosition.y = localProxy.position.y;

            _gameSession.NetworkSpawner.Instantiate(_characterPrefab, spawnPosition, Quaternion.identity, _gameSession.LocalPlayer);
        }

        private void SetupCharacter(NetworkIdentity networkIdentity)
        {
            var shooterFacade = networkIdentity.GetComponent<ShooterFacade>();
            if (shooterFacade != null)
            {
                if (_networkCameraManager.TryGetProxyByOwner(networkIdentity.Owner, out var proxy))
                {
                    shooterFacade.FollowObject.Target = proxy;
                    shooterFacade.AimTowards.Target = proxy;
                }

                _ownerToShooterMap.Add(networkIdentity.Owner, shooterFacade);

                int playerId = _playerIdManager.GetIdByUserNumber(networkIdentity.Owner.UserNumber);
                shooterFacade.ShooterColor.ChangeColor(_materials[playerId]);

                if (networkIdentity.Owner == _gameSession.LocalPlayer)
                {
                    _jumpButton.onClick.AddListener(() => shooterFacade.NetworkJump.TriggerJump());
                    _shootButton.onClick.AddListener(() => _gameSession.PlayerMsg.Send(new NetworkShootMsg()));
                }
            }
        }

        private async void ConnectSession()
        {
            var gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
            await gameSession.ConnectSessionAsync();
            _jumpButton.gameObject.SetActive(true);
            _shootButton.gameObject.SetActive(true);
            _logoutButton.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(false);
            _relocManager.OnRelocalized -= ConnectSession;
        }

        private void Logout()
        {
            _relocManager.ResetSharedAnchor();
            _gameSession.Dispose();
            NetworkManager.Instance.Logout();

            SceneManager.LoadScene(Constants.MULTIPLAYER_SCENE);
        }

    }
}