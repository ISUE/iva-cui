using System.IO;
using UnityEngine;

public class MicrophoneHandler : MonoBehaviour
{
    public bool IsRecording { get; private set; }

    private AudioClip recording;
    private int maxRecordingTime = 300;

    // This is set through a dropdown in the editor. Impl. in TextToSpeechRequestEditor.cs
    [Header("Microphone input")]
    [HideInInspector] public string selectedMicString;

    private string micStringThatIsRecording;

    [Header("Feedback sounds")]
    private AudioSource feedbackAudioSource;

    [SerializeField] private AudioSource vrAudioSrc;
    [SerializeField] private AudioSource pcAudioSrc;
    [SerializeField] private AudioClip micUnavailableSound, micOnSound, micOffSound;
    [SerializeField] private AudioClip newTaskAvailableSound;

    private int startSample = 0;
    private int endSample = 0;

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            Debug.Log($"Microphone is currently set to {selectedMicString}");
        }
        else
        {
            Debug.LogError("No microphone devices available.");
        }

        // the feedbackAudioSource will be the one of the two that is not null and is enabled
        if (vrAudioSrc != null && vrAudioSrc.isActiveAndEnabled)
        {
            feedbackAudioSource = vrAudioSrc;
        }
        else if (pcAudioSrc != null && pcAudioSrc.isActiveAndEnabled)
        {
            feedbackAudioSource = pcAudioSrc;
        }
        else
        {
            Debug.LogError("No feedback audio source available.");
        }

        Invoke(nameof(StartMicInput), .5f); // delay to avoid null reference in case mic changes by another script
    }

    public void StartMicInputAfterMicChange()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Application is not playing. Microphone input will not start.");
        }

        // End the current recording if it is still going
        StopRecording();
        Microphone.End(micStringThatIsRecording);

        StartMicInput();
    }

    private void StartMicInput()
    {
        recording = Microphone.Start(selectedMicString, true, maxRecordingTime, 16000);
        micStringThatIsRecording = selectedMicString;
        if (recording == null)
        {
            Debug.LogError("Failed to start microphone.");
            return;
        }
        ToggleMicIsRecordingUI(false);
        Debug.Log("Microphone started and is recording continuously...");
    }

    public void StartRecording()
    {
        if (recording == null)
        {
            Debug.LogError("No microphone available.");
            return;
        }

        IsRecording = true;
        startSample = Microphone.GetPosition(micStringThatIsRecording);
        PlayMicSound(true);
        ToggleMicIsRecordingUI(true);
        Debug.Log("Recording start marker set at sample: " + startSample);
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        endSample = Microphone.GetPosition(micStringThatIsRecording);
        IsRecording = false;
        PlayMicSound(false);
        ToggleMicIsRecordingUI(false);
        Debug.Log("Recording end marker set at sample: " + endSample);
    }

    private static void ToggleMicIsRecordingUI(bool value)
    {
        StudyTasks.SetMicActiveObjects(value);
    }

    public byte[] GetLatestMicAudioBytes()
    {
        if (recording == null)
        {
            Debug.LogError("No recording available.");
            return null;
        }

        int sampleCount = endSample - startSample;
        if (sampleCount < 0) sampleCount += recording.samples;

        AudioClip finalClip = CutClipAtSampleRange(recording, startSample, sampleCount);
        var handler = new AudioMemoryStreamHandler();
        MemoryStream audioStream = handler.PrepareAudioStream(finalClip);

        var audioBytes = audioStream.ToArray();
        return audioBytes;
    }

    private AudioClip CutClipAtSampleRange(AudioClip recordedClip, int startSample, int sampleCount)
    {
        float[] samples = new float[sampleCount * recordedClip.channels];
        recordedClip.GetData(samples, startSample);
        AudioClip cutClip = AudioClip.Create("CutClip", sampleCount, recordedClip.channels, recordedClip.frequency, false);
        cutClip.SetData(samples, 0);
        return cutClip;
    }

    public static byte[] GetAudioBytesFromClip(AudioClip inputClip)
    {
        var handler = new AudioMemoryStreamHandler();
        MemoryStream audioStream = handler.PrepareAudioStream(inputClip);
        var audioBytes = audioStream.ToArray();
        return audioBytes;
    }

    private void PlayMicSound(bool value)
    {
        feedbackAudioSource.clip = value ? micOnSound : micOffSound;
        feedbackAudioSource.PlayOneShot(feedbackAudioSource.clip);
    }

    public void PlayMicUnavailableSound()
    {
        feedbackAudioSource.clip = micUnavailableSound;
        feedbackAudioSource.PlayOneShot(feedbackAudioSource.clip);
    }

    public void PlayNewTaskAvailableNotificationSound()
    {
        feedbackAudioSource.clip = newTaskAvailableSound;
        feedbackAudioSource.PlayOneShot(feedbackAudioSource.clip);
    }
}