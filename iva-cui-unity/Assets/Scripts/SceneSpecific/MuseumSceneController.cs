using LLMAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumSceneController : MonoBehaviour
{
    [SerializeField] private float countDownDuration = 60f;
    private float countDownTimer;

    private static MuseumSceneController instance;

    public static MuseumSceneController GetInstance()
    {
        return instance;
    }

    private static readonly List<string> tasks = new List<string>
    {
        "(1) Get admitted to the museum",
        "(2) Look at the artifacts of exhibit 1",
        "(3) Talk to Volunteer 1",
        "(4) Look at the artifacts of exhibit 2",
        "(5) Talk to Volunteer 2",
        "(6) Leave the museum",
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
        if (task.Contains("visit 1"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent1);
            instance.SetVisitFirstExhibitObjects();
            return tasks[1];
        }
        else if (task.Contains("visit 2"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent2);
            instance.SetVisitSecondExhibitObjects();
            return tasks[3];
        }
        else if (task.Contains("leave"))
        {
            CollectInVRSurvey.MarkConvoAsCompleted(AgentType.Agent3);
            instance.SetLeaveMuseumObjects();
            return tasks[5];
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    public void HandleExhibitVisitTriggerToStartCountdown(int exhibitIdx)
    {
        countDownTimer = countDownDuration;
        StartCoroutine(StartExhibitCountdown(exhibitIdx));
    }

    public void SetFirstInteractionObjects()
    {
        PathRenderer.EnablePathAt(0);
    }

    public void SetVisitFirstExhibitObjects()
    {
        PathRenderer.EnablePathAt(1);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.MuseumTicket);
        StudyTasks.AddItemToInventory(StudyTasks.InventoryItem.MuseumBrochure);
    }

    public void SetTalkToVolunteer1Objects()
    {
        PathRenderer.EnablePathAt(2);
        StudyTasks.AdvanceTaskOnUI(tasks[2]);
    }

    public void SetVisitSecondExhibitObjects()
    {
        PathRenderer.EnablePathAt(3);
    }

    public void SetTalkToVolunteer2Objects()
    {
        PathRenderer.EnablePathAt(4);
        StudyTasks.AdvanceTaskOnUI(tasks[4]);
    }

    public void SetLeaveMuseumObjects()
    {
        PathRenderer.EnablePathAt(5);
    }

    public void EndCountdownTimer()
    {
        countDownTimer = 0;
    }

    private IEnumerator StartExhibitCountdown(int exhibitIdx)
    {
        // subtract Time.deltaTime from countDownTimer until it reaches 0
        while (countDownTimer > 0)
        {
            countDownTimer -= Time.deltaTime;
            yield return null;
        }

        switch (exhibitIdx)
        {
            case 1:
                SetTalkToVolunteer1Objects();
                break;

            case 2:
                SetTalkToVolunteer2Objects();
                break;

            default:
                throw new System.NotImplementedException();
        }
    }
}