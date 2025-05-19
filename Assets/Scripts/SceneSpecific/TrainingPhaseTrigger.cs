using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingPhaseTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("usercollider"))
        {
            TrainingSceneController.MoveToNextPhase(gameObject.name != "ew (1)");
            gameObject.SetActive(false);
        }
    }
}
