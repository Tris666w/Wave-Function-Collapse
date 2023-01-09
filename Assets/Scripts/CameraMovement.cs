using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private float _sprintScale = 2f;
    private Rigidbody _rigidbody;
    private Vector2 _turn;


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
    }

    void Update()
    {
        //Move the camera
        Vector3 velocity = Vector3.zero;

        if (Input.GetAxis("Horizontal") != 0)
        {
            velocity += transform.right * (Input.GetAxis("Horizontal") * _movementSpeed);
        }
        if (Input.GetAxis("Depth") != 0)
        {
            velocity += transform.forward * (Input.GetAxis("Depth") * _movementSpeed);
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            velocity += transform.up * (Input.GetAxis("Vertical") * _movementSpeed);
        }
        if (Input.GetAxisRaw("Sprint") != 0)
        {
            velocity *= _sprintScale;
        }

        _rigidbody.velocity = velocity;



        if (Input.GetAxisRaw("EnableCameraRotation") == 0)
            return;

        //Rotate the camera
        _turn.x += Input.GetAxis("Mouse X");
        _turn.y += Input.GetAxis("Mouse Y");
        transform.localRotation = Quaternion.Euler(-_turn.y, _turn.x, 0);
    }


}
