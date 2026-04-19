using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    [Header("Attack Settings")]
    public GameObject bulletPrefab;
    public float attackInterval = 1f;
    public float attackPoseDuration = 0.12f;
    public Sprite holdSprite;
    public Sprite gunSprite;

    private float attackTimer;
    private float attackPoseTimer;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && holdSprite != null)
        {
            spriteRenderer.sprite = holdSprite;
        }
    }

    void Update()
    {
        MovePlayer();
        AutoAttack();
        UpdateAttackPose();
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, moveY, 0);
        transform.position += move * speed * Time.deltaTime;
    }

    void AutoAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Transform nearestEnemy = FindNearestEnemy();

            if (nearestEnemy != null)
            {
                GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                bulletScript.SetTarget(nearestEnemy);
                TriggerAttackPose();
            }
        }
    }

    void TriggerAttackPose()
    {
        attackPoseTimer = attackPoseDuration;

        if (spriteRenderer != null && gunSprite != null)
        {
            spriteRenderer.sprite = gunSprite;
        }
    }

    void UpdateAttackPose()
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

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
