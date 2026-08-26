using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerUpgrades playerUpgrades;
    [SerializeField] private BuffSlot[] buffSlots;
    [SerializeField] private TextMeshProUGUI ammoDisplay;
    [SerializeField] private PlayerWeaponsManager weaponsManager;
    [SerializeField] private Image hitImage;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject pauseScreen;
    [System.Serializable]
    public class BuffSlot
    {
        public Image icon;
        public TextMeshProUGUI levelText;
    }

    private bool isPaused = false;
    private WeaponController currentWeapon;
    private MeleeAttack kickController;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (weaponsManager == null)
            weaponsManager = FindFirstObjectByType<PlayerWeaponsManager>();
        if (kickController == null)
            kickController = FindFirstObjectByType<MeleeAttack>();
        if (playerUpgrades == null)
            playerUpgrades = FindFirstObjectByType<PlayerUpgrades>();
        if (playerUpgrades != null)
        {
            playerUpgrades.OnUpgradesChanged += UpdateBuffSlots;
            UpdateBuffSlots();
        }
        if (weaponsManager != null)
        {
            weaponsManager.OnWeaponSwitched += UpdateWeapon;
            UpdateWeapon(weaponsManager.ActiveWeapon);
        }

        hitImage.enabled = false;
    }

    private void Update()
    {
        if (currentWeapon != null)
            ammoDisplay.text = $"{currentWeapon.CurrentAmmo}/{currentWeapon.MagazineSize}";
        else
            ammoDisplay.text = "";
    }

    private void UpdateBuffSlots()
    {
        if (playerUpgrades == null) return;
        var activeList = playerUpgrades.GetActiveUpgradesList();

        for (int i = 0; i < buffSlots.Length; i++)
        {
            if (i < activeList.Count)
            {
                buffSlots[i].icon.sprite = activeList[i].icon;
                buffSlots[i].icon.enabled = true;
                buffSlots[i].levelText.text = activeList[i].level.ToString();
                buffSlots[i].levelText.enabled = true;
            }
            else
            {
                buffSlots[i].icon.enabled = false;
                buffSlots[i].levelText.enabled = false;
            }
        }
    }

    private void UpdateWeapon(WeaponController newWeapon)
    {
        currentWeapon = newWeapon;
    }

    public void OnPause()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        isPaused = pauseScreen.activeSelf;
        if (!isPaused)
        {
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            kickController.canKick = false;
        }
        else
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            kickController.canKick = true;
        }
    }

    public void OnMenu()
    {
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ShowHitMarker()
    {
        if (hitImage != null)
            StartCoroutine(ShowHitMarkerCoroutine());
        if (audioSource != null && hitSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = hitSound;
            audioSource.Play();
        }
    }

    private IEnumerator ShowHitMarkerCoroutine()
    {
        hitImage.enabled = true;
        yield return new WaitForSeconds(0.1f);
        hitImage.enabled = false;
    }

    private void OnDestroy()
    {
        if (weaponsManager != null)
            weaponsManager.OnWeaponSwitched -= UpdateWeapon;
        if (playerUpgrades != null)
            playerUpgrades.OnUpgradesChanged -= UpdateBuffSlots;
    }
}