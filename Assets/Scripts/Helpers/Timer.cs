using UnityEngine;
using System.Diagnostics;

public static class Timer
{
    static Stopwatch sw; 

    public static void Start()
    {
        sw ??= new Stopwatch();
        sw = Stopwatch.StartNew();
    }

    public static void Stop(bool message)
    {
        sw.Stop();
        if (message)
            UnityEngine.Debug.Log($"The process took: {sw.ElapsedMilliseconds} ms");
    }

    public static void Stop(bool message, string word)
    {
        sw.Stop();
        if (message)
            UnityEngine.Debug.Log($"The {word} took: {sw.ElapsedMilliseconds} ms");
    }

    public static long GetElapsedTime()
    {
        return sw.ElapsedMilliseconds;
    }

}
