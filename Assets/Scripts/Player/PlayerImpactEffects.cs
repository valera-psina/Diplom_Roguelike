using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class PlayerImpactEffects : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private Image vignetteImage;
    [SerializeField] private float flashDuration = 0.2f;

    private Health health;
    private float flashTimer;

    private void Awake() => health = GetComponent<Health>();
    private void OnEnable() => health.OnDamage += OnDamageTaken;
    private void OnDisable() => health.OnDamage -= OnDamageTaken;

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float alpha = Mathf.Lerp(0, 0.5f, flashTimer / flashDuration);
            vignetteImage.color = new Color(0.35f, 0, 0, alpha);
        }
        else if (vignetteImage.color.a > 0)
        {
            vignetteImage.color = new Color(0.35f, 0, 0, 0);
        }
    }

    private void OnDamageTaken(float damage, GameObject source)
    {
        if (impulseSource != null) impulseSource.GenerateImpulse();
        flashTimer = flashDuration;
    }
}