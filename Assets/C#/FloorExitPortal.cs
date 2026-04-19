using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorExitPortal : MonoBehaviour
{
    private FloorFlowManager owner;
    private bool playerInside;

    public void Initialize(FloorFlowManager floorFlowManager)
    {
        owner = floorFlowManager;
    }

    private void Update()
    {
        if (!playerInside)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            owner?.TryEnterExitPortal(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        owner?.SetPortalPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
        owner?.SetPortalPromptVisible(false);
    }
}
