using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort port = 7777;

    void Awake() { Instance = this; }
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    } 
    public void StartHost()
    {
        SetTransport(defaultIP);
        NetworkManager.Singleton.StartHost();
        
    }

    public void StartClient(string ip)
    {
        SetTransport(ip);
        NetworkManager.Singleton.StartClient();
    }

    private void SetTransport(string ip)
    {
        var transport = NetworkManager.Singleton
            .GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[CONNECTED] Client {clientId} connected. Total clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.LogWarning($"[DISCONNECTED] Client {clientId} disconnected.");
    }
}