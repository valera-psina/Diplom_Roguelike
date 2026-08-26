using UnityEngine;
using System.Collections;

public class RangedCellsEnemy : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private AudioClip shootSound;

    [Header("Teleport")]
    [SerializeField] private int maxShotsBeforeTeleport = 3;
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float effectDuration = 2f;
    private Transform[] teleportPoints;
    private int shotsFired = 0;
    private bool isTeleporting = false;

    private Transform target;
    private AudioSource audioSource;

    public bool PlayerInRange { get; private set; }
    public bool CanAttack => PlayerInRange && !isTeleporting;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) target = playerObj.transform;
        }

        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            RoomManager room = FindFirstObjectByType<RoomManager>();
            if (room != null)
                teleportPoints = room.GetSpawnPoints();
        }
    }

    public void SetTeleportPoints(Transform[] points)
    {
        teleportPoints = points;
    }

    private void Update()
    {
        if (target == null || isTeleporting) return;

        float dist = Vector3.Distance(transform.position, target.position);
        PlayerInRange = dist <= detectionRange;

        if (PlayerInRange)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || isTeleporting)
            return;

        Vector3 direction = target != null
            ? (target.position - firePoint.position).normalized
            : transform.forward;

        Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        shotsFired++;
        if (shotsFired >= maxShotsBeforeTeleport && !isTeleporting)
        {
            StartTeleport();
        }
    }

    private void StartTeleport()
    {
        isTeleporting = true;
        shotsFired = 0;
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Teleport");
    }

    public void PerformTeleport()
    {
        if (teleportEffectPrefab != null)
        {
            GameObject fx = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, effectDuration);
        }

        if (teleportPoints != null && teleportPoints.Length > 0)
        {
            int idx = Random.Range(0, teleportPoints.Length);
            transform.position = teleportPoints[idx].position;
        }

        if (teleportEffectPrefab != null)
        {
            GameObject fx = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, effectDuration);
        }

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);
    }

    public void FinishTeleport()
    {
        isTeleporting = false;
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.ResetTrigger("Teleport");
    }
}