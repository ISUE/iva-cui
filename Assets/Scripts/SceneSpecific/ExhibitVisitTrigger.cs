using UnityEngine;

public class ExhibitVisitTrigger : MonoBehaviour
{
    [SerializeField] private int exhibitIdx;

    public enum Purpose
    { StartCountdown, EndCountdown }

    [SerializeField] private Purpose purpose;

    private void Start()
    {
        // Disable the mesh renderer of the object
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the object is triggered by the user collider
        if (other.CompareTag("usercollider"))
        {
            switch (purpose)
            {
                case Purpose.StartCountdown:
                    // Call the HandleExhibitVisitTriggerToStartCountdown method from the MuseumSceneController
                    MuseumSceneController.GetInstance().HandleExhibitVisitTriggerToStartCountdown(exhibitIdx);
                    break;

                case Purpose.EndCountdown:
                    // Call the HandleExhibitVisitTriggerToEndCountdown method from the MuseumSceneController
                    MuseumSceneController.GetInstance().EndCountdownTimer();
                    break;
            }

            // Disable the object
            gameObject.SetActive(false);
        }
    }
}