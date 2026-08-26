using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        deathPanel.SetActive(false);

        mainMenuButton.onClick.AddListener(OnMainMenu);

        StartCoroutine(SubscribeToPlayer());
    }

    private IEnumerator SubscribeToPlayer()
    {
        while (GameManager.Instance.CurrentPlayer == null)
            yield return null;

        Health playerHealth = GameManager.Instance.CurrentPlayer.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.OnDie += OnPlayerDeath;
    }

    private void OnPlayerDeath(GameObject killer)
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
        {
            var health = GameManager.Instance.CurrentPlayer.GetComponent<Health>();
            if (health != null) health.OnDie -= OnPlayerDeath;
        }
    }
}