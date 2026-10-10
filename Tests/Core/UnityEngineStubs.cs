using System.Collections;

namespace UnityEngine
{
    public class Object
    {
        public static void Destroy(Object target)
        {
            GameObject gameObject = target as GameObject;
            if (gameObject != null)
            {
                gameObject.Destroyed = true;
            }
        }
    }

    public class GameObject : Object
    {
        public bool Destroyed { get; set; }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new()
        {
            return new T();
        }
    }

    public class MonoBehaviour : Object
    {
        public GameObject gameObject = new GameObject();

        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return new Coroutine(routine);
        }

        protected void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                coroutine.Stopped = true;
            }
        }
    }

    public sealed class Coroutine
    {
        private readonly IEnumerator routine;

        public Coroutine(IEnumerator routine)
        {
            this.routine = routine;
        }

        public bool Stopped { get; set; }

        public bool MoveNext()
        {
            return !Stopped && routine.MoveNext();
        }
    }

    public sealed class WaitForSeconds
    {
        public WaitForSeconds(float seconds)
        {
            Seconds = seconds;
        }

        public float Seconds { get; private set; }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : System.Attribute
    {
        public CreateAssetMenuAttribute(string fileName = null)
        {
            this.fileName = fileName;
        }

        public string fileName { get; set; }
        public string menuName { get; set; }
        public int order { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class HeaderAttribute : System.Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class TooltipAttribute : System.Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RangeAttribute : System.Attribute
    {
        public RangeAttribute(float minimum, float maximum) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class SerializeField : System.Attribute { }
}
