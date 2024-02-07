using UnityEngine;
using System;

public class Metronome : MonoBehaviour
{
    public static event Action OnCubeClicked;

    void Start()
    {
        CentralAudioSource.OnAudioBeat += OnAudioBeat;
    }

    void OnAudioBeat(int x, int y)
    {
        transform.Rotate(new Vector3(0, 45, 0));
    }

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
        CentralAudioSource.OnAudioBeat -= OnAudioBeat;
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 45, 0));
    }
}