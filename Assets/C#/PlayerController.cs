using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(DarkPulseController))]
public class PlayerController : MonoBehaviour
{
    [FormerlySerializedAs("speed")]
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    public GameObject bulletPrefab;
    [FormerlySerializedAs("attackInterval")]
    public float attackIntervalSeconds = 1f;
    public float attackIntervalMultiplier = 1f;
    public float attackPoseDuration = 0.12f;
    public float minimumAttackInterval = 0.05f;
    public Sprite holdSprite;
    public Sprite gunSprite;

    [Header("Targeting")]
    [SerializeField] private string enemyTag = "Enemy";

    private float attackTimer;
    private float attackPoseTimer;
    private SpriteRenderer spriteRenderer;
    private DarkPulseController darkPulseController;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        darkPulseController = GetComponent<DarkPulseController>();

        if (spriteRenderer != null && holdSprite != null)
        {
            spriteRenderer.sprite = holdSprite;
        }
    }

    private void Update()
    {
        MovePlayer();
        AutoAttack();
        UpdateAttackPose();
    }

    private void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, moveY, 0f);
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void AutoAttack()
    {
        attackTimer += Time.deltaTime;
        float currentAttackInterval = Mathf.Max(minimumAttackInterval, attackIntervalSeconds * attackIntervalMultiplier);

        if (attackTimer < currentAttackInterval)
        {
            return;
        }

        attackTimer = 0f;
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null || bulletPrefab == null)
        {
            return;
        }

        GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetTarget(nearestEnemy);
        }

        if (darkPulseController != null)
        {
            darkPulseController.TryTriggerDarkPulse();
        }

        TriggerAttackPose();
    }

    private void TriggerAttackPose()
    {
        attackPoseTimer = attackPoseDuration;

        if (spriteRenderer != null && gunSprite != null)
        {
            spriteRenderer.sprite = gunSprite;
        }
    }

    private void UpdateAttackPose()
    {
        if (attackPoseTimer <= 0f)
        {
            return;
        }

        attackPoseTimer -= Time.deltaTime;

        if (attackPoseTimer <= 0f && spriteRenderer != null && holdSprite != null)
        {
            spriteRenderer.sprite = holdSprite;
        }
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}
