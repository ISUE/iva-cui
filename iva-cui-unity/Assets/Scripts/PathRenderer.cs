using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    private List<GameObject> paths;

    private static PathRenderer instance;
    public float displayInterval = 0.03f; // Time between enabling each child
    private Coroutine pathCoroutine;

    private static List<Transform> firstFiveTransforms = new List<Transform>();

    private static List<Vector3> vector3s = new List<Vector3>();
    private static List<Quaternion> quaternions = new List<Quaternion>();

    private static int numChildren;

    private void InitializePaths()
    {
        paths = new List<GameObject>();

        foreach (Transform path in transform)
        {
            paths.Add(path.gameObject);
        }
    }

    private void Awake()
    {
        instance = this;
        InitializePaths();
    }

    private void Start()
    {
        DisableAllPaths();
    }

    public static void DisableAllPaths()
    {
        Debug.Log("Disabling all paths rendering");
        foreach (GameObject path in instance.paths)
        {
            path.SetActive(false);
        }
    }

    public static void EnablePathAt(int idx)
    {
        if (idx < 0 || idx >= instance.paths.Count)
        {
            Debug.LogError("Invalid path index");
            return;
        }

        if (instance.pathCoroutine != null)
        {
            instance.StopCoroutine(instance.pathCoroutine);
        }

        // Disable all other paths and start showing the children of the selected path
        DisableAllPaths();
        Debug.Log($"Enabling path {idx} rendering");
        instance.paths[idx].SetActive(true);

        // Clear the list of active child objects and populate it with the children of the selected path
        int childIdx = 0;
        vector3s.Clear();
        quaternions.Clear();
        firstFiveTransforms.Clear();
        foreach (Transform child in instance.paths[idx].transform)
        {
            vector3s.Add(child.position);
            quaternions.Add(child.rotation);
            if (childIdx < 5)
            {
                firstFiveTransforms.Add(child);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            childIdx++;
        }
        numChildren = vector3s.Count;
        instance.pathCoroutine = instance.StartCoroutine(instance.ShowChildObjectsSequentially());
    }

    private IEnumerator ShowChildObjectsSequentially()
    {
        int[] indices = { 0, 1, 2, 3, 4 };
        int i = 0;

        while (true)
        {
            for (int k = 0; k < indices.Length; k++)
            {
                int j = (i + indices[k]) % numChildren;
                firstFiveTransforms[indices[k]].transform.position = vector3s[j];
                firstFiveTransforms[indices[k]].transform.rotation = quaternions[j];
            }

            i = (i + 1) % numChildren;

            yield return new WaitForSeconds(displayInterval);
        }
    }
}