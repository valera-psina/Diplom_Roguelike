using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject rightLeg;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask searchLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private Transform overlapStartPoint;
    [SerializeField] private Vector3 offset;
    [SerializeField, Min(0f)] private float sphereRadius;
    [SerializeField] private bool considerObstacles;
    [SerializeField] private AudioClip kickSound;
    [SerializeField] private AudioClip whooshSound;
    [SerializeField] private float cooldown = 0.1f;

    private readonly Collider[] overlapResults = new Collider[1];
    private int overlapResultCount;
    private PlayerUI cachedPlayerUI;
    private CinemachineImpulseSource impulseSource;
    private AudioSource audioSource;
    private PlayerWeaponsManager playerWeaponsManager;

    [HideInInspector] public bool canKick = true;

    void Start()
    {
        cachedPlayerUI = FindFirstObjectByType<PlayerUI>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        audioSource = GetComponent<AudioSource>();
        playerWeaponsManager = FindFirstObjectByType<PlayerWeaponsManager>();
        rightLeg.SetActive(false);
    }

    private void OnKick()
    {
        if (!canKick) return;
        StartCoroutine(DelayKick());
    }

    private IEnumerator DelayKick()
    {
        if (!canKick) yield break;
        canKick = false;

        playerWeaponsManager?.StartMeleeBlock();

        rightLeg.SetActive(true);

        if (animator != null)
            animator.SetTrigger("Kick");
        if (whooshSound != null) audioSource.PlayOneShot(whooshSound);

        yield return new WaitForSeconds(0.2f);

        if (TryFindEnemies())
        {
            TryAttackEnemies();
            CameraShake();
            if (kickSound != null) audioSource.PlayOneShot(kickSound);
        }

        yield return new WaitForSeconds(0.2f);
        playerWeaponsManager?.EndMeleeBlock();

        rightLeg.SetActive(false);

        yield return new WaitForSeconds(cooldown);
        canKick = true;
    }

    private bool TryFindEnemies()
    {
        var position = overlapStartPoint.TransformPoint(offset);
        overlapResultCount = Physics.OverlapSphereNonAlloc(position, sphereRadius, overlapResults, searchLayerMask.value);
        return overlapResultCount > 0;
    }

    private void TryAttackEnemies()
    {
        for (int i = 0; i < overlapResultCount; i++)
        {
            Collider target = overlapResults[i];
            if (target == null || target.gameObject == gameObject)
                continue;

            if (considerObstacles)
            {
                Vector3 startPos = overlapStartPoint.position;
                Vector3 targetPos = target.bounds.center;
                if (Physics.Linecast(startPos, targetPos, obstacleLayerMask))
                    continue;
            }

            if (target.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(damage, gameObject);
                if (cachedPlayerUI != null)
                    cachedPlayerUI.ShowHitMarker();
            }
        }
    }

    private void CameraShake()
    {
        if (impulseSource == null) return;
        impulseSource.GenerateImpulseWithForce(1f);
    }
}