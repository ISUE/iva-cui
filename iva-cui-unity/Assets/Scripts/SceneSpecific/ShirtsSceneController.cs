using LLMAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShirtsSceneController : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable interactableShirt;

    private static ShirtsSceneController instance;

    private bool shirtNeedsPickup;

    private void Start()
    {
        shirtNeedsPickup = false;
    }

    public static ShirtsSceneController GetInstance()
    {
        return instance;
    }

    private static readonly List<string> tasks = new List<string>
    {
        "(1) Talk to friend",
        "(2) Pick up the red shirt (press P if on PC)",
        "(3) Return the shirt (it was too large)",
        "(4) Talk to the store manager",
        "(5) Come back to the store clerk"
    };

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (shirtNeedsPickup)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SetReturnShirtAtStoreObjects();
            }
        }
    }

    public static string GetTaskAt(int idx)
    {
        return tasks[idx];
    }

    public static string HandleLLMDeterminedTask(string task)
    {
        if (task.Contains("take shirt"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent1);
            instance.SetPickUpRedShirtObjects();
            return tasks[1];
        }
        else if (task.Contains("talk to manager"))
        {
            instance.SetTalkToManagerObjects();
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent2);
            return tasks[3];
        }
        else if (task.Contains("refund approved"))
        {
            instance.SetComeBackToClerkObjects();
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent3);
            return tasks[4];
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Handle Specific Tasks
    ////////////////////////////////////////////////////////////

    public void SetTalkToFriendObjects()
    {
        // Make the arrow point to Friend
        PathRenderer.EnablePathAt(0);
    }

    public void SetPickUpRedShirtObjects()
    {
        shirtNeedsPickup = true;
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.ConfirmationCode);
        PathRenderer.DisableAllPaths();
    }

    private bool shirtAlreadyPickedUp = false;

    [ContextMenu("ShirtWasPickedUp")]
    public void SetReturnShirtAtStoreObjects()
    {
        if (shirtAlreadyPickedUp)
        {
            return;
        }

        StudyTasks.AdvanceTaskOnUI(tasks[2]);

        StartCoroutine(HideShirtAfterDelay(1f));

        // Make the arrow point to the store Clerk
        PathRenderer.EnablePathAt(1);

        shirtNeedsPickup = false;
        shirtAlreadyPickedUp = true;
    }

    private IEnumerator HideShirtAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        interactableShirt.gameObject.SetActive(false);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.Shirt);
    }

    public void SetTalkToManagerObjects()
    {
        // Make the arrow point to the manager
        PathRenderer.EnablePathAt(2);
    }

    public void SetComeBackToClerkObjects()
    {
        // Make the arrow point to the store Clerk
        StudyTasks.RemoveItemFromInventory(StudyTasks.InventoryItem.Shirt);
        PathRenderer.EnablePathAt(3);
    }
}