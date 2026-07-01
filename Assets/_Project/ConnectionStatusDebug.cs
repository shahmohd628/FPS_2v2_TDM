// Quick debug script — put on any GameObject in Game scene
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ConnectionStatusDebug : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    void Update()
    {
        if (NetworkManager.Singleton == null) return;

        int count = NetworkManager.Singleton.ConnectedClientsList.Count;
        string role = NetworkManager.Singleton.IsHost ? "HOST" : "CLIENT";
        statusText.text = $"{role} | Connected: {count}";
    }
}