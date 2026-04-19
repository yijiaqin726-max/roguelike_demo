using UnityEngine;
using System;

public class ExpOrb : MonoBehaviour
{
    public static event Action<GameObject, int> OrbCollected;

    public int expAmount = 1;
    public float magnetRadius = 2.4f;
    public float magnetAcceleration = 28f;
    public float maxMagnetSpeed = 10f;

    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private Vector3 velocity;

    void Start()
    {
        baseScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
    }

    void Update()
    {
        UpdateMagnet();

        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
        transform.localScale = baseScale * pulse;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.85f + Mathf.Sin(Time.time * 5f) * 0.15f;
            spriteRenderer.color = color;
        }
    }

    private void UpdateMagnet()
    {
        if (playerTarget == null)
        {
            return;
        }

        Vector3 toPlayer = playerTarget.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > magnetRadius)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 4f);
            return;
        }

        Vector3 desiredVelocity = toPlayer.normalized * maxMagnetSpeed;
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, magnetAcceleration * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExperience playerExp = other.GetComponent<PlayerExperience>();

            if (playerExp != null)
            {
                playerExp.GainExp(expAmount);
            }

            OrbCollected?.Invoke(other.gameObject, expAmount);
            PulseVisual.Spawn(transform.position, 0.9f, new Color(0.52f, 1f, 0.82f, 0.75f), 0.14f);

            Destroy(gameObject);
        }
    }
}
