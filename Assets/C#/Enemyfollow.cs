using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    private Transform player;
    public float speed = 2f;

    private bool isTouchingPlayer = false;
    private float damageTimer = 0f;
    public float damageInterval = 1f; // 每1秒扣一次血

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 跟随玩家
        Vector3 direction = player.position - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;

        // 如果贴着玩家，就持续扣血
        if (isTouchingPlayer)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                DealDamage();
                damageTimer = 0f;
            }
        }
    }

    void DealDamage()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isTouchingPlayer = true;
            damageTimer = 0f; // 刚碰到就重新计时
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }
}