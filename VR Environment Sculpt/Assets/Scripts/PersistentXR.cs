using UnityEngine;

public class PersistentXR : MonoBehaviour
{
    private static PersistentXR instance;

    void Awake()
    {
        // If another PersistentXR already exists, destroy this duplicate.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
