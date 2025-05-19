using System.IO;
using UnityEngine;
using System.Collections.Generic;

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
        // Create a log entry
        var logEntry = new LogEntry
        {
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
            who_spoke = "user",
            spoken_to = speakingTo.ToString(),
            message = message
        };

        // Add the log entry to the list
        logEntries.Add(logEntry);

        // Write all entries to the file
        WriteToFile();
    }

    public static void LogAgentMessage(LLMAgents.AgentType speakingAgent, string message)
    {
        // Create a log entry
        var logEntry = new LogEntry
        {
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
            who_spoke = speakingAgent.ToString(),
            spoken_to = "user",
            message = message
        };

        // Add the log entry to the list
        logEntries.Add(logEntry);

        // Write all entries to the file
        WriteToFile();
    }

    private static void WriteToFile()
    {
        // Create a wrapper object that contains the list of log entries
        var logWrapper = new LogWrapper
        {
            logs = logEntries
        };

        // Convert the wrapper object to a JSON string
        string json = JsonUtility.ToJson(logWrapper, true);

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);
    }

    public static void CreateMessageHistoryLog(int participantId)
    {
        // fname = {participant_id}_{timestamp}_{scene}.json
        var fname = $"{participantId}_{System.DateTimeOffset.Now.ToUnixTimeSeconds()}_{StudyControls.GetUserStudySceneName()}.json";
        filePath = Path.Combine(Application.streamingAssetsPath, "Conversations", fname);

        // Create directory if it does not exist
        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Conversations");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Initialize the file with an empty array in the correct JSON format
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
