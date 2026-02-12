using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    private CharacterController _characterController;

    public float MovementSpeed = 10f;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }
    public void Move(Vector2 movementVector)
    {
        Vector2 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        Debug.Log("Forward " + transform.forward + " " +"Right " + transform.right);
        move = move * MovementSpeed * Time.deltaTime;
        _characterController.Move(move);
    }
}
