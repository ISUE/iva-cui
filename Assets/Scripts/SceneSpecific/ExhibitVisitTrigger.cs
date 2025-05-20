using UnityEngine;

public class ExhibitVisitTrigger : MonoBehaviour
{
    [SerializeField] private int exhibitIdx;

    public enum Purpose
    { StartCountdown, EndCountdown }

    [SerializeField] private Purpose purpose;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("usercollider"))
        {
            switch (purpose)
            {
                case Purpose.StartCountdown:
                    MuseumSceneController.GetInstance().HandleExhibitVisitTriggerToStartCountdown(exhibitIdx);
                    break;

                case Purpose.EndCountdown:
                    MuseumSceneController.GetInstance().EndCountdownTimer();
                    break;
            }

            gameObject.SetActive(false);
        }
    }
}