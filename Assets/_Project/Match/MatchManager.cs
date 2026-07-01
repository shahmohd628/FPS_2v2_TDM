using Unity.Netcode;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private CountdownUI countdownUI;
    [SerializeField] private ResultsScreen resultsScreen;
    [SerializeField] private HUD hud;

    void Start()
    {
        // NetworkGameManager may spawn slightly after this — guard with a check
        if (NetworkGameManager.Instance != null)
            Subscribe();
        else
            Invoke(nameof(Subscribe), 0.5f);
    }

    void Subscribe()
    {
        if (NetworkGameManager.Instance == null) { Invoke(nameof(Subscribe), 0.5f); return; }
        NetworkGameManager.Instance.OnStateChanged += HandleStateChange;
    }

    void OnDestroy()
    {
        if (NetworkGameManager.Instance != null)
            NetworkGameManager.Instance.OnStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(MatchState newState)
    {
        switch (newState)
        {
            case MatchState.Lobby:
                hud?.Hide();
                countdownUI?.Hide();
                resultsScreen?.Hide();
                SetPlayerInputEnabled(false);
                break;

            case MatchState.Countdown:
                countdownUI?.Show();
                SetPlayerInputEnabled(false);
                break;

            case MatchState.Playing:
                countdownUI?.Hide();
                hud?.Show();
                SetPlayerInputEnabled(true);
                break;

            case MatchState.MatchEnd:
                hud?.Hide();
                SetPlayerInputEnabled(false);
                resultsScreen?.Show(
                    ScoreManager.Instance.RedScore,
                    ScoreManager.Instance.BlueScore);
                break;
        }
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        var localObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localObj == null) return;

        var controller = localObj.GetComponent<PlayerController>();
        var weapon = localObj.GetComponent<WeaponController>();
        if (controller != null) controller.enabled = enabled;
        if (weapon != null) weapon.enabled = enabled;
    }
}