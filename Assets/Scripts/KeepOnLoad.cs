using UnityEngine;

public class KeepOnLoad : MonoBehaviour
{
    public GameObject[] objectsKept;

    private void Start()
    {
        foreach (GameObject obj in objectsKept)
            DontDestroyOnLoad(obj);
    }
}
