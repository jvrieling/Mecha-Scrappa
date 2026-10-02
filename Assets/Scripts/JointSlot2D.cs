using UnityEngine;

public enum JointType
{
    Shoulder,
    Elbow,
    Hand,
    Universal
}

public class JointSlot2D : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private JointType jointType = JointType.Shoulder;
    [SerializeField] private bool isOccupied = false;

    [Header("Magnet / Attraction Settings")]
    [Tooltip("Enable magnet effect on this slot (typically true for body slots).")]
    [SerializeField] private bool isMagnetActive = true;
    [Tooltip("Detection radius to start pulling nearby joint slots.")]
    [SerializeField] private float pullRadius = 1.0f;
    [Tooltip("Speed/force applied to drag the object toward this slot.")]
    [SerializeField] private float pullSpeed = 8.0f;
    [Tooltip("Distance threshold to trigger the auto-attachment.")]
    [SerializeField] private float attachThreshold = 0.15f;

    private PlayerJointManager jointManager;

    public JointType JointType => jointType;
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }

    private void Awake()
    {
        // Try finding the joint manager on parent/root object
        jointManager = GetComponentInParent<PlayerJointManager>();
    }

    private void FixedUpdate()
    {
        // Only active base slots on the player should pull external objects
        if (!isMagnetActive || isOccupied || jointManager == null) return;

        CheckAndPullNearbySlots();
    }

    private void CheckAndPullNearbySlots()
    {
        // Search for nearby colliders within the pull radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);

        foreach (Collider2D hit in hitColliders)
        {
            // Look for a JointSlot2D component on the detected collider or its children
            JointSlot2D nearbySlot = hit.GetComponent<JointSlot2D>() ?? hit.GetComponentInChildren<JointSlot2D>();

            // Skip if no slot found, if it's already attached/occupied, or if it belongs to our own body
            if (nearbySlot == null || nearbySlot.IsOccupied || nearbySlot.transform.IsChildOf(transform.root))
                continue;

            // Ensure the slot types match (or either is Universal)
            if (nearbySlot.JointType != jointType && jointType != JointType.Universal && nearbySlot.JointType != JointType.Universal)
                continue;

            Rigidbody2D targetRb = nearbySlot.GetComponentInParent<Rigidbody2D>();
            if (targetRb == null) continue;

            // Calculate direction and distance from the target's joint to this slot
            Vector2 slotPosition = transform.position;
            Vector2 targetJointPos = nearbySlot.transform.position;
            Vector2 pullDirection = slotPosition - targetJointPos;
            float distance = pullDirection.magnitude;

            if (distance <= attachThreshold)
            {
                // Trigger full attachment when close enough
                jointManager.AttachArm(nearbySlot, this, jointType);
                Debug.Log($"{gameObject.name} attaching {nearbySlot.gameObject.name}", nearbySlot.gameObject);
                continue;
            }
            else
            {
                // Apply a pulling force directly towards the slot position
                Vector2 pullForce = pullDirection.normalized * pullSpeed;
                targetRb.AddForceAtPosition(pullForce, targetJointPos, ForceMode2D.Force);
                worldPosAppliedForce = targetJointPos;
                pullDirectionDebug = pullDirection;
                Debug.Log($"Pulling on {nearbySlot.gameObject.name}", nearbySlot.gameObject);
            }
        }
    }

    Vector3 worldPosAppliedForce, pullDirectionDebug;

    private void OnDrawGizmosSelected()
    {
        // Visualize pull range in Scene View when selected
        Gizmos.color = isOccupied ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        Gizmos.color = Color.orange;
        Gizmos.DrawSphere(worldPosAppliedForce, 0.3f);
        Gizmos.DrawLine(worldPosAppliedForce, worldPosAppliedForce + pullDirectionDebug.normalized);
    }
}