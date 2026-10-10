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

    public static class Mathf
    {
        public static float Abs(float f) { return System.Math.Abs(f); }
        public static float Max(float a, float b) { return System.Math.Max(a, b); }
        public static float Min(float a, float b) { return System.Math.Min(a, b); }
        public static int FloorToInt(float f) { return (int)System.Math.Floor(f); }
        public static float Round(float f) { return (float)System.Math.Round(f); }
        public static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }

    public static class AudioSettings
    {
        private static double dsp = 100.0;
        public static double dspTime
        {
            get { return dsp; }
            set { dsp = value; }
        }
    }

    public class AudioSource : MonoBehaviour
    {
        public bool isPlaying { get; set; }
        public double scheduledTime { get; set; }
        public void PlayScheduled(double time) { scheduledTime = time; isPlaying = true; }
        public void Stop() { isPlaying = false; }
        public void Pause() { isPlaying = false; }
        public void UnPause() { isPlaying = true; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class SerializeField : System.Attribute { }
}
