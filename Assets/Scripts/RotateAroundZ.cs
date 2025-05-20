using UnityEngine;

public class RotateAroundZ : MonoBehaviour
{
    public float speed = 400.0f;

    private RectTransform rectTransform = null;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rectTransform == null) return;
        rectTransform.Rotate(0, 0, speed * Time.deltaTime);
    }
}