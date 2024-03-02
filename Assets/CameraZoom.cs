using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minZoomDistance = -1.111111e+12f;
    public float maxZoomDistance = 1.111111e+12f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.z += scroll * zoomSpeed;
        pos.z = Mathf.Clamp(pos.z, minZoomDistance, maxZoomDistance);
        transform.position = pos;
    }
}