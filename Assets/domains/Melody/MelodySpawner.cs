using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class MelodySpawner : MonoBehaviour
{
    [SerializeField] private ObiRope[] ropes;
    [SerializeField] private GameObject melodyEventPrefab;
    [SerializeField] private GameObject melodyMarkerPrefab;

    private GameObject[] _melodyMarkers;
    private List<MelodyEvent> _melodyEvents = new List<MelodyEvent>();
    private float _elapsedTime;
    private float _noteFallInElapsedTime;

    private (int measure, int beat, float timing) beatInfo;

    void Awake ()
    {
        ActManager.OnElapsedTimeChanged += OnElapsedTimeChanged;
        ActManager.OnMelodyReady += OnMelodyReady;
        ActManager.OnBPMReady += OnBPMReady;

        _melodyMarkers = new GameObject[ropes.Length];
    }
    private void OnElapsedTimeChanged(float elapsedTime)
    {
        _elapsedTime = elapsedTime;
    }

    private void OnBPMReady(int bpm, int signature)
    {
        _noteFallInElapsedTime = BeatHelpers.GetSecondsPerBeatFromBPM(bpm) * ActManager.Instance.CurrentSceneData.timeFactor;
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
        UpdateMelodyMarkers();

        if ( _melodyEvents.Count == 0)
        {
            return;
        }

        foreach (MelodyEvent melodyEvent in _melodyEvents)
        {
            if (_elapsedTime <= (melodyEvent.timing + 0.150f) && _elapsedTime >= (melodyEvent.timing - _noteFallInElapsedTime))
            {
                SpawnOrUpdateMelodyEvent(melodyEvent);
            }
            else {
                ReleaseMelodyEvent(melodyEvent);
            }
        }
    }

    private void UpdateMelodyMarkers()
    {
        for (int i = 0; i < ropes.Length; i++) {
            var worldPosition = RopeHelpers.GetParticlePositionByRopeLengthPercentage(
                ropes[i],
                ActManager.Instance.CurrentSceneData.tempoMarkerPositionPercentage,
                ActManager.Instance.CurrentSceneData.ropeDirection
            ).Item2;
            if (_melodyMarkers[i] != null) {
                _melodyMarkers[i].transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
            } else {
                _melodyMarkers[i] = Instantiate(melodyMarkerPrefab, new Vector3(worldPosition.x, worldPosition.y, 0), Quaternion.identity);
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
                var position = RopeHelpers.GetParticlePositionByRopeLengthPercentage(targetRope, 0, ActManager.Instance.CurrentSceneData.ropeDirection).Item2;
                GameObject instance = Instantiate(melodyEventPrefab, position, Quaternion.identity);
                instanceAtRopeIndex.Instance = instance;
            }

            else
            {
                float fallProgress = (_elapsedTime - (melodyEvent.timing - _noteFallInElapsedTime)) / _noteFallInElapsedTime;
                float percentagePosition = fallProgress * ActManager.Instance.CurrentSceneData.tempoMarkerPositionPercentage;

                var position = RopeHelpers.GetParticlePositionByRopeLengthPercentage(
                    targetRope,
                    percentagePosition,
                    ActManager.Instance.CurrentSceneData.ropeDirection
                ).Item2;
                instanceAtRopeIndex.Instance.transform.position = position;
            }
        }
    }

    private void ReleaseMelodyEvent(MelodyEvent melodyEvent)
    {
        foreach (MelodyInstanceAtRopeIndex instanceAtRopeIndex in melodyEvent.instancesAtRopeIndex)
        {
            if (instanceAtRopeIndex.Instance != null)
            {
                Destroy(instanceAtRopeIndex.Instance);
                instanceAtRopeIndex.Instance = null;
            }
        }
    }
}