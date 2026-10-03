using UnityEngine;

public enum JointType { Shoulder, Elbow, Hand, Universal }

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

    [Header("Visual Effects")]
    [SerializeField] private JointParticleBeam particleBeam;

    private PlayerJointManager jointManager;
    private Vector3 worldPosAppliedForce, pullDirectionDebug;

    public JointType JointType => jointType;
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }

    private void Awake()
    {
        // Try finding the joint manager on parent/root object
        jointManager = GetComponentInParent<PlayerJointManager>();
        if (particleBeam == null) particleBeam = GetComponentInChildren<JointParticleBeam>();
    }

    private void FixedUpdate()
    {
        // Only active, unoccupied slots connected to the player chain perform magnet pulling
        if (!isMagnetActive || isOccupied || jointManager == null)
        {
            if (particleBeam != null) particleBeam.ClearBeam();
            return;
        }

        CheckAndPullNearbySlots();
    }

    private void CheckAndPullNearbySlots()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        bool pulledAnyTarget = false;

        foreach (Collider2D hit in hitColliders)
        {
            JointSlot2D nearbySlot = hit.GetComponent<JointSlot2D>() ?? hit.GetComponentInChildren<JointSlot2D>();

            if (nearbySlot == null || nearbySlot.IsOccupied || nearbySlot.transform.IsChildOf(transform.root))
                continue;

            if (nearbySlot.JointType != jointType && jointType != JointType.Universal && nearbySlot.JointType != JointType.Universal)
                continue;

            Rigidbody2D targetRb = nearbySlot.GetComponentInParent<Rigidbody2D>();
            if (targetRb == null) continue;

            Vector2 slotPosition = transform.position;
            Vector2 targetJointPos = nearbySlot.transform.position;
            Vector2 pullDirection = slotPosition - targetJointPos;
            float distance = pullDirection.magnitude;

            pulledAnyTarget = true;

            // Render particle beam between magnet slot and target slot
            if (particleBeam != null)
            {
                particleBeam.RenderBeam(transform.position, targetJointPos);
            }

            if (distance <= attachThreshold)
            {
                if (particleBeam != null) particleBeam.ClearBeam();
                jointManager.AttachArm(nearbySlot, this, jointType);
                Debug.Log($"{gameObject.name} attaching {nearbySlot.gameObject.name}", nearbySlot.gameObject);
                break;
            }
            else
            {
                Vector2 pullForce = pullDirection.normalized * pullSpeed;
                targetRb.AddForceAtPosition(pullForce, targetJointPos, ForceMode2D.Force);
                worldPosAppliedForce = targetJointPos;
                pullDirectionDebug = pullDirection;
            }
        }

        // If no targets were pulled this frame, clear existing particles
        if (!pulledAnyTarget && particleBeam != null)
        {
            particleBeam.ClearBeam();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOccupied ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        Gizmos.color = Color.orange;
        Gizmos.DrawSphere(worldPosAppliedForce, 0.3f);
        Gizmos.DrawLine(worldPosAppliedForce, worldPosAppliedForce + pullDirectionDebug.normalized);
    }
}