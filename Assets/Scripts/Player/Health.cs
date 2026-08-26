using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float deathDisableDelay = 1f;
    [SerializeField] private bool allowVampirism = true;
    [SerializeField] private bool isGhost = false;
    public bool IsGhost => isGhost;
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool isInvincible { get; set; }
    public bool IsDead => m_IsDead;
    public float GetRatio() => CurrentHealth / maxHealth;
    public bool CanPickup() => CurrentHealth < maxHealth;

    public event Action<float, GameObject> OnDamage;
    public event Action<float> OnHeal;
    public event Action<GameObject> OnDie;
    public event Action<float, float> OnHealthChanged;

    private bool m_IsDead;
    private float baseMaxHealth;
    private bool baseInitialized;
    private GameObject lastDotSource;
    private Coroutine currentDotCoroutine;

    private void Awake()
    {
        if (!baseInitialized)
        {
            baseMaxHealth = maxHealth;
            baseInitialized = true;
        }

        CurrentHealth = maxHealth;
        OnHealthChanged += UpdateHealthInfo;
        UpdateHealthInfo(CurrentHealth, maxHealth);
    }

    private void UpdateHealthInfo(float current, float max)
    {
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(current)}/{max}";
        if (healthBar != null)
            healthBar.fillAmount = current / max;
    }

    public void TakeDamage(float damage, GameObject damageSource)
    {
        if (isInvincible || m_IsDead) return;
        float healthBefore = CurrentHealth;
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        float trueDamage = healthBefore - CurrentHealth;
        if (trueDamage > 0f)
            OnDamage?.Invoke(trueDamage, damageSource);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        HandleDeath(damageSource);
    }

    public bool Heal(float healAmount)
    {
        if (m_IsDead) return false;
        float healthBefore = CurrentHealth;
        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        float trueHeal = CurrentHealth - healthBefore;
        if (trueHeal > 0f)
        {
            OnHeal?.Invoke(trueHeal);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            return true;
        }
        return false;
    }

    public void Kill(GameObject killer = null)
    {
        if (m_IsDead) return;
        CurrentHealth = 0f;
        OnDamage?.Invoke(maxHealth, killer);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        HandleDeath(killer);
    }

    private void HandleDeath(GameObject killer)
    {
        if (m_IsDead) return;
        if (CurrentHealth <= 0f)
        {
            m_IsDead = true;
            OnDie?.Invoke(killer);
            if (allowVampirism && CompareTag("Enemy"))
                GameManager.RaiseEnemyDied(gameObject, killer);
            if (!CompareTag("Player"))
                StartCoroutine(DisableAfterDelay());
        }
    }

    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(deathDisableDelay);
        gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        baseMaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        m_IsDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetHealth(float current, float max)
    {
        maxHealth = max;
        CurrentHealth = Mathf.Clamp(current, 0f, maxHealth);
        m_IsDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetMaxHealthMultiplier(float mult)
    {
        maxHealth = baseMaxHealth * mult;
        if (CurrentHealth > maxHealth) CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ApplyDot(float damagePerTick, float tickInterval, float duration, GameObject source)
    {
        if (m_IsDead) return;
        if (currentDotCoroutine != null) StopCoroutine(currentDotCoroutine);
        currentDotCoroutine = StartCoroutine(DotCoroutine(damagePerTick, tickInterval, duration, source));
    }

    private IEnumerator DotCoroutine(float damagePerTick, float tickInterval, float duration, GameObject source)
    {
        lastDotSource = source;
        float elapsed = 0f;
        while (elapsed < duration && !m_IsDead && gameObject != null && gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            TakeDamage(damagePerTick, source);
        }
        currentDotCoroutine = null;
    }

    public void SetCurrentHealth(float current)
    {
        CurrentHealth = Mathf.Clamp(current, 0f, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}