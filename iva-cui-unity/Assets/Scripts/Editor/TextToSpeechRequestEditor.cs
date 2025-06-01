using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MicrophoneHandler))]
public class TextToSpeechRequestEditor : Editor
{
    // This index will keep track of which string is currently selected in the dropdown
    private int selectedIndex = 0;

    // List of options for the dropdown
    private string[] options;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        List<string> micList = new List<string>();
        foreach (var device in Microphone.devices)
        {
            micList.Add(device);
        }

        // Convert the list of strings to an array for the dropdown
        options = micList.ToArray();

        MicrophoneHandler microphoneHandler = (MicrophoneHandler)target; // Cast the target to your specific script type

        // Find the current index of the selected string to set the dropdown correctly
        selectedIndex = System.Array.IndexOf(options, microphoneHandler.selectedMicString);
        if (selectedIndex < 0) selectedIndex = 0; // Default to first option if not found

        // Display the dropdown and get the new selected index
        selectedIndex = EditorGUILayout.Popup("Select Microphone", selectedIndex, options);

        // Update the selected string in your TTS script
        microphoneHandler.selectedMicString = options[selectedIndex];

        if (GUI.changed)
        {
            string _micName;
            foreach (var device in Microphone.devices)
            {
                //Debug.Log("Name: " + device);
                if (device.Contains(options[selectedIndex]))
                {
                    _micName = device;
                    microphoneHandler.selectedMicString = _micName;
                    Debug.Log("Microphone changed to: " + _micName);
                    microphoneHandler.StartMicInputAfterMicChange();
                }
            }

            // Optional: Mark the TTS object as dirty to ensure the change is saved
            EditorUtility.SetDirty(microphoneHandler);
        }
    }
}