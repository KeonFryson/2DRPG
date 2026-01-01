using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
   private TrailRenderer trailRenderer;

    void Start()
    {
        // Don't look for trail renderer yet since the child might not be added
    }

    void Update()
    {

    }

    // Call this method after a child weapon is added to activate its trail
    public void ActivateTrail()
    {
        // Find the trail renderer on the child when activating
        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.Clear(); // Clear any existing trail
        }
    }

    // Call this to deactivate the trail
    public void DeactivateTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
    }

    // Call this when a new weapon child is added
    public void OnWeaponEquipped()
    {
        // Refresh the trail renderer reference for the new child
        trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false; // Start disabled
        }
    }
}
