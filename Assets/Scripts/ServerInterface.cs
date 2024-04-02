using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LLMAgents;

public class ServerInterface : MonoBehaviour
{
    public string ip_colon_port = "127.0.0.1:8000";

    private void Awake()
    {
        StartCoroutine(SendRefreshRequest());
    }

    private IEnumerator SendRefreshRequest()
    {
        Debug.Log("Sending refresh request");
        string url = $"http://{ip_colon_port}/refresh/";

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

    public IEnumerator SendTextToSpeechRequest(AgentType agentType, string text)
    {
        string agentTypeString = agentType.ToString().ToLower();

        // URL encode the text
        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"http://{ip_colon_port}/speak_{agentTypeString}/?q={encodedText}";

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
                Debug.Log($"Received: {webRequest.downloadHandler.text}");
                (string audioPath, string message, string transition) = ExtractInfoFromResponse(webRequest.downloadHandler.text);
                //print(audioPath);
                string audioFileUrl = $"http://{ip_colon_port}/{audioPath}";
                //Debug.Log($"Message: {message}");
                print($"Transition: {transition}");
                StudyTaskUI.AddTask(transition);
                StartCoroutine(DownloadAndPlayAudio(agentType, audioFileUrl));
            }
        }
    }

    private IEnumerator DownloadAndPlayAudio(AgentType agent, string audioUrl)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                AgentSelectionController.PlayAudioForAgent(agent, clip);
            }
            else
            {
                Debug.LogError($"Failed to download audio clip: {www.error}");
            }
        }
        float timeAsOfAudioPlay = Time.time;
        print($"Total time since button press: {timeAsOfAudioPlay - MicrophoneWithTTS.timeAsOfButtonPress}");
    }

    [System.Serializable]
    public class SpeechResponse
    {
        public string message;
        public string audio;
        public string transition;
    }

    // Function to extract the audio URL and the message from the server response
    private (string audioPath, string message, string transition) ExtractInfoFromResponse(string response)
    {
        SpeechResponse speechResponse = JsonUtility.FromJson<SpeechResponse>(response);
        return (speechResponse.audio, speechResponse.message, speechResponse.transition);
    }
}