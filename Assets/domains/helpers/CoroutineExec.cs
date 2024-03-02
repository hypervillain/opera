using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExec
{
    public static Coroutine ExecuteAfterDelay(MonoBehaviour owner, float delay, Action action)
    {
        return owner.StartCoroutine(ExecuteAfterDelayRoutine(delay, action));
    }

    private static IEnumerator ExecuteAfterDelayRoutine(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static Coroutine ExecuteRepeatedly(MonoBehaviour owner, float initialDelay, float interval, Action action)
    {
        return owner.StartCoroutine(ExecuteRepeatedlyRoutine(initialDelay, interval, action));
    }

    private static IEnumerator ExecuteRepeatedlyRoutine(float initialDelay, float interval, Action action)
    {
        yield return new WaitForSeconds(initialDelay);
        while (true)
        {
            action?.Invoke();
            yield return new WaitForSeconds(interval);
        }
    }

    public static void StopCoroutine(MonoBehaviour owner, Coroutine coroutine)
    {
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
    }
}