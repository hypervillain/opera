using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class MelodyMarker : MonoBehaviour
{
    public int dev_elementIndex = 10;
    public GameObject player;
    public GameObject melodyMarkerPrefab;

    private ObiRope rope;
    private GameObject melodyMarker;

    void Start () {
        rope = GetComponent<ObiRope>();
    }
	void Update ()
    {
        int lastParticle = rope.elements[rope.elements.Count - dev_elementIndex].particle2;
        Vector3 lastPosition = rope.solver.positions[lastParticle];
        Vector3 worldPosition = rope.transform.TransformPoint(lastPosition);

        if (melodyMarker != null) {
            melodyMarker.transform.position = new Vector3(worldPosition.x, worldPosition.y, -0.1f);

        } else {
            melodyMarker = Instantiate(melodyMarkerPrefab, new Vector3(worldPosition.x, worldPosition.y, -0.1f), Quaternion.identity);
        }
	}
}