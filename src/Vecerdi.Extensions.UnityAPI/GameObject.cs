// ReSharper disable once CheckNamespace
namespace UnityEngine;

public class GameObject {
    public GameObject(string name) { }

#pragma warning disable CA1822
    public T AddComponent<T>() where T : MonoBehaviour => null!;
    public object AddComponent(Type componentType) => null!;
#pragma warning restore CA1822
}
