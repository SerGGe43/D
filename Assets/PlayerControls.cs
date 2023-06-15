using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float Speed = 2;

    private Rigidbody2D componentRigidbody;

    private void Start()
    {
        componentRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        componentRigidbody.velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) componentRigidbody.velocity += Vector2.left * Speed;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) componentRigidbody.velocity += Vector2.right * Speed;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) componentRigidbody.velocity += Vector2.up * Speed;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) componentRigidbody.velocity += Vector2.down * Speed;
    }
}