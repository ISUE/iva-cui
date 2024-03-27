using System.Collections.Generic;
using UnityEngine;

namespace LLMAgents
{
    public enum AgentType
    {
        Friend, Clerk, Manager, None
    }

    public class AgentSelectionController : MonoBehaviour
    {
        public List<ActivationZone> zones;

        public static ActivationZone currentZone;

        private static List<ActivationZone> activeZones;

        private void Awake()
        {
            activeZones = new List<ActivationZone>();
            activeZones.AddRange(zones);
        }

        public static void PlayAudioForAgent(AgentType agentType, AudioClip audioClip)
        {
            foreach (ActivationZone zone in activeZones)
            {
                if (zone.GetZoneAgentType() == agentType)
                {
                    zone.PlayAudio(audioClip);
                    return;
                }
            }
        }

        public AgentType GetActiveAgentType()
        {
            // loop through all zones and return the first activated zone by string
            foreach (ActivationZone zone in zones)
                if (zone.GetIsActivated)
                    return zone.GetZoneAgentType();

            return AgentType.None;
        }
    }
}