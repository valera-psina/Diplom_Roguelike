using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Weapons/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Movement")]
    public float speed = 100f;
    public float maxLifeTime = 5f;
    public float sphereCastRadius = 0.05f;
    public LayerMask collisionMask;

    [Header("Damage & Force")]
    public float damage = 35f;
    public float impactForce = 10f;

    [Header("Effects")]
    public GameObject bulletHolePrefab;
    public float decalOffset = 0.01f;
    public float decalLifetime = 25f;
    public GameObject impactEffect;

    [Header("Ghost Damage")]
    public bool isGhostDamage = false;
}