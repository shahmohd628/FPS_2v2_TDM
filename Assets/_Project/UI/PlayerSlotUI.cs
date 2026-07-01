using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private TMP_Text emptyText;

    public void SetPlayer(string username, bool isHost)
    {
        if (emptyText != null) emptyText.gameObject.SetActive(false);
        usernameText.gameObject.SetActive(true);
        usernameText.text = isHost ? $"♛ {username} (Host)" : username;
        teamText.text = ""; // team shown after match starts
    }

    public void SetEmpty()
    {
        usernameText.gameObject.SetActive(false);
        teamText.text = "";
        if (emptyText != null) emptyText.gameObject.SetActive(true);
    }
}