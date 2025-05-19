using UnityEngine;

public class RecordEyeData : MonoBehaviour
{
    [SerializeField] private OVREyeGaze leftEye, rightEye;

    private string startTimeStamp;
    private string participantID;
    private string fileName;

    // Get the participant ID from the StudyController
    // Get the start time as unix timestamp
    // Create a new file with the participant ID and start time (streaming assets, folder "EyeData")
    // Every frame, write the eye position, eye rotation (as euler angles), and timestamp to the file

    private void Start()
    {
        startTimeStamp = System.DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        participantID = StudyControls.instance.GetCounterBalanceOrder.ToString();
        fileName = $"{participantID}_{StudyControls.GetUserStudySceneName()}_{startTimeStamp}.csv";
        string filePath = Application.streamingAssetsPath + "/EyeData/" + fileName;
        var header = "Time,LeftEyeX,LeftEyeY,LeftEyeZ,LeftEyeRotX,LeftEyeRotY,LeftEyeRotZ,RightEyeX,RightEyeY,RightEyeZ,RightEyeRotX,RightEyeRotY,RightEyeRotZ\n";

        // Check if the directory doesn't exist, create it
        if (!System.IO.Directory.Exists(Application.streamingAssetsPath + "/EyeData"))
        {
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "/EyeData");
        }

        // Check if file doesn't exist, create it and write the header
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllText(filePath, header);
        }
        else // If file already exists, append the header
        {
            Debug.LogWarning($"File {fileName} already exists, appending data to it.");
        }
    }

    private void Update()
    {
        string time = System.DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        string leftEyePos = $"{leftEye.transform.position.x},{leftEye.transform.position.y},{leftEye.transform.position.z}";
        string leftEyeRot = $"{leftEye.transform.eulerAngles.x},{leftEye.transform.eulerAngles.y},{leftEye.transform.eulerAngles.z}";
        string rightEyePos = $"{rightEye.transform.position.x},{rightEye.transform.position.y},{rightEye.transform.position.z}";
        string rightEyeRot = $"{rightEye.transform.eulerAngles.x},{rightEye.transform.eulerAngles.y},{rightEye.transform.eulerAngles.z}";
        string data = $"{time},{leftEyePos},{leftEyeRot},{rightEyePos},{rightEyeRot}\n";
        string filePath = Application.streamingAssetsPath + "/EyeData/" + fileName;
        System.IO.File.AppendAllText(filePath, data);
    }
}