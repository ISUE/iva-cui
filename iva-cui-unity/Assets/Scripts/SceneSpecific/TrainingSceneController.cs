using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TrainingSceneController : MonoBehaviour
{
    [SerializeField] private AudioSource botAudioSource;

    [SerializeField] private Transform interactiveShirt;
    private static int trainingPhaseIdx = 0;
    private static int pathPhaseIdx = 0;

    [SerializeField] private Text transcriptionTextUI;
    [SerializeField] private GameObject endOfSceneObject;

    [SerializeField] private float delayResponseBy = 4.25f;
    [SerializeField] private float stdevDelayResponseBy = 0.2f;
    private float timeStamp_UserFinishedInput = 0.0f;

    private static List<string> taskStringsForDisplay = new List<string>();
    private static List<TMP_Text> taskTexts;
    private static List<RectTransform> micActiveObjects = new List<RectTransform>();

    public enum InventoryItem
    {
        ConfirmationCode, Shirt, HotelDirectory, HotelKey, MealPass, MuseumTicket, MuseumBrochure
    }

    private static Dictionary<InventoryItem, Sprite> inventorySprites;
    private static List<Image> inventorySlotsOnUI = new List<Image>();

    private static MicrophoneHandler microphoneHandler;

    private static readonly List<string> tasks = new List<string>
    {
        "(1) Move forward",
        "(2) Turn and move right",
        "(3) Complete the surveys",
        "(4) Pick up an object",
        "(5) Use Microphone"
    };

    [SerializeField] private InputActionReference controllerMicButton;

    public void OnTrainingSceneShirtPickup()
    {
        StartCoroutine(OnTrainingSceneShirtPickupIEnumerator());
    }

    private IEnumerator OnTrainingSceneShirtPickupIEnumerator()
    {
        yield return new WaitForSeconds(1.5f);
        Training_AddItemToInventory(InventoryItem.Shirt);
        interactiveShirt.gameObject.SetActive(false);
        MoveToNextPhase();
    }

    public static void MoveToNextPhase(bool advancePath = true)
    {
        if (advancePath)
        {
            PathRenderer.EnablePathAt(pathPhaseIdx);
            pathPhaseIdx++;
        }
        AdvanceTaskOnUI(tasks[trainingPhaseIdx]);
        trainingPhaseIdx++;
        microphoneHandler.PlayNewTaskAvailableNotificationSound();
    }

    private void InitializeTaskUI()
    {
        taskStringsForDisplay = new List<string>
        {
            "<b>Tasks</b>",
        };

        SetUpdatedTaskText();
        MoveToNextPhase();
    }

    public static void AdvanceTaskOnUI(string task)
    {
        if (taskStringsForDisplay.Count > 1)
        {
            taskStringsForDisplay[taskStringsForDisplay.Count - 1] = $"<s>{taskStringsForDisplay[taskStringsForDisplay.Count - 1]}</s>";
        }

        taskStringsForDisplay.Add(task);
        SetUpdatedTaskText();
    }

    private static void SetUpdatedTaskText()
    {
        string text = "";
        foreach (string task in taskStringsForDisplay)
        {
            text += task + "\n";
        }
        foreach (TMP_Text taskText in taskTexts)
        {
            taskText.text = text;
        }
    }

    public static void SetMicActiveObjects(bool active)
    {
        foreach (Transform micActiveObject in micActiveObjects)
        {
            micActiveObject.gameObject.SetActive(active);
        }
    }

    private void Start()
    {
        ServerInterface.RefreshTrainingConversation();

        microphoneHandler = FindObjectOfType<MicrophoneHandler>();
        if (microphoneHandler == null)
        {
            Debug.LogError("MicrophoneHandler not found.");
        }

        taskTexts = FindObjectsOfType<TMP_Text>().Where(t => t.name == "task_ui_text").ToList();

        // all gameobjects name "mic_active_object" need to be found even if they are disabled
        micActiveObjects = FindObjectsOfType<RectTransform>().Where(t => t.name == "mic_active_object").ToList();
        SetMicActiveObjects(false);

        // Initialize the inventory
        inventorySprites = new Dictionary<InventoryItem, Sprite>
        {
            { InventoryItem.Shirt, Resources.Load<Sprite>("Inventory/Red_Shirt") },
        };

        // find the four objects "InventorySlot1", "InventorySlot2", "InventorySlot3", "InventorySlot4"
        // they have an Image component where sprites will be placed
        var inventorySlots = FindObjectsOfType<RectTransform>().Where(t => t.name.Contains("InventorySlot")).ToList();
        foreach (var slot in inventorySlots)
        {
            var img = slot.GetComponent<Image>();
            if (img != null)
            {
                inventorySlotsOnUI.Add(img);
            }
            slot.gameObject.SetActive(false);
        }

        Invoke(nameof(InitializeTaskUI), .1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            HandleMicButtonInput();
            return;
        }

        if (controllerMicButton != null && controllerMicButton.action.WasPressedThisFrame())
        {
            HandleMicButtonInput();
            return;
        }
    }

    private bool isThinking = false;

    private void HandleMicButtonInput()
    {
        if (!microphoneHandler.IsRecording)
        {
            if (isThinking || botAudioSource.isPlaying)
            {
                microphoneHandler.PlayMicUnavailableSound();
                return;
            }
            microphoneHandler.StartRecording();
            SetMicActiveObjects(true);
        }
        else
        {
            microphoneHandler.StopRecording();
            SetMicActiveObjects(false);
            var audioBytes = microphoneHandler.GetLatestMicAudioBytes();
            StartCoroutine(ServerInterface.instance.UploadAudioBytes(audioBytes, PrintTranscriptionAndSendResponseGenerationRequest));
            timeStamp_UserFinishedInput = Time.time;
            isThinking = true;
        }
    }

    private static int micInputsDone = 0;

    public void PrintTranscriptionAndSendResponseGenerationRequest(string transcription)
    {
        var transcription_on_ui = $"You said: \"{transcription}\"";
        transcriptionTextUI.text = transcription_on_ui;
        micInputsDone++;

        if (micInputsDone == 3)
        {
            endOfSceneObject.SetActive(true);
        }

        StartCoroutine(GenerateResponseToTranscription(transcription));
    }

    private IEnumerator GenerateResponseToTranscription(string text)
    {
        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"http://127.0.0.1:8000/speak/agent1/?q={encodedText}";

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
                TrainingSpeechResponse speechResponse = ExtractInfoFromResponse(webRequest.downloadHandler.text);

                string audioFileUrl = $"http://127.0.0.1:8000/{speechResponse.audio}";

                StartCoroutine(Training_DownloadAndPlayAudio(audioFileUrl, speechResponse));
            }
        }
    }

    public static float GetNormalRandom(float mean, float standardDeviation)
    {
        // Box-Muller transform
        float u1 = 1.0f - Random.value; // uniform(0,1) random number
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + standardDeviation * randStdNormal; // return the normally distributed value
    }

    private IEnumerator Training_DownloadAndPlayAudio(string audioUrl, TrainingSpeechResponse speechResponse)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                // target delay duration is delayResponseBy +-stdevDelayResponseBy (stdev of 0.2 seconds)
                float targetDelayDuration = GetNormalRandom(delayResponseBy, stdevDelayResponseBy);
                float timeSinceUserInput = Time.time - timeStamp_UserFinishedInput;
                float remainingDelay = targetDelayDuration - timeSinceUserInput;

                if (remainingDelay > 0f)
                {
                    Debug.Log($"Target delay: {targetDelayDuration}. Remaining delay: {remainingDelay}.");
                    yield return new WaitForSeconds(remainingDelay);
                }
                else
                {
                    Debug.LogWarning($"Target delay: {targetDelayDuration}. DELAY WAS NEGATIVE ({remainingDelay}) -> RESPONSE WAS LATE");
                }

                botAudioSource.clip = clip;
                botAudioSource.Play();
                isThinking = false;
                timeStamp_UserFinishedInput = 0.0f;
            }
            else
            {
                Debug.LogError($"Failed to download audio clip: {www.error}");
                isThinking = false;
            }
        }
    }

    [System.Serializable]
    public class TrainingSpeechResponse
    {
        public string message;
        public string audio;
        public string transition;

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

    private TrainingSpeechResponse ExtractInfoFromResponse(string response)
    {
        return JsonUtility.FromJson<TrainingSpeechResponse>(response);
    }

    public static void Training_AddItemToInventory(InventoryItem item)
    {
        Sprite itemSprite = Training_GetSpriteForItem(item);

        foreach (var slot in inventorySlotsOnUI)
        {
            if (slot.gameObject.activeInHierarchy && slot.sprite != null)
            {
                if (slot.sprite.name == itemSprite.name)
                {
                    Debug.LogWarning($"{item} is already in inventory");
                    return;
                }
            }
        }

        foreach (var slot in inventorySlotsOnUI)
        {
            if (slot.sprite == null)
            {
                slot.sprite = itemSprite;
                slot.gameObject.SetActive(true);
                break;
            }
        }
    }

    public static void Training_RemoveItemFromInventory(InventoryItem item)
    {
        Sprite itemSprite = Training_GetSpriteForItem(item);

        foreach (var slot in inventorySlotsOnUI)
        {
            if (slot.sprite != null && slot.gameObject.activeInHierarchy)
            {
                if (slot.sprite.name == itemSprite.name)
                {
                    slot.sprite = null;
                    slot.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    public static Sprite Training_GetSpriteForItem(InventoryItem item)
    {
        if (inventorySprites.TryGetValue(item, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogError($"Sprite for {item} not found!");
            return null;
        }
    }
}