using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class MelodySpawner : MonoBehaviour
{
    public int noteFallInTimeInBeats = 4;
    [SerializeField] private ObiRope[] ropes;
    [SerializeField] private GameObject melodyEventPrefab;

    private MelodyMarker[] _melodyMarkers;
    private List<MelodyEvent> _melodyEvents = new List<MelodyEvent>();
    private float _elapsedTime;
    private float _noteFallInElapsedTime;

    void Start ()
    {
        ActManager.OnElapsedTimeChanged += HandleElapsedTimeChanged;
        ActManager.OnMelodyReady += OnMelodyReady;
        ActManager.OnBPMReady += OnBPMReady;
        _melodyMarkers = new MelodyMarker[ropes.Length];
        for (int i = 0; i < ropes.Length; i++) {
            _melodyMarkers[i] = ropes[i].GetComponent<MelodyMarker>();
            if (_melodyMarkers[i] == null) {
                throw new Exception("Rope is missing MelodyMarker component");
            }
        }
    }
    private void HandleElapsedTimeChanged(float elapsedTime)
    {
        _elapsedTime = elapsedTime;
    }

    private void OnBPMReady(int bpm, int signature)
    {
        _noteFallInElapsedTime = 60f / bpm * noteFallInTimeInBeats;
        Debug.Log($"Note Fall in elapsed time {_noteFallInElapsedTime}");
    }

    private void OnMelodyReady(List<NoteEvent> noteEvents)
    {
        _melodyEvents.Clear();
        for (int i = 0; i < noteEvents.Count; i++)
        {
            _melodyEvents.Add(new MelodyEvent 
            { 
                index = i,
                timing = noteEvents[i].timing, 
                instancesAtRopeIndex = Enumerable
                    .Range(0, ropes.Length)
                    .Select(ropeIndex => new MelodyInstanceAtRopeIndex { RopeIndex = ropeIndex, Instance = null })
                    .ToList(),
                length = noteEvents[i].length
            });
        }
    }

	void Update ()
    {
        if (_melodyEvents == null || _melodyEvents.Count == 0)
        {
            return;
        }

        foreach (MelodyEvent melodyEvent in _melodyEvents)
        {
            if (_elapsedTime >= (melodyEvent.timing - 0.150f) && _elapsedTime <= (melodyEvent.timing + _noteFallInElapsedTime))
            {
                SpawnOrUpdateMelodyEvent(melodyEvent);
            }
            else {
                ReleaseMelodyEvent(melodyEvent);
            }
        }
    }

    private void SpawnOrUpdateMelodyEvent(MelodyEvent melodyEvent)
    {
        foreach (MelodyInstanceAtRopeIndex instanceAtRopeIndex in melodyEvent.instancesAtRopeIndex)
        {
            var targetRope = ropes[instanceAtRopeIndex.RopeIndex];
            if (targetRope == null)
            {
                throw new Exception("Target rope by index does not exist.");
            }

            if (instanceAtRopeIndex.Instance == null)
            {
                GameObject instance = Instantiate(melodyEventPrefab, /** temp */targetRope.transform.position, Quaternion.identity);
                instanceAtRopeIndex.Instance = instance;
            }

            else
            {
                var position = instanceAtRopeIndex.Instance.transform.position;
                instanceAtRopeIndex.Instance.transform.position = new Vector3(position.x, position.y - 0.01f, position.z);
            }
        }
    }

    private void CalculateRelativeRopePosition(ObiRope rope, MelodyEvent melodyEvent)
    {
        // TODO
    }

    private void ReleaseMelodyEvent(MelodyEvent melodyEvent)
    {
        // TODO
    }
}