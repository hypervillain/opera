using UnityEngine;

public static class BeatHelpers
{
    public static float GetSecondsPerBeatFromBPM(int bpm)
    {
        return 60f / bpm;
    }

    public static int CalculateUniqueBeatNumber(int measure, int beat, int signature)
    {
        return (measure - 1) * signature + (beat - 1);
    }
}