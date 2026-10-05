using UnityEngine;

public class PlayerJointManager : MonoBehaviour
{
    [Header("Available Slots")]
    [SerializeField] private JointSlot2D[] bodySlots;

    private void Awake()
    {
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
        // 1. Get the TRUE root of the incoming part, regardless of how deep the slot is nested
        Part incomingPart = targetSlot.GetComponentInParent<Part>();
        GameObject armObj = incomingPart.gameObject;

        // Get the Rigidbody2D of the specific part we're attaching to
        Rigidbody2D parentRb = slotToAttachTo.GetComponentInParent<Rigidbody2D>();

        // 2. Align position and rotation (This now safely moves the whole part)
        Vector3 offset = armObj.transform.position - targetSlot.transform.position;
        armObj.transform.position = slotToAttachTo.transform.position + offset;
        armObj.transform.rotation = slotToAttachTo.transform.rotation;

        // Grab the hinge directly from the Part object
        HingeJoint2D hinge = incomingPart.GetComponent< HingeJoint2D>();
        if (hinge == null) throw new System.Exception($"No hinge joint found on {armObj.name}!");

        hinge.enabled = false;

        // 3. Chain the hinge to the parent part's Rigidbody
        hinge.connectedBody = parentRb;
        hinge.autoConfigureConnectedAnchor = false;

        // Set anchors relative to their respective local transform spaces
        hinge.anchor = armObj.transform.InverseTransformPoint(targetSlot.transform.position);
        hinge.connectedAnchor = parentRb.transform.InverseTransformPoint(slotToAttachTo.transform.position);

        hinge.enabled = true;

        // Register attachment
        incomingPart.SetAttachmentConnection(targetSlot, slotToAttachTo);

        // Mark slots occupied
        slotToAttachTo.IsOccupied = true;
        targetSlot.IsOccupied = true;
        targetSlot.gameObject.SetActive(false);

        // 4. Inject the JointManager into the new part's unused slots
        JointSlot2D[] newSlots = armObj.GetComponentsInChildren< JointSlot2D>();
        foreach (var slot in newSlots)
        {
            if (!slot.IsOccupied)
            {
                slot.SetManager(this);
            }
        }

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