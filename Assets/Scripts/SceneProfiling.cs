using LLMAgents;
using System;
using System.IO;
using UnityEngine;

public class SceneProfiling
{
    private static string filePath = Path.Combine(Application.streamingAssetsPath, "latency_log.csv");

    public static int randomRequestId;

    public static float speakStart; // When the user first presses the mic button
    public static float speakEnd; // When the user presses mic button again to stop recording

    public static float asrStart; // When the ASR request is sent
    public static float asrEnd; // When the ASR response is received

    public static float ttsReqStart; // When the TTS request is sent
    public static float ttsReqEnd; // When the TTS response is received

    public static float ttsVoiceDownloadStart; // When the TTS voice starts downloading
    public static float ttsVoicePlayStart; // When the TTS voice starts speaking
    public static float ttsVoicePlayEnd; // When the TTS voice stops speaking

    public static void ResetTimes()
    {
        speakStart = 0;
        speakEnd = 0;
        asrStart = 0;
        asrEnd = 0;
        ttsReqStart = 0;
        ttsReqEnd = 0;
        ttsVoiceDownloadStart = 0;
        ttsVoicePlayStart = 0;
        ttsVoicePlayEnd = 0;
    }

    public static void SetRandomRequestId()
    {
        randomRequestId = UnityEngine.Random.Range(0, 1000000);
    }

    public static void ComputeTimes(ServerInterface.SpeechResponse speechResponse)
    {
        float userSpeakDuration = speakEnd - speakStart;
        float asrDuration = asrEnd - asrStart;
        float ttsReqDuration = ttsReqEnd - ttsReqStart;
        float ttsDownloadDuration = ttsVoicePlayStart - ttsVoiceDownloadStart;
        float totalDuration = ttsVoicePlayStart - speakEnd;
        float ttsAudioResponseDuration = ttsVoicePlayEnd - ttsVoicePlayStart;

        string txt = "";
        txt += $"Total delay: {totalDuration}s\n";
        txt += $"User speak : {userSpeakDuration}s\n";
        txt += $"ASR duration: {asrDuration}s\n";
        txt += $"TTS request duration: {ttsReqDuration}s\n";
        txt += $"TTS download duration: {ttsDownloadDuration}s\n";
        txt += $"TTS play duration: {ttsAudioResponseDuration}s\n";

        // Add the fields from the response JSON
        txt += $"llm_generation_time: {speechResponse.llm_generation_time}ms\n";
        txt += $"speech_generation_time: {speechResponse.speech_generation_time}ms\n";
        txt += $"llm_client_name: {speechResponse.llm_client_name}\n";
        txt += $"user_input_word_count: {speechResponse.user_input_word_count}\n";
        txt += $"response_word_count: {speechResponse.response_word_count}\n";
        txt += $"transition_length: {speechResponse.transition_length}\n";

        Debug.Log(txt);
    }

    public static void SetCustomPath(string path)
    {
        filePath = path;
    }

    public static void LogLatencyDetails(AgentType agentType, ServerInterface.SpeechResponse speechResponse)
    {
        float userSpeakDuration = speakEnd - speakStart;
        float asrDuration = asrEnd - asrStart;
        float ttsReqDuration = ttsReqEnd - ttsReqStart;
        float ttsDownloadDuration = ttsVoicePlayStart - ttsVoiceDownloadStart;
        float totalDuration = ttsVoicePlayStart - speakEnd;
        float ttsAudioResponseDuration = ttsVoicePlayEnd - ttsVoicePlayStart;

        string currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string agentTypeString = agentType.ToString();

        string newLine = $"{currentTime},{randomRequestId},{agentTypeString},{userSpeakDuration},{asrDuration},{ttsReqDuration},{totalDuration},";
        newLine += $"{speakStart},{speakEnd},{asrStart},{asrEnd},{ttsReqStart},{ttsReqEnd},{ttsVoiceDownloadStart},{ttsVoicePlayStart},{ttsVoicePlayEnd},";
        newLine += $"{speechResponse.llm_generation_time},{speechResponse.speech_generation_time},{speechResponse.llm_client_name}";

        if (!File.Exists(filePath))
        {
            string header = "current_time,request_id,agent_type,speak_duration,asr_duration,tts_req_duration,total_duration,";
            header += "speakStart,speakEnd,asrStart,asrEnd,ttsReqStart,ttsReqEnd,ttsVoiceDownloadStart,ttsVoicePlayStart,ttsVoicePlayEnd,";
            header += "llm_generation_time,speech_generation_time,llm_client_name";
            File.WriteAllText(filePath, header + Environment.NewLine);
        }

        File.AppendAllText(filePath, newLine + Environment.NewLine);
    }
}