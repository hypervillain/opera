using UnityEngine;

public class Object2DInputControl : MonoBehaviour
{
    public float speed = 25f;

    void Update()
    {
        // Get input values
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        // Translate the object based on input
        transform.Translate(moveX, moveY, 0f);
    }
}