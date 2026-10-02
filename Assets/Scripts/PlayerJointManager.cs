using UnityEngine;

public class PlayerJointManager : MonoBehaviour
{
    [Header("Available Slots")]
    [SerializeField] private JointSlot2D[] bodySlots;

    private void Awake()
    {
        // Auto-find all JointSlot2D children on startup if not assigned
        if (bodySlots == null || bodySlots.Length == 0)
        {
            bodySlots = GetComponentsInChildren<JointSlot2D>();
        }
    }

    /// <summary>
    /// Attaches an arm prefab or existing instance to a matching slot.
    /// </summary>
    public bool AttachArm(JointSlot2D targetSlot, JointSlot2D slotToAttachTo, JointType targetType)
    {
        GameObject armObj =  targetSlot.gameObject;

        // Find the arm's corresponding joint anchor
        JointSlot2D armAnchor = armObj.GetComponentInChildren<JointSlot2D>();
        if (armAnchor == null)
        {
            Debug.LogError("The object being attached has no JointSlot2D component!");
            return false;
        }

        Rigidbody2D playerRb = GetComponent<Rigidbody2D>();
        Rigidbody2D armRb = armObj.GetComponent<Rigidbody2D>();

        // Align arm position so its anchor matches the body slot exactly
        Vector3 offset = armObj.transform.position - armAnchor.transform.position;
        armObj.transform.position = slotToAttachTo.transform.position + offset;
        armObj.transform.rotation = slotToAttachTo.transform.rotation;

        // Set up the HingeJoint2D on the arm
        HingeJoint2D hinge = armObj.GetComponentInParent<HingeJoint2D>();
        if (hinge == null)
        {
            throw new System.Exception($"No hinge joint was found on {armObj.name}'s parent!");
        }

        // Configure the joint
        hinge.connectedBody = playerRb;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.enabled = true;

        // Set local anchor points relative to each object
        hinge.anchor = armObj.transform.InverseTransformPoint(armAnchor.transform.position);
        hinge.connectedAnchor = transform.InverseTransformPoint(slotToAttachTo.transform.position);

        // Mark slot as filled
        slotToAttachTo.IsOccupied = true;
        targetSlot.IsOccupied = true;
        slotToAttachTo.gameObject.SetActive(false);

        return true;
    }

    private JointSlot2D FindUnoccupiedSlot(JointType type)
    {
        foreach (var slot in bodySlots)
        {
            if (!slot.IsOccupied && (slot.JointType == type || slot.JointType == JointType.Universal))
            {
                return slot;
            }
        }
        return null;
    }
}