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

    public AgentType GetZoneAgentType()
    {
        return agentType;
    }

    private void Start()
    {
        parentZone = transform.parent;
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("usercollider"))
            return;
        parentZone.GetComponent<Renderer>().material = standard;
        isActivated = false;
        AgentSelectionController.currentZone = null;
    }

    public void PlayAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
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