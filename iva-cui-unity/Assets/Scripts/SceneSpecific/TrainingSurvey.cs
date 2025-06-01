using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingSurvey : MonoBehaviour
{
    [SerializeField] private List<Toggle> toggleList;
    [SerializeField] private Text selectOneOptionWarningText;

    [SerializeField] private List<Transform> transformsToDisableWhenAnswerAccepted;

    public void ConfirmChoiceButton()
    {
        var selectedToggles = toggleList.FindAll(toggle => toggle.isOn);
        {
            if (selectedToggles.Count == 1)
            {
                var selectedToggle = selectedToggles[0];
                var selectedToggleIndex = toggleList.IndexOf(selectedToggle);
                Debug.Log($"Selected option: {selectedToggleIndex + 1}");

                ProceedInScene();
            }
            else
            {
                selectOneOptionWarningText.gameObject.SetActive(true);
            }
        }
    }

    private void ProceedInScene()
    {
        if (gameObject.name == "SurveyTraining2")
            TrainingSceneController.MoveToNextPhase();

        foreach (var t in transformsToDisableWhenAnswerAccepted)
        {
            t.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void OnToggleChoice()
    {
        selectOneOptionWarningText.gameObject.SetActive(false);
    }
}