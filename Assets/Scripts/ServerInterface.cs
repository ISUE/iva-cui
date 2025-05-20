using LLMAgents;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ServerInterface : MonoBehaviour
{
    public string hostIpPort = "127.0.0.1:8000";
    public string whisperIpPort = "http://127.0.0.1:8083/transcribe_audio/";

    public static ServerInterface instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (refreshAtStart)
        {
            StartCoroutine(SendRefreshRequest(StudyControls.GetUserStudySceneName()));
        }
    }

    [SerializeField] private bool refreshAtStart = true;

    public static void RefreshConversation()
    {
        instance.StartCoroutine(instance.SendRefreshRequest(StudyControls.GetUserStudySceneName()));
    }

    public static void RefreshTrainingConversation()
    {
        instance.StartCoroutine(instance.SendRefreshRequest("Training"));
    }

    private IEnumerator SendRefreshRequest(string sceneToRefresh)
    {
        Debug.Log("Sending refresh request for " + sceneToRefresh);
        string url = $"http://{hostIpPort}/refresh/{sceneToRefresh}/";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error refreshing message history: {webRequest.error}");
            }
            else
            {
                Debug.Log($"Server response: {webRequest.downloadHandler.text}");
            }
        }
    }

    public IEnumerator UploadAudioBytes(byte[] audioBytes, Action<string> callback)
    {
        SceneProfiling.asrStart = Time.time;

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioBytes, "temp.wav", "audio/wav");

        UnityWebRequest www = UnityWebRequest.Post(whisperIpPort, form);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            string text = ExtractTranscriptiontFromResponse(www.downloadHandler.text);
            SceneProfiling.asrEnd = Time.time;
            callback?.Invoke(text);
        }
    }

    [System.Serializable]
    public class WhisperResponse
    {
        public string transcription;
    }

    private string ExtractTranscriptiontFromResponse(string response)
    {
        WhisperResponse whisperResponse = JsonUtility.FromJson<WhisperResponse>(response);
        return whisperResponse.transcription;
    }

    public IEnumerator SendTextToSpeechRequest(AgentType agentType, string text)
    {
        SceneProfiling.ttsReqStart = Time.time;
        string agentTypeString = agentType.ToString().ToLower();

        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"http://{hostIpPort}/speak/{agentTypeString}/?q={encodedText}";

        print($"Sending a request to middleware server");

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Error: {webRequest.error}");
            }
            else
            {
                SceneProfiling.ttsReqEnd = Time.time;
                Debug.Log($"Received: {webRequest.downloadHandler.text}");
                SpeechResponse speechResponse = ExtractInfoFromResponse(webRequest.downloadHandler.text);

                ConversationLogger.LogAgentMessage(agentType, speechResponse.message);

                string audioFileUrl = $"http://{hostIpPort}/{speechResponse.audio}";

                StartCoroutine(DownloadAndPlayAudio(agentType, audioFileUrl, speechResponse));
            }
        }
    }

    private IEnumerator DownloadAndPlayAudio(AgentType agent, string audioUrl, SpeechResponse speechResponse)
    {
        SceneProfiling.ttsVoiceDownloadStart = Time.time;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                AgentSelectionController.PlayAudioForAgent(agent, clip, speechResponse);

                StartCoroutine(SendTransitionCheckRequest(agent));

                SceneProfiling.ComputeTimes(speechResponse);
            }
            else
            {
                Debug.LogError($"Failed to download audio clip: {www.error}");
            }
        }
    }

    [System.Serializable]
    public class SpeechResponse
    {
        public string message;
        public string audio;
        public string transition;

        // Fields arriving in JSON response
        public string llm_client_name;

        public int user_input_word_count;
        public int response_word_count;
        public int transition_length;
        public float llm_generation_time;
        public float speech_generation_time;

        public override string ToString()
        {
            return $"Message: {message}\n" +
                   $"Audio: {audio}\n" +
                   $"Transition: {transition}\n" +
                   $"LLM Client Name: {llm_client_name}\n" +
                   $"User Input Word Count: {user_input_word_count}\n" +
                   $"Response Word Count: {response_word_count}\n" +
                   $"Transition Length: {transition_length}\n" +
                   $"LLM Generation Time: {llm_generation_time}\n" +
                   $"Speech Generation Time: {speech_generation_time}";
        }
    }

    private SpeechResponse ExtractInfoFromResponse(string response)
    {
        return JsonUtility.FromJson<SpeechResponse>(response);
    }

    public IEnumerator SendTransitionCheckRequest(AgentType agentType)
    {
        string agentTypeString = agentType.ToString().ToLower();

        string url = $"http://{hostIpPort}/check_transition/{agentTypeString}/";

        Debug.Log($"Sending transition check for {agentTypeString} via {url}");

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Error: {webRequest.error}");
            }
            else
            {
                TransitionCheckResponse transitionResponse = ExtractTransitionCheckResponse(webRequest.downloadHandler.text);
                var transition = transitionResponse.transition;
                Debug.Log($">> Transition response: {transition}");

                StudyTasks.agentFinishedTalking = false;
                StartCoroutine(StudyTasks.HandleLLMDeterminedTask(transition));
            }
        }
    }

    [System.Serializable]
    public class TransitionCheckResponse
    {
        public string role;
        public string transition;
    }

    private TransitionCheckResponse ExtractTransitionCheckResponse(string response)
    {
        return JsonUtility.FromJson<TransitionCheckResponse>(response);
    }
}