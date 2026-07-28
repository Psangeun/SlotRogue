using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using R3;
using SlotRogue.UI.GameFlow;
using SlotRogue.UI.Iap;
using SlotRogue.UI.Leaderboard;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SlotRogue.UI.App
{
    /// <summary>
    /// GameStart(로비) 씬의 최상위 조립자입니다.
    /// ViewModel 생성 → View 바인딩 → 시작/종료 흐름 연결만 담당합니다(ADR-0020).
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameStartSceneRoot : MonoBehaviour
    {
        private const float LobbyResourceRefreshIntervalSeconds = 1f;

        private static bool _openLeaderboardOnNextLoad;

        [SerializeField] private GameStartView _view;
        [SerializeField] private LeaderboardView _leaderboardView;
        [SerializeField] private Button _rankingButton;
        [Header("Lobby HUD")]
        [SerializeField] private TMP_Text _profileNameText;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private TMP_Text _energyTimerText;
        [SerializeField] private TMP_Text _currencyText;
        [SerializeField] private TMP_Text _lobbyStatusText;
        [SerializeField] private LobbyProfileSelectionView _profileSelectionView;
        [Header("Settings")]
        [SerializeField] private GameObject _settingPanel;
        [SerializeField] private Button _settingOpenButton;
        [SerializeField] private Button _settingCloseButton;
        [FormerlySerializedAs("_replayTutorialButton")]
        [SerializeField] private Button _tutorialResetButton;
        [FormerlySerializedAs("_loginView")]
        [SerializeField] private PlayerNicknameSetupView _nicknameSetupView;
        [SerializeField] private Button _removeAdsButton;
        [SerializeField] private TMP_Text _removeAdsButtonText;
        [SerializeField] private CodelessIAPButton _removeAdsIapButton;
        [SerializeField] private IapFulfillmentHandler _iapFulfillmentHandler;
        [SerializeField] private RectTransform _lobbyPlanetLayer;
        [SerializeField] private RectTransform[] _lobbyPlanets;

        [Header("Lobby Planet Animation")]
        [Tooltip("행성별 부유 속도/위치/투명도 프리셋. 비어 있으면 기본값을 사용합니다.")]
        [SerializeField] private LobbyPlanetPreset[] _planetPresets =
            LobbySpaceBackground.CreateDefaultPresets();

        private GameStartViewModel _viewModel;
        private LeaderboardViewModel _leaderboardViewModel;
        private LobbyProfileSelectionViewModel _profileSelectionViewModel;
        private readonly LobbySpaceBackground _lobbyBackground = new();
        private string _currentLobbyNickname = string.Empty;
        private string _lobbyStatusMessage = string.Empty;
        private float _nextLobbyResourceRefreshTime;
        private bool _startGameInProgress;

        public static void RequestOpenLeaderboardOnNextLoad()
        {
            _openLeaderboardOnNextLoad = true;
        }

        private void Awake()
        {
            IapStoreConnectionCallbacks.Register();

            _viewModel = new GameStartViewModel();
            _leaderboardViewModel = new LeaderboardViewModel();
            _profileSelectionViewModel = new LobbyProfileSelectionViewModel();

            if (_view == null)
            {
                Debug.LogError(
                    "[GameStartSceneRoot] GameStartView must be wired in the inspector.");
            }

            EnsureLeaderboardView();
            EnsureNicknameSetupView();
            _lobbyBackground.Bind(_lobbyPlanetLayer, _lobbyPlanets, _planetPresets);
            ValidateLobbyHudReferences();
            EnsureProfileSelectionView();
            RenderLobbyHud();
            ConfigureSettingsPanel();
            ConfigureRemoveAdsButton();
            AdsRemoveState.Changed += HandleAdsRemoveChanged;
            LobbyPlayerResourceStore.Changed += HandleLobbyResourcesChanged;

            if (_view != null)
            {
                _view.StartRequested += _viewModel.RequestStartGame;
                _view.QuitRequested += _viewModel.RequestQuitGame;
            }

            _viewModel.StartGameRequested += StartGame;
            _viewModel.QuitGameRequested += QuitGame;

            if (_leaderboardView != null)
            {
                // 상태 구독·close/refresh 입력은 View.Bind가 소유한다(ADR-0020).
                // 리더보드 팝업은 자기 실행(launcher) 버튼을 들지 않는다.
                _leaderboardView.Bind(_leaderboardViewModel);
            }

            // 랭킹 열기는 로비가 소유하는 별도 버튼이 담당한다.
            if (_rankingButton != null)
            {
                _rankingButton.onClick.AddListener(HandleRankingClicked);
            }

            if (_nicknameSetupView != null)
            {
                // 닉네임 설정 View는 같은 leaderboard 상태를 렌더한다(View 전용 ViewModel이 아니라
                // SceneRoot가 구독을 소유). 구독 즉시 현재 값이 1회 흘러 초기 Render를 처리한다.
                _nicknameSetupView.Initialize();
                _nicknameSetupView.NicknameSubmitted += HandleNicknameSubmitted;
                _leaderboardViewModel.State.Subscribe(_nicknameSetupView.Render).AddTo(this);
            }

            _leaderboardViewModel.State.Subscribe(HandleLeaderboardStateChanged).AddTo(this);
        }

        private void Start()
        {
            if (_openLeaderboardOnNextLoad)
            {
                _openLeaderboardOnNextLoad = false;
                _leaderboardViewModel?.OpenAsync().Forget();
                return;
            }

            _leaderboardViewModel?.EvaluateProfileRequirement();
            RenderLobbyHud();
        }

        private void Update()
        {
            _lobbyBackground.Tick(Time.unscaledDeltaTime);
            if (Time.unscaledTime >= _nextLobbyResourceRefreshTime)
            {
                _nextLobbyResourceRefreshTime =
                    Time.unscaledTime + LobbyResourceRefreshIntervalSeconds;
                RenderLobbyHud();
            }
        }

        // 인스펙터에서 프리셋 배열이 비었을 때 우클릭 메뉴로 기본값을 채운다.
        [ContextMenu("행성 프리셋 기본값으로 채우기")]
        private void FillDefaultPlanetPresets()
        {
            _planetPresets = LobbySpaceBackground.CreateDefaultPresets();
        }

        private void OnDestroy()
        {
            AdsRemoveState.Changed -= HandleAdsRemoveChanged;
            LobbyPlayerResourceStore.Changed -= HandleLobbyResourcesChanged;

            if (_view != null)
            {
                _view.StartRequested -= _viewModel.RequestStartGame;
                _view.QuitRequested -= _viewModel.RequestQuitGame;
            }

            if (_viewModel != null)
            {
                _viewModel.StartGameRequested -= StartGame;
                _viewModel.QuitGameRequested -= QuitGame;
            }

            _profileSelectionViewModel?.Dispose();

            // Leaderboard close/refresh와 상태 구독은 View.Bind(.AddTo)가 소유해 자동 해제된다.
            // SceneRoot가 직접 연결한 랭킹 버튼만 여기서 해제한다(ADR-0020).
            if (_rankingButton != null)
            {
                _rankingButton.onClick.RemoveListener(HandleRankingClicked);
            }

            if (_settingOpenButton != null)
            {
                _settingOpenButton.onClick.RemoveListener(HandleSettingOpenClicked);
            }

            if (_settingCloseButton != null)
            {
                _settingCloseButton.onClick.RemoveListener(HandleSettingCloseClicked);
            }

            if (_tutorialResetButton != null)
            {
                _tutorialResetButton.onClick.RemoveListener(HandleTutorialResetClicked);
            }

            if (_nicknameSetupView != null)
            {
                _nicknameSetupView.NicknameSubmitted -= HandleNicknameSubmitted;
            }
        }

        private void HandleRankingClicked()
        {
            _leaderboardViewModel.OpenAsync().Forget();
        }

        private void HandleSettingOpenClicked()
        {
            if (_settingPanel == null)
            {
                Debug.LogError(
                    "[GameStartSceneRoot] SettingPanel must be wired in the inspector.");
                return;
            }

            _settingPanel.SetActive(true);
            _settingPanel.transform.SetAsLastSibling();
        }

        private void HandleSettingCloseClicked()
        {
            if (_settingPanel != null)
            {
                _settingPanel.SetActive(false);
            }
        }

        private void HandleTutorialResetClicked()
        {
            FirstRunTutorialState.ResetForDebug();
        }

        private void HandleNicknameSubmitted(string nickname)
        {
            _leaderboardViewModel.SaveProfileAsync(nickname).Forget();
        }

        private void HandleLeaderboardStateChanged(LeaderboardViewState state)
        {
            if (!string.IsNullOrWhiteSpace(state?.PlayerName))
            {
                _currentLobbyNickname = state.PlayerName.Trim();
            }
            else
            {
                RefreshLobbyNicknameFromStore();
            }

            RenderLobbyHud();
        }

        private void HandleLobbyResourcesChanged(
            LobbyPlayerResourceSnapshot snapshot)
        {
            RenderLobbyHud(snapshot);
        }

        private void EnsureLeaderboardView()
        {
            if (_leaderboardView != null)
            {
                _leaderboardView.gameObject.SetActive(true);
                _leaderboardView.EnsureRuntimeLayout();
                return;
            }

            Debug.LogError(
                "[GameStartSceneRoot] LeaderboardView must be wired in the inspector.");
        }

        private void EnsureNicknameSetupView()
        {
            if (_nicknameSetupView != null)
            {
                return;
            }

            Debug.LogError(
                "[GameStartSceneRoot] PlayerNicknameSetupView must be wired in the inspector.");
        }

        private void EnsureProfileSelectionView()
        {
            if (_profileSelectionView != null)
            {
                _profileSelectionView.Bind(_profileSelectionViewModel);
                return;
            }

            Debug.LogError(
                "[GameStartSceneRoot] LobbyProfileSelectionView must be wired in the inspector.");
        }

        private void ValidateLobbyHudReferences()
        {
            if (_profileNameText != null &&
                _energyText != null &&
                _energyTimerText != null &&
                _currencyText != null)
            {
                return;
            }

            var builder = new System.Text.StringBuilder();
            AppendMissing(builder, _profileNameText != null, "Profile Name Text");
            AppendMissing(builder, _energyText != null, "Energy Text");
            AppendMissing(builder, _energyTimerText != null, "Energy Timer Text");
            AppendMissing(builder, _currencyText != null, "Currency Text");
            Debug.LogError(
                "[GameStartSceneRoot] Lobby HUD references must be wired in the inspector. " +
                $"Missing: {builder}",
                this);
        }

        private void ConfigureSettingsPanel()
        {
            if (_settingPanel != null)
            {
                _settingPanel.SetActive(false);
            }
            else
            {
                Debug.LogError(
                    "[GameStartSceneRoot] SettingPanel must be wired in the inspector.");
            }

            if (_settingOpenButton != null)
            {
                _settingOpenButton.onClick.AddListener(HandleSettingOpenClicked);
            }
            else
            {
                Debug.LogError(
                    "[GameStartSceneRoot] Setting Open Button must be wired in the inspector.");
            }

            if (_settingCloseButton != null)
            {
                _settingCloseButton.onClick.AddListener(HandleSettingCloseClicked);
            }
            else
            {
                Debug.LogError(
                    "[GameStartSceneRoot] Setting Close Button must be wired in the inspector.");
            }

            // 튜토리얼 초기화: 완료 플래그를 지워 다음 Play가 튜토리얼로 시작되게 한다.
            if (_tutorialResetButton != null)
            {
                _tutorialResetButton.onClick.AddListener(HandleTutorialResetClicked);
            }
            else
            {
                Debug.LogError(
                    "[GameStartSceneRoot] Tutorial Reset Button must be wired in the inspector.");
            }
        }

        private void ConfigureRemoveAdsButton()
        {
            if (_removeAdsButton == null ||
                _removeAdsButtonText == null ||
                _removeAdsIapButton == null ||
                _iapFulfillmentHandler == null)
            {
                Debug.LogError(
                    "[GameStartSceneRoot] Remove Ads Button references are missing.");
                return;
            }

            _removeAdsIapButton.productId = AdsRemoveState.ProductId;
            _removeAdsIapButton.buttonType = CodelessButtonType.Purchase;
            _removeAdsIapButton.automaticallyConfirmTransaction = true;
            _removeAdsIapButton.button = _removeAdsButton;

            RenderRemoveAdsButton(AdsRemoveState.IsRemoved);
        }

        private void HandleAdsRemoveChanged(bool isRemoved)
        {
            RenderRemoveAdsButton(isRemoved);
        }

        private void RenderRemoveAdsButton(bool isRemoved)
        {
            if (_removeAdsButton != null)
            {
                _removeAdsButton.interactable = !isRemoved;
            }

            if (_removeAdsButtonText != null)
            {
                _removeAdsButtonText.text = isRemoved
                    ? "광고 제거 구매 완료"
                    : "광고 제거 구매";
            }
        }

        private void StartGame()
        {
            StartGameAsync().Forget();
        }

        private async UniTaskVoid StartGameAsync()
        {
            if (_leaderboardViewModel == null ||
                !_leaderboardViewModel.HasCompletedProfile)
            {
                _leaderboardViewModel?.RequireProfile();
                return;
            }

            if (!LobbyPlayerResourceStore.TrySpendStartEnergy(
                    out LobbyPlayerResourceSnapshot resources))
            {
                _lobbyStatusMessage =
                    $"에너지가 부족합니다. 게임 시작에는 {resources.StartEnergyCost}개가 필요합니다.";
                RenderLobbyHud(resources);
                return;
            }

            if (_startGameInProgress)
            {
                return;
            }

            _startGameInProgress = true;
            _lobbyStatusMessage = "게임 준비 중...";
            RenderLobbyHud(resources);
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (!FirstRunTutorialState.IsCompleted)
            {
                GameFlowSession.StartTutorialRun();
            }
            else
            {
                // 이어하기는 Boot의 ResumePanel에서만 처리하고, 로비 Play는 새 런을 시작합니다.
                GameFlowSession.StartNewRun();
            }

            try
            {
                await GameSceneLoader.LoadRunGameAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameStartSceneRoot] RunGame scene load failed: {ex.Message}");
                _startGameInProgress = false;
                _lobbyStatusMessage = "게임 시작에 실패했습니다.";
                RenderLobbyHud();
            }
        }

        private void RenderLobbyHud()
        {
            RenderLobbyHud(LobbyPlayerResourceStore.Load());
        }

        private void RenderLobbyHud(LobbyPlayerResourceSnapshot resources)
        {
            RefreshLobbyNicknameFromStore();

            if (_profileNameText != null)
            {
                _profileNameText.text = _currentLobbyNickname;
            }

            if (_energyText != null)
            {
                _energyText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}",
                    resources.Energy,
                    resources.MaxEnergy);
            }

            if (_energyTimerText != null)
            {
                bool isRecovering = !resources.IsEnergyFull;
                _energyTimerText.gameObject.SetActive(isRecovering);
                _energyTimerText.text = isRecovering
                    ? FormatEnergyTimerLabel(resources)
                    : string.Empty;
            }

            if (_currencyText != null)
            {
                _currencyText.text =
                    resources.Currency.ToString(CultureInfo.InvariantCulture);
            }

            if (_lobbyStatusText != null)
            {
                _lobbyStatusText.text = _lobbyStatusMessage;
            }
        }

        internal static string FormatEnergyTimerLabel(
            LobbyPlayerResourceSnapshot resources)
        {
            if (resources.IsEnergyFull)
            {
                return string.Empty;
            }

            int remainingSeconds = Math.Max(
                0,
                (int)Math.Ceiling(resources.TimeUntilNextEnergy.TotalSeconds));
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}",
                minutes,
                seconds);
        }

        private void RefreshLobbyNicknameFromStore()
        {
            if (LeaderboardPlayerProfileStore.TryLoad(
                    out LeaderboardPlayerProfile profile))
            {
                _currentLobbyNickname = profile.Nickname;
            }
        }

        private static void AppendMissing(
            System.Text.StringBuilder builder,
            bool hasReference,
            string label)
        {
            if (hasReference)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(label);
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
