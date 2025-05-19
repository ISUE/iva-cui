using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LLMAgents
{
    public enum AgentType
    {
        Agent1 = 0, Agent2 = 1, Agent3 = 2, None = 100
    }

    public class AgentSelectionController : MonoBehaviour
    {
        public static AgentSelectionController instance;

        public List<ActivationZone> zones;

        public List<AudioClip> agent1AudioClips;
        public List<AudioClip> agent2AudioClips;
        public List<AudioClip> agent3AudioClips;

        public static ActivationZone currentZone;
        public static ActivationZone lastZone;

        private void Awake()
        {
            instance = this;
        }

        public static void PlayAudioForAgent(AgentType agentType, AudioClip audioClip)
        {
            foreach (ActivationZone zone in instance.zones)
            {
                if (zone.GetZoneAgentType() == agentType)
                {
                    // Get the delay duration for the agent -- from instance of StudyControls
                    var timeAsOfMicInputEnd = SceneProfiling.speakEnd;
                    var timeNow = Time.time;
                    var targetDelayDurationInDistribution = TrainingSceneController.GetNormalRandom(StudyControls.delayDurations[StudyControls.instance.delayDuration], 0.23f);

                    var remainingDelay = targetDelayDurationInDistribution - (timeNow - timeAsOfMicInputEnd);

                    Debug.Log($"Target delay: {targetDelayDurationInDistribution}.");

                    if (remainingDelay < 0)
                    {
                        Debug.LogWarning($"Target delay: {targetDelayDurationInDistribution}. DELAY WAS NEGATIVE ({remainingDelay}) -> RESPONSE WAS LATE");
                        remainingDelay = 0;
                    }

                    // But if the delay condition is "One", remove all delay since existing delay already follows the wanted distribution
                    if (StudyControls.instance.delayDuration == StudyControls.DelayDuration.One)
                    {
                        remainingDelay = 0.0f;
                    }

                    instance.StartCoroutine(instance.PlayAgentResponseAfterDelay(zone, audioClip, remainingDelay));
                    instance.StartCoroutine(StudyTasks.SetAgentFinishedTalkingAfterSeconds(audioClip.length + remainingDelay));
                    return;
                }
            }
        }

        private IEnumerator PlayAgentResponseAfterDelay(ActivationZone zone, AudioClip audioClip, float remainingDelay)
        {
            yield return new WaitForSeconds(remainingDelay);

            SceneProfiling.ttsVoicePlayStart = Time.time;
            StudyControls.someoneIsThinking = false;
            if (StudyControls.USE_NEW_LOOKAWAY)
            {
                zone.LookAwayFromPlayerWhileThinking(false);
            }
            zone.LookAtPlayer(true);
            zone.PlayAudio(audioClip);
        }

        public AgentType GetActiveAgentType()
        {
            foreach (ActivationZone zone in zones)
                if (zone.GetIsActivated)
                    return zone.GetZoneAgentType();

            return AgentType.None;
        }

        public static bool SomeoneIsSpeaking()
        {
            foreach (ActivationZone zone in instance.zones)
                if (zone.AvatarCurrentlySpeaking)
                    return true;

            return false;
        }
    }
}