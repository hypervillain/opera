using UnityEngine;
using System;

public class Metronome : MonoBehaviour
{
    public static event Action OnCubeClicked;
    void OnEnable()
    {
        // CentralAudioSource.OnAudioBeat += RotateCube;
    }

    void OnMouseDown()
    {
        OnCubeClicked?.Invoke();
    }

    void OnDisable()
    {
        // CentralAudioSource.OnAudioBeat -= RotateCube;
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 45, 0));
    }
}