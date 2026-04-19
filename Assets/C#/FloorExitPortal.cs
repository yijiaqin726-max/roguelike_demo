using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorExitPortal : MonoBehaviour
{
    private FloorFlowManager owner;
    private bool playerInside;
    private FloorExitPortalVisual visual;

    public void Initialize(FloorFlowManager floorFlowManager)
    {
        owner = floorFlowManager;
        visual = GetComponent<FloorExitPortalVisual>();
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
        visual?.SetHighlighted(true);
        owner?.SetPortalPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
        visual?.SetHighlighted(false);
        owner?.SetPortalPromptVisible(false);
    }
}
