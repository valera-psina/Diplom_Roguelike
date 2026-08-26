using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private AudioClip hitSound;
    private AudioSource audioSource;

    private Dictionary<Health, float> lastDamageTimes = new Dictionary<Health, float>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;

            float currentTime = Time.time;

            if (!lastDamageTimes.ContainsKey(targetHealth) ||
                currentTime - lastDamageTimes[targetHealth] >= damageInterval)
            {
                targetHealth.TakeDamage(damage, gameObject);
                lastDamageTimes[targetHealth] = currentTime;

                if (hitSound == null) return;
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
                lastDamageTimes.Remove(targetHealth);
        }
    }
}