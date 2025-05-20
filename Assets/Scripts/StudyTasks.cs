using LLMAgents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudyTasks : MonoBehaviour
{
    private static List<string> taskStringsForDisplay = new List<string>();
    private static List<TMP_Text> taskTexts;
    private static List<RectTransform> micActiveObjects = new List<RectTransform>();

    public enum InventoryItem
    {
        ConfirmationCode, Shirt, HotelReservation, HotelDirectory, HotelKey, MealPass, MuseumTicket, MuseumBrochure
    }

    private static Dictionary<InventoryItem, Sprite> inventorySprites;
    private static Dictionary<InventoryItem, string> inventoryNames;

    private static List<RectTransform> inventorySlotsOnUI = new List<RectTransform>();
    private static List<Image> inventoryImagesOnUI = new List<Image>();
    private static List<TMP_Text> inventoryTextOnUI = new List<TMP_Text>();

    private static MicrophoneHandler microphoneHandler;

    private void InitializeFirstTask()
    {
        taskStringsForDisplay = new List<string>
        {
            "<b>Tasks</b>"
        };

        switch (StudyControls.GetUserStudyScene())
        {
            case StudyControls.UserStudyScene.Shirts:
                taskStringsForDisplay.Add(ShirtsSceneController.GetTaskAt(0));
                ShirtsSceneController.GetInstance().SetTalkToFriendObjects();
                break;

            case StudyControls.UserStudyScene.Hotel:
                taskStringsForDisplay.Add(HotelSceneController.GetTaskAt(0));
                HotelSceneController.GetInstance().SetFirstInteractionObjects();
                break;

            case StudyControls.UserStudyScene.Museum:
                taskStringsForDisplay.Add(MuseumSceneController.GetTaskAt(0));
                MuseumSceneController.GetInstance().SetFirstInteractionObjects();
                break;
        }

        SetUpdatedTaskText();
    }

    public static bool agentFinishedTalking = false;

    public static IEnumerator SetAgentFinishedTalkingAfterSeconds(float seconds, AgentType agentType, ServerInterface.SpeechResponse speechResponse)
    {
        yield return new WaitForSeconds(seconds);
        agentFinishedTalking = true;
        SceneProfiling.ttsVoicePlayEnd = Time.time;
        SceneProfiling.LogLatencyDetails(agentType, speechResponse);
    }

    public static IEnumerator HandleLLMDeterminedTask(string task)
    {
        if (task.Length == 0 || task.Equals("none"))
        {
            yield break;
        }

        yield return new WaitUntil(() => agentFinishedTalking);

        string taskText = "";

        switch (StudyControls.GetUserStudyScene())
        {
            case StudyControls.UserStudyScene.Shirts:
                taskText = ShirtsSceneController.HandleLLMDeterminedTask(task);
                break;

            case StudyControls.UserStudyScene.Hotel:
                taskText = HotelSceneController.HandleLLMDeterminedTask(task);
                break;

            case StudyControls.UserStudyScene.Museum:
                taskText = MuseumSceneController.HandleLLMDeterminedTask(task);
                break;
        }

        AdvanceTaskOnUI(taskText);
    }

    public static void AdvanceTaskOnUI(string task)
    {
        // Make the latest task strikethrough
        if (taskStringsForDisplay.Count > 1)
        {
            taskStringsForDisplay[taskStringsForDisplay.Count - 1] = $"<s>{taskStringsForDisplay[taskStringsForDisplay.Count - 1]}</s>";
        }

        taskStringsForDisplay.Add(task);

        SetUpdatedTaskText();
        microphoneHandler.PlayNewTaskAvailableNotificationSound();
    }

    private static void SetUpdatedTaskText()
    {
        // concatenate the text and update the text field
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
        microphoneHandler = FindObjectOfType<MicrophoneHandler>();
        if (microphoneHandler == null)
        {
            Debug.LogError("MicrophoneHandler not found.");
        }

        taskTexts = FindObjectsOfType<TMP_Text>().Where(t => t.name == "task_ui_text").ToList();

        // all gameobjects name "mic_active_object" need to be found even if they are disabled
        micActiveObjects = FindObjectsOfType<RectTransform>().Where(t => t.name == "mic_active_object").ToList();

        // Initialize the dictionary
        inventorySprites = new Dictionary<InventoryItem, Sprite>
        {
            { InventoryItem.ConfirmationCode, Resources.Load<Sprite>("Inventory/Confirmation_Code") },
            { InventoryItem.Shirt, Resources.Load<Sprite>("Inventory/Red_Shirt") },
            { InventoryItem.HotelReservation, Resources.Load<Sprite>("Inventory/#2024") },
            { InventoryItem.HotelDirectory, Resources.Load<Sprite>("Inventory/Hotel_Brocherure") },
            { InventoryItem.HotelKey, Resources.Load<Sprite>("Inventory/Key-Hotel") },
            { InventoryItem.MealPass, Resources.Load<Sprite>("Inventory/Meal_Ticket") },
            { InventoryItem.MuseumTicket, Resources.Load<Sprite>("Inventory/Museum_Ticket") },
            { InventoryItem.MuseumBrochure, Resources.Load<Sprite>("Inventory/Museum_Booklet") }
        };

        inventoryNames = new Dictionary<InventoryItem, string>
        {
            { InventoryItem.ConfirmationCode, "Confirmation Code" },
            { InventoryItem.Shirt, "Red Shirt" },
            { InventoryItem.HotelReservation, "Hotel Reservation" },
            { InventoryItem.HotelDirectory, "Hotel Directory" },
            { InventoryItem.HotelKey, "Room 111 Key" },
            { InventoryItem.MealPass, "Dinner Voucher" },
            { InventoryItem.MuseumTicket, "Museum Ticket" },
            { InventoryItem.MuseumBrochure, "Museum Brochure" }
        };

        // find the four objects "InventorySlot1", "InventorySlot2", "InventorySlot3", "InventorySlot4"
        // they have an Image component where sprites will be placed
        var inventorySlots = FindObjectsOfType<RectTransform>().Where(t => t.name.Contains("Inventory_Slot")).ToList();
        foreach (var slot in inventorySlots)
        {
            inventorySlotsOnUI.Add(slot);

            // Loop through children to find the Image component and the TMP_Text component

            foreach (Transform child in slot)
            {
                if (child.name.Contains("Image_Inventory"))
                {
                    inventoryImagesOnUI.Add(child.GetComponent<Image>());
                }
                else if (child.name.Contains("Text_Inventory"))
                {
                    inventoryTextOnUI.Add(child.GetComponent<TMP_Text>());
                }
            }

            slot.gameObject.SetActive(false);
        }

        Invoke(nameof(InitializeFirstTask), .5f);
    }

    public static void AddItemToInventory(InventoryItem item)
    {
        // fill the first unocupied slot
        // iterate over the list of inventorySlotsOnUI and find the first unoccupied slot to add the item there
        // Set that object to active

        Sprite itemSprite = GetSpriteForItem(item);

        // first check if an item is already present, and if so log warning
        foreach (var slot in inventoryImagesOnUI)
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

        for (int i = 0; i < inventorySlotsOnUI.Count; i++)
        {
            if (inventoryImagesOnUI[i].sprite == null)
            {
                inventoryImagesOnUI[i].sprite = itemSprite;
                inventoryTextOnUI[i].text = inventoryNames[item];
                inventorySlotsOnUI[i].gameObject.SetActive(true);
                break;
            }
        }
    }

    public static void RemoveItemFromInventory(InventoryItem item)
    {
        Sprite itemSprite = GetSpriteForItem(item);

        for (int i = 0; i < inventorySlotsOnUI.Count; i++)
        {
            if (inventorySlotsOnUI[i].gameObject.activeInHierarchy && inventoryImagesOnUI[i].sprite != null)
            {
                if (inventoryImagesOnUI[i].sprite.name == itemSprite.name)
                {
                    inventoryImagesOnUI[i].sprite = null;
                    inventoryTextOnUI[i].text = "";
                    inventorySlotsOnUI[i].gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    public static Sprite GetSpriteForItem(InventoryItem item)
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

    // ContextMenu methods for testing each inventory item
    [ContextMenu("PickUpConfirmationCode")]
    public void PickUpConfirmationCode()
    {
        AddItemToInventory(InventoryItem.ConfirmationCode);
    }

    [ContextMenu("PickUpShirt")]
    public void PickUpShirt()
    {
        AddItemToInventory(InventoryItem.Shirt);
    }

    [ContextMenu("PickUpHotelDirectory")]
    public void PickUpHotelDirectory()
    {
        AddItemToInventory(InventoryItem.HotelDirectory);
    }

    [ContextMenu("PickUpHotelKey")]
    public void PickUpHotelKey()
    {
        AddItemToInventory(InventoryItem.HotelKey);
    }

    [ContextMenu("PickUpMealPass")]
    public void PickUpMealPass()
    {
        AddItemToInventory(InventoryItem.MealPass);
    }

    [ContextMenu("PickUpMuseumTicket")]
    public void PickUpMuseumTicket()
    {
        AddItemToInventory(InventoryItem.MuseumTicket);
    }

    [ContextMenu("PickUpMuseumBrochure")]
    public void PickUpMuseumBrochure()
    {
        AddItemToInventory(InventoryItem.MuseumBrochure);
    }

    [ContextMenu("RemoveConfirmationCode")]
    public void RemoveConfirmationCode()
    {
        RemoveItemFromInventory(InventoryItem.ConfirmationCode);
    }

    [ContextMenu("RemoveShirt")]
    public void RemoveShirt()
    {
        RemoveItemFromInventory(InventoryItem.Shirt);
    }

    [ContextMenu("RemoveHotelDirectory")]
    public void RemoveHotelDirectory()
    {
        RemoveItemFromInventory(InventoryItem.HotelDirectory);
    }

    [ContextMenu("RemoveHotelKey")]
    public void RemoveHotelKey()
    {
        RemoveItemFromInventory(InventoryItem.HotelKey);
    }

    [ContextMenu("RemoveMealPass")]
    public void RemoveMealPass()
    {
        RemoveItemFromInventory(InventoryItem.MealPass);
    }

    [ContextMenu("RemoveMuseumTicket")]
    public void RemoveMuseumTicket()
    {
        RemoveItemFromInventory(InventoryItem.MuseumTicket);
    }

    [ContextMenu("RemoveMuseumBrochure")]
    public void RemoveMuseumBrochure()
    {
        RemoveItemFromInventory(InventoryItem.MuseumBrochure);
    }

    [ContextMenu("PickupReservation")]
    public void PickupReservation()
    {
        AddItemToInventory(InventoryItem.HotelReservation);
    }

    [ContextMenu("RemoveReservation")]
    public void RemoveReservation()
    {
        RemoveItemFromInventory(InventoryItem.HotelReservation);
    }
}