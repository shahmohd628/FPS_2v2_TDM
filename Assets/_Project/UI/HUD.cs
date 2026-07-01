using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class HUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    [Header("Ammo")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Scores")]
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;

    private Health _playerHealth;
    private WeaponController _playerWeapon;
    private bool _hooked;

    void OnEnable()
    {
        TryHookLocalPlayer();
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScores;
    }

    void OnDisable()
    {
        if (_playerHealth != null) _playerHealth.OnHealthChanged -= UpdateHealth;
        if (_playerWeapon != null) _playerWeapon.OnAmmoChanged -= UpdateAmmo;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScores;
        _hooked = false;
    }

    void Update()
    {
        if (!_hooked) TryHookLocalPlayer();
    }

    void TryHookLocalPlayer()
    {
        var localObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localObj == null) return;

        _playerHealth = localObj.GetComponent<Health>();
        _playerWeapon = localObj.GetComponent<WeaponController>();

        if (_playerHealth == null || _playerWeapon == null) return;

        _playerHealth.OnHealthChanged += UpdateHealth;
        _playerWeapon.OnAmmoChanged += UpdateAmmo;

        UpdateHealth(_playerHealth.CurrentHealth);
        UpdateAmmo(_playerWeapon.CurrentMag, _playerWeapon.ReserveAmmo);
        _hooked = true;
    }

    void UpdateHealth(int hp)
    {
        if (healthBar != null) healthBar.value = hp / 100f;
        if (healthText != null) healthText.text = hp.ToString();
    }

    void UpdateAmmo(int mag, int reserve)
    {
        if (ammoText != null) ammoText.text = $"{mag} / {reserve}";
    }

    void UpdateScores(int red, int blue)
    {
        if (redScoreText != null) redScoreText.text = $"RED  {red}";
        if (blueScoreText != null) blueScoreText.text = $"BLUE  {blue}";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}