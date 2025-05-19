using LLMAgents;
using UnityEngine;

public class SurveyPopupTrigger : MonoBehaviour
{
    [SerializeField] private AgentType agentType;

    private void Start()
    {
        // Check if the name of the object is the same as the agent type
        if (!name.Contains(agentType.ToString()))
        {
            Debug.LogError("Agent type and object name do not match.");
        }
    }
}