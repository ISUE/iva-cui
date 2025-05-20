using LLMAgents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class ActivationZone : MonoBehaviour
{
    [SerializeField] private AudioClip fillerSfxAudioClip;
    [SerializeField] private AudioSource fillerSfxAudioSource;

    private Transform parentZone;

    private bool isActivated = false;

    public bool GetIsActivated => isActivated;

    private Material standard;
    public Material activated;

    [SerializeField] private AgentType agentType;
    [SerializeField] private Transform avatarHead;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Animator animator;
    [SerializeField] private Canvas thinkingCanvas;

    private AimConstraint aimConstraint;

    private Transform lookAtTransform;
    private Vector3 lookAwayPosition;

    private bool playerInZone = false;
    private bool lookingAwayWhileThinking = false;
    private bool lastAngleWasBad = false;

    private float lookAwayDuration = .6f;
    private float lookAwayTimeElapsed = 0f;

    public bool AvatarCurrentlySpeaking => audioSource.isPlaying;

    public Transform GetAgentAvatar()
    {
        return avatarHead;
    }

    public AgentType GetZoneAgentType()
    {
        return agentType;
    }

    private void Start()
    {
        if (StudyControls.USE_NEW_LOOKAWAY)
        {
            lookAtTransform = GameObject.Find("LookAtMe").transform;
        }
        else
        {
            lookAtTransform = Camera.main.transform;
        }

        parentZone = transform.parent;
        SetThinkingStatus(StudyControls.instance.waitIndicatorType, false);

        // Set the 'standard' material variable to the current material
        standard = parentZone.GetComponent<Renderer>().material;

        // Add an aim constraint source
        avatarHead.TryGetComponent(out aimConstraint);
        if (aimConstraint == null)
        {
            Debug.LogWarning("No aim constraint found on agent head.");
        }
        else
        {
            var source = new ConstraintSource
            {
                sourceTransform = lookAtTransform,
                weight = 1,
            };
            aimConstraint.AddSource(source);
            aimConstraint.constraintActive = true;
            aimConstraint.weight = 0;
        }
    }

    private void Update()
    {
        if (StudyControls.USE_NEW_LOOKAWAY)
        {
            if (playerInZone)
            {
                if (lookingAwayWhileThinking)
                {
                    // do nothing
                }
                else
                {
                    lookAtTransform.position = Camera.main.transform.position;
                }
            }
        }
    }

    public void LookAwayFromPlayerWhileThinking(bool val)
    {
        lookingAwayWhileThinking = val;
        StartCoroutine(GraduallyLookAwayFromPlayerWhileThinking(val));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("usercollider"))
            return;
        parentZone.GetComponent<Renderer>().material = activated;
        isActivated = true;
        AgentSelectionController.currentZone = this;
        AgentSelectionController.lastZone = AgentSelectionController.currentZone;
        playerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("usercollider"))
            return;
        parentZone.GetComponent<Renderer>().material = standard;
        isActivated = false;
        AgentSelectionController.currentZone = null;
        playerInZone = false;
        LookAtPlayer(false);
    }

    public void MarkAsInteractedAtLeastOnce()
    {
        if (animator != null)
        {
            animator.SetBool("HasInteracted", true);
        }
    }

    public void SetIsListening()
    {
        animator?.SetBool("IsListening", true);
        if (StudyControls.USE_NEW_LOOKAWAY)
        {
            LookAtPlayer(true);
        }
    }

    public void SetThinkingStatus(StudyControls.WaitIndicatorType waitIndicatorType, bool val)
    {
        switch (waitIndicatorType)
        {
            case StudyControls.WaitIndicatorType.None:
                break;

            case StudyControls.WaitIndicatorType.Natural:
                if (animator != null)
                {
                    if (val)
                    {
                        int random = Random.Range(0, 3);
                        switch (random)
                        {
                            case 0:
                                animator?.SetBool("IsThinking1", val);
                                break;

                            case 1:
                                animator?.SetBool("IsThinking2", val);
                                break;

                            case 2:
                                animator?.SetBool("IsThinking3", val);
                                break;
                        }
                    }
                    else
                    {
                        animator?.SetBool("IsThinking1", val);
                        animator?.SetBool("IsThinking2", val);
                        animator?.SetBool("IsThinking3", val);
                    }
                }

                PlayFillerAudio(val);
                break;

            case StudyControls.WaitIndicatorType.Artificial:
                thinkingCanvas.transform.parent.gameObject.SetActive(val);
                PlayFillerSFX(val);
                break;
        }

        if (StudyControls.USE_NEW_LOOKAWAY)
        {
            if (val) LookAwayFromPlayerWhileThinking(val);
        }
    }

    private void PlayFillerSFX(bool val)
    {
        if (fillerSfxAudioSource == null)
        {
            Debug.LogError($"Filler SFX audio source is null for {agentType}.");
        }

        fillerSfxAudioSource.clip = fillerSfxAudioClip;
        if (val)
        {
            fillerSfxAudioSource.Play();
        }
        else
        {
            fillerSfxAudioSource.Stop();
        }
    }

    private void PlayFillerAudio(bool proceed)
    {
        if (!proceed) return;

        List<AudioClip> clips = null;

        switch (agentType)
        {
            case AgentType.Agent1:
                clips = AgentSelectionController.instance.agent1AudioClips;
                break;

            case AgentType.Agent2:
                clips = AgentSelectionController.instance.agent2AudioClips;
                break;

            case AgentType.Agent3:
                clips = AgentSelectionController.instance.agent3AudioClips;
                break;
        }

        if (clips is null) return;

        // If the condition is smallest delay, pick the clips that are shorter than 1 second
        if (StudyControls.instance.delayDuration == StudyControls.DelayDuration.One)
        {
            clips = clips.Where(clip => clip.length < 1).ToList();
        }

        audioSource.clip = clips[Random.Range(0, clips.Count)];
        audioSource.Play();
    }

    public void PlayAudio(AudioClip audioClip)
    {
        SetThinkingStatus(StudyControls.instance.waitIndicatorType, false);

        audioSource.clip = audioClip;
        audioSource.Play();

        float clipLength = audioClip.length;
        StartCoroutine(SetAgentBackToIdleAndLookAtPlayer(clipLength));
    }

    private IEnumerator SetAgentBackToIdleAndLookAtPlayer(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
    }

    public void LookAtPlayer(bool value)
    {
        if (StudyControls.USE_NEW_LOOKAWAY)
        {
            if (value && !playerInZone)
            {
                return;
            }
        }

        if (value && !playerInZone)
        {
            return;
        }

        StartCoroutine(GraduallyEnableLookAtAimConstraint(value));
    }

    private IEnumerator GraduallyEnableLookAtAimConstraint(bool look)
    {
        if (aimConstraint == null)
        {
            Debug.LogWarning("No aim constraint found on agent head.");
            yield break;
        }

        float duration = .6f;
        float timeElapsed = 0f;

        if (look)
        {
            if (aimConstraint.weight > 0.95f)
                yield break; // exit early if already looking at player

            while (timeElapsed < duration)
            {
                aimConstraint.weight = timeElapsed / duration;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            if (aimConstraint.weight < 0.05f)
                yield break; // exit early if already looking away from player

            while (timeElapsed < duration)
            {
                aimConstraint.weight = 1 - (timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator GraduallyLookAwayFromPlayerWhileThinking(bool val)
    {
        if (!playerInZone)
        {
            yield break;
        }

        // Gradually update the position of the lookAtObject.position to transition from the camera position to the decided-on look away position
        if (aimConstraint == null)
        {
            Debug.LogWarning("No aim constraint found on agent head.");
            yield break;
        }

        if (val)
        {
            Vector3 agentToCamera = Camera.main.transform.position - avatarHead.position;
            agentToCamera.Normalize();

            Vector3 orthogonalToAgentToCameraAndSceneUp = Vector3.Cross(agentToCamera, Vector3.up);
            orthogonalToAgentToCameraAndSceneUp.Normalize();

            print(orthogonalToAgentToCameraAndSceneUp);

            lookAwayPosition = Camera.main.transform.position + .5f * orthogonalToAgentToCameraAndSceneUp;
            lookAwayPosition.y -= .5f;
            print("lookaway pos" + lookAwayPosition);

            // Check if this is within the 60 degree cone of the agent body's forward direction
            // angle only around the y-axis is considered
            Transform agentBodyTransform = avatarHead.parent.parent.parent.parent.parent.parent.parent.parent.transform; // based on VALID body hierarchy
            print($"agent's body: {agentBodyTransform.name}");

            Vector3 agentBodyForward = agentBodyTransform.forward;
            agentBodyForward.Normalize();

            Vector3 agentToLookAway = lookAwayPosition - avatarHead.position;
            agentToLookAway.Normalize();

            float angle = Vector3.Angle(agentBodyForward, agentToLookAway);
            print("angle: " + angle);

            if (angle < 30f)
            {
                lastAngleWasBad = false;
                print("angle is less than 30 degrees -> enabling");
            }
            else
            {
                Debug.LogWarning("Angle is no good. Playing default animation");
                lastAngleWasBad = true;
                LookAtPlayer(false);
            }

            lookAwayTimeElapsed = 0f;

            while (lookAwayTimeElapsed < lookAwayDuration)
            {
                Vector3 camPos = Camera.main.transform.position;
                float lookAwayWeight = lookAwayTimeElapsed / lookAwayDuration;
                float lookAtCameraWeight = 1 - lookAwayWeight;

                Vector3 interpolatedLookAtPos = (camPos * lookAtCameraWeight + lookAwayPosition * lookAwayWeight);
                lookAtTransform.position = interpolatedLookAtPos;
                lookAwayTimeElapsed += Time.deltaTime;

                yield return null;
            }
        }
        else // transition the look away object back to looking at camera
        {
            if (lastAngleWasBad)
            {
                LookAtPlayer(true);
                yield break;
            }

            lookAwayTimeElapsed = 0f;
            while (lookAwayTimeElapsed < lookAwayDuration)
            {
                //lookAwayPosition
                Vector3 camPos = Camera.main.transform.position;
                float lookAtCameraWeight = lookAwayTimeElapsed / lookAwayDuration;
                float lookAwayWeight = 1 - lookAtCameraWeight;

                Vector3 interpolatedLookAtPos = (camPos * lookAtCameraWeight + lookAwayPosition * lookAwayWeight);
                lookAtTransform.position = interpolatedLookAtPos;
                lookAwayTimeElapsed += Time.deltaTime;

                yield return null;
            }
        }
    }
}