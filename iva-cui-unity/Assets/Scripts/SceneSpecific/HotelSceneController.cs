using LLMAgents;
using System.Collections.Generic;
using UnityEngine;

public class HotelSceneController : MonoBehaviour
{
    private static HotelSceneController instance;

    public static HotelSceneController GetInstance()
    {
        return instance;
    }

    private static readonly List<string> tasks = new List<string>
    {
        "(1) Check in",
        "(2) Check your room",
        "(3) Complain to receptionist",
        "(4) Go to the restaurant",
        "(5) Take a seat"
    };

    private void Awake()
    {
        instance = this;
    }

    public static string GetTaskAt(int idx)
    {
        return tasks[idx];
    }

    public static string HandleLLMDeterminedTask(string task)
    {
        if (task.Contains("go to room"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent1);
            instance.SetGoToRoomObjects();
            return tasks[1];
        }
        else if (task.Contains("go to receptionist"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent2);
            instance.SetGoToReceptionistObjects();
            return tasks[2];
        }
        else if (task.Contains("go to restaurant"))
        {
            instance.SetGoToRestaurantObjects();
            return tasks[3];
        }
        else if (task.Contains("take a seat"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent3);
            instance.SetTakeASeatObjects();
            return tasks[4];
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    public void SetFirstInteractionObjects()
    {
        PathRenderer.EnablePathAt(0);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.HotelReservation);
    }

    private void SetGoToRoomObjects()
    {
        PathRenderer.EnablePathAt(1);
        StudyTasks.RemoveItemFromInventory(StudyTasks.InventoryItem.HotelReservation);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.HotelDirectory);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.HotelKey);
    }

    private void SetGoToReceptionistObjects()
    {
        PathRenderer.EnablePathAt(2);
    }

    private void SetGoToRestaurantObjects()
    {
        PathRenderer.EnablePathAt(3);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.MealPass);
    }

    private void SetTakeASeatObjects()
    {
        PathRenderer.EnablePathAt(4);
        StudyTasks.RemoveItemFromInventory(StudyTasks.InventoryItem.MealPass);
    }
}