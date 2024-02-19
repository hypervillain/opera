using UnityEngine;
using Obi;
using FMOD;


public class MelodyMarker : MonoBehaviour
{
    public float percentagePosition = 0.95f;
    public int dev_elementIndex = 10;
    public GameObject melodyMarkerPrefab;

    private ObiRope rope;
    private GameObject melodyMarker;

    void Start () {
        rope = GetComponent<ObiRope>();
    }
	void Update ()
    {
        int lastParticle = rope.elements[rope.elements.Count - dev_elementIndex].particle2;
        Vector3 worldPosition = rope.transform.TransformPoint(rope.solver.positions[lastParticle]);

        (_, worldPosition) = RopeHelpers.GetParticlePositionByRopeLengthPercentage(rope, percentagePosition);

        if (melodyMarker != null) {
            melodyMarker.transform.position = new Vector3(worldPosition.x, worldPosition.y, -0.1f);
        } else {
            melodyMarker = Instantiate(melodyMarkerPrefab, new Vector3(worldPosition.x, worldPosition.y, -0.1f), Quaternion.identity);
        }
	}
}