using Obi;
using UnityEngine;
using System.Collections.Generic;

public class KnotEvent
{
    public float timing;
    public GameObject[] Instances;
    public int uniqueBeatNumber;

    public KnotEvent(float timing, int uniqueBeatNumber, GameObject[] instances)
    {
        this.timing = timing;
        this.uniqueBeatNumber = uniqueBeatNumber;
        this.Instances = instances;
    }
}

public class TempoKnotManager: MonoBehaviour
{
    private float _elapsedTime;
    private BeatTracker _beatTracker;
    private List<KnotEvent> _knotEvents = new List<KnotEvent>();

    /** All these params should be fetched from MelodySpawner or a shared object */
    public ObiRope[] _ropes;
    public GameObject knotPrefab;
    public int _noteFallInTimeInBeats;


    void Awake()
    {
        ActManager.OnBeatTrackerUpdate += OnBeatTrackerUpdate;
        ActManager.OnElapsedTimeChanged += OnElapsedTimeChanged;
    }

    private void OnElapsedTimeChanged(float elapsedTime)
    {
        _elapsedTime = elapsedTime;
    }
    private void OnBeatTrackerUpdate(BeatTracker beatTracker)
    {
        _beatTracker = beatTracker;
        List<KnotEvent> knotsToRemove = new List<KnotEvent>();
        foreach (KnotEvent knotEvent in _knotEvents)
        {
            int difference = knotEvent.uniqueBeatNumber - beatTracker.uniqueBeatNumber;

            if (difference < 0)
            {
                knotsToRemove.Add(knotEvent);
            }
            else
            {
                knotEvent.timing = beatTracker.timing + (difference * beatTracker.secondsPerBeat);
            }
        }

        CreateKnot(beatTracker);

        // Deez
        ReleaseKnots(knotsToRemove);
    }

    public void Update()
    {
        if (_beatTracker == null)
        {
            Debug.Log("_beatTracker is not initialised.");
            return;
        }
        // No
        float melodyMarkerPositionPercentage = 0.9f;
        // no no
        RopeHelpers.RopeDirection ropeDirection = RopeHelpers.RopeDirection.Down;

        List<KnotEvent> knotsToRemove = new List<KnotEvent>();
        foreach (KnotEvent knotEvent in _knotEvents)
        {
            float fallProgress = (_elapsedTime - (knotEvent.timing - _beatTracker.secondsPerMeasure)) / _beatTracker.secondsPerMeasure;
            float percentagePosition = fallProgress * melodyMarkerPositionPercentage;
            if (percentagePosition >= 0.9)
            {
                Debug.Log($"delete knot event per percentage");
                knotsToRemove.Add(knotEvent);
            }
            for (int i = 0; i < knotEvent.Instances.Length; i++)
            {
                if (_ropes[i] != null)
                {
                    var position = RopeHelpers.GetParticlePositionByRopeLengthPercentage(_ropes[i], percentagePosition, ropeDirection).Item2;
                    knotEvent.Instances[i].transform.position = position;
                }
            }
        }

        ReleaseKnots(knotsToRemove);

    }

    private void CreateKnot(BeatTracker beatTracker)
    {
        GameObject[] instances = new GameObject[_ropes.Length];

        for (int i = 0; i < _ropes.Length; i++)
        {
            instances[i] = Instantiate(knotPrefab, _ropes[i].transform.position, Quaternion.identity, _ropes[i].transform);
        }
        _knotEvents.Add(new KnotEvent(beatTracker.timing + _noteFallInTimeInBeats * beatTracker.secondsPerBeat, beatTracker.uniqueBeatNumber + _noteFallInTimeInBeats, instances));
    }

    private void ReleaseKnot(KnotEvent knotEvent)
    {
        foreach (GameObject instance in knotEvent.Instances)
        {
            if (instance != null)
            {
                GameObject.Destroy(instance);
            }
        }
    }

    private void ReleaseKnots(List<KnotEvent> knotsToRemove)
    {
        foreach (KnotEvent knotEvent in knotsToRemove)
        {
            ReleaseKnot(knotEvent);
            _knotEvents.Remove(knotEvent);
        }
    }
}