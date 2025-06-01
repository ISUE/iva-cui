using UnityEngine;

public class TrainingPhaseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("usercollider"))
        {
            TrainingSceneController.MoveToNextPhase(gameObject.name != "ew (1)");
            gameObject.SetActive(false);
        }
    }
}