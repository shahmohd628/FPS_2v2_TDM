using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;

public class UGSManager : MonoBehaviour
{
    public static UGSManager Instance { get; private set; }
    public bool IsSignedIn { get; private set; }

    async void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        await SignInAsync();
    }

    private async Task SignInAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            IsSignedIn = true;
            Debug.Log($"[UGS] Signed in. PlayerID: " +
                      $"{AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UGS] Sign-in failed: {e.Message}");
        }
    }

    // Call this from MainMenuUI before any session operations
    public async Task WaitForSignIn(float timeoutSeconds = 5f)
    {
        float elapsed = 0f;
        while (!IsSignedIn && elapsed < timeoutSeconds)
        {
            await Task.Delay(100);
            elapsed += 0.1f;
        }
    }
}