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
        public Transform transform = new Transform();
        public T GetComponent<T>() where T : class { return null; }
        public T AddComponent<T>() where T : class, new() { return new T(); }
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
        public Transform transform = new Transform();

        public T GetComponent<T>() where T : class { return null; }
        public T AddComponent<T>() where T : class, new() { return new T(); }

        public static GameObject Instantiate(GameObject original, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            return new GameObject();
        }

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
    public sealed class HideInInspector : System.Attribute { }

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

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
    }

    public class Transform : Object
    {
        public Vector3 position { get; set; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }

    public class SpriteRenderer : MonoBehaviour
    {
        public Color color { get; set; }
    }

    public enum KeyCode
    {
        None = 0,
        LeftArrow, DownArrow, UpArrow, RightArrow,
        A, S, W, D,
        Return, Space, RightShift, LeftShift
    }

    public static class Input
    {
        public static bool GetKeyDown(KeyCode key) { return false; }
    }

    public static class Random
    {
        private static System.Random rng = new System.Random();
        public static int Range(int min, int max) { return rng.Next(min, max); }
        public static float Range(float min, float max) { return (float)(min + rng.NextDouble() * (max - min)); }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class SerializeField : System.Attribute { }
}
