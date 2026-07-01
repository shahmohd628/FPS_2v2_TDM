using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ResultsScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text redScoreLabel;
    [SerializeField] private TMP_Text blueScoreLabel;

    public void Show(int redScore, int blueScore)
    {
        if (panel != null) panel.SetActive(true);
        gameObject.SetActive(true);

        string winner = redScore >= ScoreManager.KillsToWin
            ? "<color=#FF4444>RED TEAM WINS</color>"
            : "<color=#4488FF>BLUE TEAM WINS</color>";

        if (winnerText != null)    winnerText.text    = winner;
        if (redScoreLabel != null) redScoreLabel.text  = $"Red: {redScore}";
        if (blueScoreLabel != null) blueScoreLabel.text = $"Blue: {blueScore}";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void OnReturnToLobby()
    {
        // 1. Leave UGS session (cleans up Relay and Lobby internally)
        SessionManager.Instance?.LeaveSession();

        // 2. Shutdown NGO
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        // 3. Return to main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}