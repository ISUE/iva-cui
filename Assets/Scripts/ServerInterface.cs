using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ServerInterface : MonoBehaviour
{
    public string ip_colon_port = "127.0.0.1:8000";

    public AudioSource friend_audioSource;
    public AudioSource clerk_audioSource;
    public AudioSource manager_audioSource;

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

    public IEnumerator SendTextToSpeechRequest(string agentType, string text)
    {
        // URL encode the text
        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"http://{ip_colon_port}/speak_{agentType}/?q={encodedText}";

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
                (string audioPath, string message) = ExtractInfoFromResponse(webRequest.downloadHandler.text);
                //print(audioPath);
                string audioFileUrl = $"http://{ip_colon_port}/{audioPath}";
                Debug.Log($"Message: {message}"); // Log or display the message as needed
                StartCoroutine(DownloadAndPlayAudio(agentType, audioFileUrl));
            }
        }
    }

    private IEnumerator DownloadAndPlayAudio(string agent, string audioUrl)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                
                // Determine audio source based on agent type
                AudioSource audioSource = null;
                switch (agent)
                {
                    case "friend":
                        audioSource = friend_audioSource;
                        break;
                    case "clerk":
                        audioSource = clerk_audioSource;
                        break;
                    case "manager":
                        audioSource = manager_audioSource;
                        break;
                    default:
                        Debug.LogError($"Unknown agent type: {agent}");
                        break;
                }

                if (audioSource != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }
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
    }

    // Function to extract the audio URL and the message from the server response
    private (string audioPath, string message) ExtractInfoFromResponse(string response)
    {
        SpeechResponse speechResponse = JsonUtility.FromJson<SpeechResponse>(response);
        return (speechResponse.audio, speechResponse.message);
    }
}