using UnityEngine;
using UnityEngine.Serialization;

public class EnemyFollow : MonoBehaviour
{
    [FormerlySerializedAs("speed")]
    public float moveSpeed = 2f;
    [FormerlySerializedAs("damageInterval")]
    public float contactDamageInterval = 1f;

    [SerializeField] private int contactDamage = 1;
    [SerializeField] private string playerTag = "Player";

    private Transform player;
    private bool isTouchingPlayer;
    private float damageTimer;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;

        if (!isTouchingPlayer)
        {
            return;
        }

        damageTimer += Time.deltaTime;
        if (damageTimer >= contactDamageInterval)
        {
            DealDamage();
            damageTimer = 0f;
        }
    }

    private void DealDamage()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        isTouchingPlayer = true;
        damageTimer = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouchingPlayer = false;
        }
    }
}
