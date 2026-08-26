using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject barPanel;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private Health bossHealth;

    private void Start()
    {
        barPanel.SetActive(false);
        StartCoroutine(FindBoss());
    }

    private IEnumerator FindBoss()
    {
        while (true)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                bossHealth = boss.GetComponent<Health>();
                if (bossHealth != null)
                {
                    bossHealth.OnHealthChanged += UpdateBar;
                    bossHealth.OnDie += HideBar;
                    UpdateBar(bossHealth.CurrentHealth, bossHealth.MaxHealth);
                    if (bossNameText != null) bossNameText.text = boss.name;
                    barPanel.SetActive(true);
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateBar(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = current / max;
    }

    private void HideBar(GameObject killer)
    {
        barPanel.SetActive(false);
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateBar;
            bossHealth.OnDie -= HideBar;
        }
    }
}