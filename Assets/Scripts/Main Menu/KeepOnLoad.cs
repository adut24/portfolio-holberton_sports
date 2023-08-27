using UnityEngine;

/// <summary>
/// Manages the objects kept between different scene.
/// </summary>
public class KeepOnLoad : MonoBehaviour
{
    public GameObject[] objectsKept;

    private void Start()
    {
        foreach (GameObject obj in objectsKept)
            DontDestroyOnLoad(obj);
    }
}
