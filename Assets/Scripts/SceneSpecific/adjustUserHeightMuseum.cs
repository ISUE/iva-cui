using Unity.XR.CoreUtils;
using UnityEngine;

public class adjustUserHeightMuseum : MonoBehaviour
{
    [SerializeField] private float heightAdjustment = 0.128f;
    private XROrigin player;
    private bool heightAdjusted = false;

    private void Start()
    {
        player = FindObjectOfType<XROrigin>();
        if (player == null)
        {
            Debug.LogWarning("XROrigin not found. If you're trying to run this in VR mode, make sure the XROrigin is active in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("usercollider") && !heightAdjusted)
        {
            AdjustHeight(heightAdjustment);
            heightAdjusted = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("usercollider") && heightAdjusted)
        {
            AdjustHeight(-heightAdjustment);
            heightAdjusted = false;
        }
    }

    private void AdjustHeight(float adjustment)
    {
        if (player != null)
        {
            player.transform.position += new Vector3(0, adjustment, 0);
            Debug.Log($"Height adjusted by {adjustment * 100} cm. New position: {player.transform.position}");
        }
    }
}