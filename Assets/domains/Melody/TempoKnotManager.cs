using Obi;
using UnityEngine;
using System.Linq;
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

        if (ActManager.Instance.CurrentSceneData.beatNumbersToMarkTimeAt.Contains(beatTracker.beat))
        {
            CreateKnot(beatTracker);
        }

        // Deez
        ReleaseKnots(knotsToRemove);
    }

    public void Update()
    {
        if (_beatTracker == null || ActManager.Instance.CurrentSceneData == null)
        {
            return;
        }

        List<KnotEvent> knotsToRemove = new List<KnotEvent>();
        foreach (KnotEvent knotEvent in _knotEvents)
        {
            float timeSinceFallStarted = _elapsedTime - (knotEvent.timing - (ActManager.Instance.CurrentSceneData.timeFactor * _beatTracker.secondsPerBeat));
            float timeProgressRatio = timeSinceFallStarted / (ActManager.Instance.CurrentSceneData.timeFactor * _beatTracker.secondsPerBeat);

            float ropePositionPercentage = Mathf.Clamp01(timeProgressRatio) * ActManager.Instance.CurrentSceneData.tempoMarkerPositionPercentage;

            if (timeProgressRatio >= 1)
            {
                knotsToRemove.Add(knotEvent);
            }
            else
            {
                for (int i = 0; i < knotEvent.Instances.Length; i++)
                {
                    if (_ropes[i] != null)
                    {
                        var position = RopeHelpers.GetParticlePositionByRopeLengthPercentage(_ropes[i], ropePositionPercentage, ActManager.Instance.CurrentSceneData.ropeDirection).Item2;
                        knotEvent.Instances[i].transform.position = position;
                    }
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
        float knotTiming = beatTracker.timing + (ActManager.Instance.CurrentSceneData.timeFactor * beatTracker.secondsPerBeat);
        _knotEvents.Add(new KnotEvent(knotTiming, beatTracker.uniqueBeatNumber + ActManager.Instance.CurrentSceneData.timeFactor, instances));
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