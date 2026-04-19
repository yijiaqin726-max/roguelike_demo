using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorExitPortal : MonoBehaviour
{
    private FloorFlowManager owner;

    public void Initialize(FloorFlowManager floorFlowManager)
    {
        owner = floorFlowManager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        owner?.AdvanceToNextFloor();
    }
}
