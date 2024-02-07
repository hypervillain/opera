using UnityEngine;

public class MoveWithArrows : MonoBehaviour
{
    public float speed;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Translate(moveX, moveY, 0f);
    }
}