// ReSharper disable once CheckNamespace

namespace UnityEngine;

#pragma warning disable
public class MonoBehaviour {
    // ReSharper disable once InconsistentNaming
    public GameObject gameObject => null!;

    public static T? FindAnyObjectByType<T>(FindObjectsInactive inactive) where T : MonoBehaviour => null;
    public static void DontDestroyOnLoad(MonoBehaviour behaviour) { }
    public static void Destroy(MonoBehaviour behaviour) { }
    public static void Destroy(GameObject gameObject) { }
}
