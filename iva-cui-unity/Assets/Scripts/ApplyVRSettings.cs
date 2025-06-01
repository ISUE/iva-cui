using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ApplyVRSettings : MonoBehaviour
{
    private string questMicString = "Headset Microphone (Oculus Virtual Audio Device)";

    [SerializeField] private GameObject EyeTrackingObject;

    private XROrigin player;
    private Camera playerHead;

    private float referenceHeightM = 0.0f;
    private Vector3 currentHMDPosition;

    private void Start()
    {
        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin == null) { return; }

        if (xrOrigin.isActiveAndEnabled)
        {
            Debug.Log("Applying VR settings");
            ApplySettings();
        }

        player = FindObjectOfType<XROrigin>();
        playerHead = Camera.main;
    }

    private void ApplySettings()
    {
        MicrophoneHandler microphoneHandler = FindObjectOfType<MicrophoneHandler>();
        if (microphoneHandler == null)
        {
            Debug.LogError("MicrophoneHandler not found when applying VR Settings");
            return;
        }
        microphoneHandler.selectedMicString = questMicString;

        if (EyeTrackingObject != null)
        {
            EyeTrackingObject.SetActive(true);
        }
    }

    private void Update()
    {
        // Record the standing height on pressing 'T'
        if (Input.GetKeyDown(KeyCode.T))
        {
            referenceHeightM = playerHead.transform.position.y;
            Debug.Log($"Recorded Reference Height: {referenceHeightM}");
        }

        // Reset the origin on pressing 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            var dynamicMoveProvider = FindObjectOfType<DynamicMoveProvider>();
            if (dynamicMoveProvider == null)
            {
                Debug.LogError("IT'S OVER 9000!!! (DynamicMoveProvider not found)");
                return;
            }
            dynamicMoveProvider.useGravity = false;
            dynamicMoveProvider.gravityApplicationMode = DynamicMoveProvider.GravityApplicationMode.AttemptingMove;
            ResetOrigin();
        }
    }

    private void ResetOrigin()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        currentHMDPosition = playerHead.transform.position;
        float deltaY = currentHMDPosition.y - referenceHeightM;

        player.transform.position -= new Vector3(0, deltaY, 0);

        Debug.Log($"Player Position Adjusted: {player.transform.position}");
    }
}