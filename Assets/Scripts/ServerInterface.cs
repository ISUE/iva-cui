using System;
using System.Collections;
using LLMAgents;
using UnityEngine;
using UnityEngine.Networking;

public class ServerInterface : MonoBehaviour
{
    public string ip_colon_port = "127.0.0.1:8000";
    public string whisper_ip_colon_port = "http://45.56.116.241:8083/upload/";

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
        string url = $"http://{ip_colon_port}/refresh/{sceneToRefresh}/";

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

        UnityWebRequest www = UnityWebRequest.Post(whisper_ip_colon_port, form);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            //print($"Time taken to upload: {Time.time - recordingStopTime}");

            // extract text using serialization
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

    // function to extract the text from the server response
    private string ExtractTranscriptiontFromResponse(string response)
    {
        WhisperResponse whisperResponse = JsonUtility.FromJson<WhisperResponse>(response);
        return whisperResponse.transcription;
    }

    public IEnumerator SendTextToSpeechRequest(AgentType agentType, string text)
    {
        SceneProfiling.ttsReqStart = Time.time;
        string agentTypeString = agentType.ToString().ToLower();

        // URL encode the text
        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"http://{ip_colon_port}/speak/{agentTypeString}/?q={encodedText}";

        print($"Sending a request to middleware server");

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Error: {webRequest.error}");
                // TODO FIXME reset the status back to idle
            }
            else
            {
                SceneProfiling.ttsReqEnd = Time.time;
                Debug.Log($"Received: {webRequest.downloadHandler.text}");
                SpeechResponse speechResponse = ExtractInfoFromResponse(webRequest.downloadHandler.text);

                ConversationLogger.LogAgentMessage(agentType, speechResponse.message);

                //print(speechResponse);
                string audioFileUrl = $"http://{ip_colon_port}/{speechResponse.audio}";

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
                AgentSelectionController.PlayAudioForAgent(agent, clip);

                if (ProfileLocalDelays.isProfiling) // mark response arrived only if profiling
                {
                    ProfileLocalDelays.responseArrived = true;
                }

                if (!ProfileLocalDelays.isProfiling) // only request transition if not profiling
                {
                    StartCoroutine(SendTransitionCheckRequest(agent));
                }

                SceneProfiling.ComputeTimes(speechResponse);
                SceneProfiling.LogLatencyDetails(agent, speechResponse);
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

        /* We also have the following fields in the response JSON:
         * logging_info = {
            "llm_client_name": LLM_CLIENT_NAME,
            "user_input_word_count": len(text.split()),
            "response_word_count": len(llm_response.split()),
            "transition_length": len(next_user_task.split()),
            "llm_generation_time": f"{_llm_processing_duration:.3f}",
            "speech_generation_time": f"{_speech_generation_duration:.3f}"}
        */
        public string llm_client_name;
        public int user_input_word_count;
        public int response_word_count;
        public int transition_length;
        public float llm_generation_time;
        public float speech_generation_time;

        public override string ToString()
        {
            // Separate each field with a newline
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

    // Function to extract the audio URL and the message from the server response
    private SpeechResponse ExtractInfoFromResponse(string response)
    {
        return JsonUtility.FromJson<SpeechResponse>(response);
    }

    public IEnumerator SendTransitionCheckRequest(AgentType agentType)
    {
        string agentTypeString = agentType.ToString().ToLower();

        string url = $"http://{ip_colon_port}/check_transition/{agentTypeString}/";

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