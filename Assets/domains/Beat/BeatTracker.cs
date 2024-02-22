public class BeatTracker
{
    public int bpm;
    public int beat;
    public int measure;
    public int signature;
    public float timing;

    public int uniqueBeatNumber;
    public float secondsPerBeat;
    public float secondsPerMeasure;

    public BeatTracker(int bpm, int signature)
    {
        this.beat = 0;
        this.measure = 0;

        this.bpm = bpm;
        this.signature = signature;
        
        this.timing = 0;
        this.uniqueBeatNumber = 0;
        this.secondsPerBeat = BeatHelpers.GetSecondsPerBeatFromBPM(bpm);
        this.secondsPerMeasure = this.secondsPerBeat * signature;
    }

    public void OnBeatUpdate(int measure, int beat, float timing)
    {
        this.beat = beat;
        this.timing = timing;
        this.measure = measure;
        
        this.uniqueBeatNumber = BeatHelpers.CalculateUniqueBeatNumber(measure, beat, signature);
    }
}