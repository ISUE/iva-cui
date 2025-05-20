using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConversationLogger : MonoBehaviour
{
    private static ConversationLogger instance;
    private static string filePath;
    private static List<LogEntry> logEntries = new List<LogEntry>();

    public static ConversationLogger GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CreateMessageHistoryLog(StudyControls.instance.GetCounterBalanceOrder);
    }

    public static void LogUserMessage(LLMAgents.AgentType speakingTo, string message)
    {
        var logEntry = new LogEntry
        {
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
            who_spoke = "user",
            spoken_to = speakingTo.ToString(),
            message = message
        };

        logEntries.Add(logEntry);
        WriteToFile();
    }

    public static void LogAgentMessage(LLMAgents.AgentType speakingAgent, string message)
    {
        var logEntry = new LogEntry
        {
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
            who_spoke = speakingAgent.ToString(),
            spoken_to = "user",
            message = message
        };

        logEntries.Add(logEntry);
        WriteToFile();
    }

    private static void WriteToFile()
    {
        var logWrapper = new LogWrapper { logs = logEntries };
        string json = JsonUtility.ToJson(logWrapper, true);
        File.WriteAllText(filePath, json);
    }

    public static void CreateMessageHistoryLog(int participantId)
    {
        var fname = $"{participantId}_{System.DateTimeOffset.Now.ToUnixTimeSeconds()}_{StudyControls.GetUserStudySceneName()}.json";
        filePath = Path.Combine(Application.streamingAssetsPath, "Conversations", fname);

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Conversations");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        logEntries = new List<LogEntry>();
        WriteToFile();
    }

    [System.Serializable]
    private class LogEntry
    {
        public long timestamp;
        public string who_spoke;
        public string spoken_to;
        public string message;
    }

    [System.Serializable]
    private class LogWrapper
    {
        public List<LogEntry> logs;
    }
}