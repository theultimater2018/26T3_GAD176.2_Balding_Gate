using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5f;

    public Rigidbody2D playerRB2D;

    private Vector2 moveInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        playerRB2D.linearVelocity = moveInput * speed;
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
