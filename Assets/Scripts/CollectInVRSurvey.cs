using LLMAgents;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CollectInVRSurvey : MonoBehaviour
{
    private readonly List<(string, List<string>)> surveyQuestions = new()
    {
        ("From the moment I stopped talking, the avatar was quick to start responding meaningfully.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("I felt absorbed during my interaction with this virtual agent.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("The virtual agent's body movements were human-like.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("The virtual agent left a good impression on me.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("I felt awkward, scared, and strange when talking to this agent.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("This agent was reliable, competent, and interactive.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("I would be willing to interact and spend time with this virtual agent again.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("I found this agent's voice to be human-like.",
            new List<string> { "1 = Disagree", "2", "3", "4", "5 = Agree" }),
        ("How human-like was the promptness of reply of this virtual agent?",
            new List<string> { "Very delayed", "Delayed", "Timely (as expected)", "Fast", "Too fast" })
    };

    private int currentQuestionIdx = -1;

    public static CollectInVRSurvey instance;

    [SerializeField] private List<Toggle> toggleList;
    [SerializeField] private Text questionText;
    [SerializeField] private Text selectOneOptionWarningText;

    private Transform agent1SurveyTransform, agent2SurveyTransform, agent3SurveyTransform;

    [SerializeField] private string surveyFilePath = "survey_answers";

    private int subjectId;
    private string sceneName;
    private string unixTimeStamp;

    ////////////////////////////////
    private Collider a1col, a2col, a3col;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        convComplete = new Dictionary<AgentType, bool>
        {
            {AgentType.Agent1, false},
            {AgentType.Agent2, false},
            {AgentType.Agent3, false}
        };

        survComplete = new Dictionary<AgentType, bool>
        {
            {AgentType.Agent1, false},
            {AgentType.Agent2, false},
            {AgentType.Agent3, false}
        };

        FindSurveyTransforms();
        CreateSurveyAnswersFile();
        DisableMeshRenderersOfSurveyColliders();
    }

    public static CollectInVRSurvey GetInstance()
    {
        return instance;
    }

    private void FindSurveyTransforms()
    {
        agent1SurveyTransform = GameObject.Find("Agent1 Survey Transform").transform;
        agent2SurveyTransform = GameObject.Find("Agent2 Survey Transform").transform;
        agent3SurveyTransform = GameObject.Find("Agent3 Survey Transform").transform;
    }

    private void DisableMeshRenderersOfSurveyColliders()
    {
        a1col = GameObject.Find("Agent1 Survey Wall").GetComponent<Collider>();
        a2col = GameObject.Find("Agent2 Survey Wall").GetComponent<Collider>();
        a3col = GameObject.Find("Agent3 Survey Wall").GetComponent<Collider>();

        a1col.enabled = false;
        a2col.enabled = false;
        a3col.enabled = false;

        a1col.GetComponent<MeshRenderer>().enabled = false;
        a2col.GetComponent<MeshRenderer>().enabled = false;
        a3col.GetComponent<MeshRenderer>().enabled = false;
    }

    // When the user walks into an area, they will be prompted with a pop-up survey.
    // This requires the conversation with a specific Agent to be completed.

    private static Dictionary<AgentType, bool> convComplete;
    private static Dictionary<AgentType, bool> survComplete;
    private AgentType agentType;

    public static void MarkConvoAsCompleted(AgentType agent)
    {
        // if already marked as complete, log an error
        if (convComplete[agent])
        {
            Debug.LogError($"Conversation with {agent} Agent already marked as complete.");
            return;
        }

        convComplete[agent] = true;
        instance.ShowSurvey(agent);
    }

    public void ShowSurvey(AgentType _agentType)
    {
        if (StudyControls.instance.ignoreUserStudyConditions)
        {
            Debug.Log("Ignoring survey because ignoreUserStudyConditions is true");
            return;
        }

        // if conv not complete or survey already completed, ignore
        if (!convComplete[_agentType])
        {
            Debug.Log("Ignoring survey because conversation not complete");
            return;
        }

        if (survComplete[_agentType])
        {
            Debug.Log("Ignoring survey because survey already completed");
            return;
        }

        switch (_agentType)
        {
            case AgentType.Agent1:
                a1col.enabled = true;
                break;

            case AgentType.Agent2:
                a2col.enabled = true;
                break;

            case AgentType.Agent3:
                a3col.enabled = true;
                break;
        }

        agentType = _agentType;
        var conditionStr = $"delay: {StudyControls.instance.delayDuration}, fill: {StudyControls.instance.waitIndicatorType}";
        var sceneName = StudyControls.GetUserStudySceneName();
        var agentTypeStr = agentType.ToString();

        var logStr = $"Pop-up Survey for {agentTypeStr} with {conditionStr} in {sceneName}";
        Debug.Log(logStr);

        transform.position = agentType switch
        {
            AgentType.Agent1 => agent1SurveyTransform.position,
            AgentType.Agent2 => agent2SurveyTransform.position,
            AgentType.Agent3 => agent3SurveyTransform.position,
            _ => transform.position
        };

        transform.rotation = agentType switch
        {
            AgentType.Agent1 => agent1SurveyTransform.rotation,
            AgentType.Agent2 => agent2SurveyTransform.rotation,
            AgentType.Agent3 => agent3SurveyTransform.rotation,
            _ => transform.rotation
        };

        currentQuestionIdx = -1;
        GoToNextQuestion();
    }

    public void CloseSurvey()
    {
        survComplete[agentType] = true;
        var agentTypeStr = agentType.ToString();
        var logStr = $"Survey for {agentTypeStr} completed";
        Debug.Log(logStr);
        transform.position = Vector3.one * 3000f;

        a1col.enabled = false;
        a2col.enabled = false;
        a3col.enabled = false;

        // if this was the last question, show "EndOfScene" object at the location of this survey
        if (survComplete[AgentType.Agent1] && survComplete[AgentType.Agent2] && survComplete[AgentType.Agent3])
        {
            Debug.Log("Displaying EndOfScene object...");
            var endOfScene = GameObject.Find("EndOfScene");
            endOfScene.transform.position = agent3SurveyTransform.position;
            endOfScene.transform.rotation = agent3SurveyTransform.rotation;
            endOfScene.SetActive(true);
        }
    }

    private void GoToNextQuestion()
    {
        currentQuestionIdx++;

        if (currentQuestionIdx >= surveyQuestions.Count)
        {
            CloseSurvey();
            return;
        }

        var currentQuestionTuple = surveyQuestions[currentQuestionIdx];

        questionText.text = currentQuestionTuple.Item1;

        for (int i = 0; i < currentQuestionTuple.Item2.Count; i++)
        {
            var toggle = toggleList[i];
            //toggle.gameObject.SetActive(true);
            toggle.isOn = false;
            toggle.GetComponentInChildren<Text>().text = currentQuestionTuple.Item2[i];
        }
    }

    #region File Saving

    private void CreateSurveyAnswersFile()
    {
        subjectId = StudyControls.instance.GetCounterBalanceOrder;
        sceneName = StudyControls.GetUserStudySceneName();
        unixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        surveyFilePath = $"survey_answers_{subjectId}_{sceneName}_{unixTimeStamp}.csv";
        var surveyDirFilePath = Path.Combine(Application.streamingAssetsPath, "Surveys");
        surveyFilePath = Path.Combine(surveyDirFilePath, surveyFilePath);

        if (!Directory.Exists(surveyDirFilePath))
        {
            Directory.CreateDirectory(surveyDirFilePath);
        }

        if (!File.Exists(surveyFilePath))
        {
            string header = "SubjectId,SceneName,AgentType,Delay,Fillin,Question,Answer,Timestamp\n";
            File.WriteAllText(surveyFilePath, header);
        }
    }

    private void LogSurveyAnswer(string responseStr)
    {
        if (!File.Exists(surveyFilePath))
        {
            Debug.LogError("Can't log! Survey answers file not found");
            return;
        }

        // subjectId, sceneName, agentType, question, answer, timestamp
        string question = surveyQuestions[currentQuestionIdx].Item1;
        string newLine = $"{subjectId},{sceneName},{agentType},{StudyControls.instance.delayDuration},{StudyControls.instance.waitIndicatorType},\"{question}\",{responseStr},{DateTime.Now}\n";

        File.AppendAllText(surveyFilePath, newLine);
    }

    #endregion File Saving

    #region ButtonCallbacks

    public void ConfirmChoiceButton()
    {
        Debug.Log("Attempting to save the question response");

        // if multiple toggles are on, warn the user
        var selectedToggles = toggleList.FindAll(toggle => toggle.isOn);
        if (selectedToggles.Count == 1)
        {
            var selectedToggle = selectedToggles[0];
            var selectedToggleIndex = toggleList.IndexOf(selectedToggle);
            Debug.Log($"Selected option: {selectedToggleIndex + 1}");
            // save the response
            var agentTypeStr = agentType.ToString();
            var responseStr = selectedToggleIndex.ToString();
            var logStr = $"Survey response for {agentTypeStr}: {responseStr}";
            Debug.Log(logStr);

            LogSurveyAnswer(responseStr);
            GoToNextQuestion();
        }
        else
        {
            selectOneOptionWarningText.gameObject.SetActive(true);
            Debug.LogWarning("Multiple toggles are on. Please select only one option.");
        }
    }

    public void OnToggleOneChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }

    public void OnToggleTwoChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }

    public void OnToggleThreeChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }

    public void OnToggleFourChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }

    public void OnToggleFiveChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }

    #endregion ButtonCallbacks
}