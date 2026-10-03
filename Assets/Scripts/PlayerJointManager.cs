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
        GameObject armObj = targetSlot.transform.parent.gameObject;
        Rigidbody2D playerRb = GetComponent<Rigidbody2D>();

        // 1. Align position and rotation first
        Vector3 offset = armObj.transform.position - targetSlot.transform.position;
        armObj.transform.position = slotToAttachTo.transform.position + offset;
        armObj.transform.rotation = slotToAttachTo.transform.rotation;

        // Get HingeJoint2D
        HingeJoint2D hinge = armObj.GetComponentInParent<HingeJoint2D>();
        if (hinge == null)
        {
            throw new System.Exception($"No hinge joint was found on {armObj.name}'s parent!");
        }

        // 2. Disable joint temporarily to reset reference frame calculations
        hinge.enabled = false;

        // 3. Configure connected body and anchors
        hinge.connectedBody = playerRb;
        hinge.autoConfigureConnectedAnchor = false;

        // Set local anchors relative to each object's transform space
        hinge.anchor = armObj.transform.InverseTransformPoint(targetSlot.transform.position);
        hinge.connectedAnchor = transform.InverseTransformPoint(slotToAttachTo.transform.position);

        // 4. Re-enable the joint so Unity computes reference limits from the current orientation
        hinge.enabled = true;

        // Register attachment in Part component if present
        Part part = armObj.GetComponentInParent<Part>();
        if (part != null)
        {
            part.SetAttachmentConnection(targetSlot, slotToAttachTo);
        }

        // Mark slots occupied
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