using System.Collections;
using UnityEngine;

public class CameraTargetSingleton : MonoBehaviour
{
    public static CameraTargetSingleton Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}