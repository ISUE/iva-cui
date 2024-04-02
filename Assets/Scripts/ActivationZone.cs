using LLMAgents;
using System.Collections;
using UnityEngine;

public class ActivationZone : MonoBehaviour
{
    private Transform parentZone;

    private bool isActivated = false;

    public bool GetIsActivated => isActivated;

    public Material standard;
    public Material activated;

    [SerializeField] private AgentType agentType;
    [SerializeField] private Transform agentAvatar;
    [SerializeField] private AudioSource audioSource;
    public FourStageAvatarStatusIndicator avatarStatusIndicator;

    [SerializeField] private Canvas thinkingCanvas;

    public Transform GetAgentAvatar()
    {
        return agentAvatar;
    }

    public AgentType GetZoneAgentType()
    {
        return agentType;
    }

    private void Start()
    {
        parentZone = transform.parent;
        avatarStatusIndicator?.SetStatus(FourStageAvatarStatusIndicator.Status.Idle);
        SetThinkingStatus(false);
    }

    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("usercollider"))
            return;
        parentZone.GetComponent<Renderer>().material = activated;
        isActivated = true;
        AgentSelectionController.currentZone = this;
        LookAtPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("usercollider"))
            return;
        parentZone.GetComponent<Renderer>().material = standard;
        isActivated = false;
        AgentSelectionController.currentZone = null;
    }

    public void SetThinkingStatus(bool val)
    {
        thinkingCanvas.gameObject.SetActive(val);
    }

    public void PlayAudio(AudioClip audioClip, string transition)
    {
        audioSource.clip = audioClip;
        audioSource.Play(); 

        SetThinkingStatus(false);
        avatarStatusIndicator?.SetStatus(FourStageAvatarStatusIndicator.Status.Speaking);
        LookAtPlayer();

        float clipLength = audioClip.length;
        StartCoroutine(SetAgentBackToIdleAndLookAtPlayer(clipLength, transition));
    }

    private IEnumerator SetAgentBackToIdleAndLookAtPlayer(float clipLength, string transition)
    {
        yield return new WaitForSeconds(clipLength);
        UserStudyControls.latestInteractionData.timeOfFeedbackFinish = Time.time;
        UserStudyControls.StopTrackingAverageAngle("Speaking");
        UserStudyControls.SaveInteractionData();
        StudyTaskUI.AddTask(transition);
        LookAtPlayer();
        avatarStatusIndicator?.SetStatus(FourStageAvatarStatusIndicator.Status.Idle);
    }

    public void LookAtPlayer()
    {
        StartCoroutine(LookAtPlayerCoroutine());
    }

    private IEnumerator LookAtPlayerCoroutine()
    {
        float duration = .5f;
        float timeElapsed = 0f;

        Vector3 avatarPosition = agentAvatar.position;
        Quaternion avatarRotation = agentAvatar.rotation;
        Vector3 playerPosition = Camera.main.transform.position;
        Vector3 direction = playerPosition - avatarPosition;

        Quaternion toRotation = Quaternion.LookRotation(direction, transform.up);

        while (timeElapsed < duration)
        {
            agentAvatar.rotation = Quaternion.Slerp(avatarRotation, toRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}