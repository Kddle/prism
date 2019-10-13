using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSCharacterController : MonoBehaviour
{
    public float WalkSpeed = 5f;
    public float JumpForce = 0f;

    private float Speed = 0f;

    [SerializeField]
    private float Sensitivity = 3f;

    [SerializeField]
    private string HorizontalAxisName = "Horizontal";
    [SerializeField]
    private string VerticalAxisName = "Vertical";
    [SerializeField]
    private string XLookAxisName = "Mouse X";
    [SerializeField]
    private string YLookAxisName = "Mouse Y";
    [SerializeField]
    private string JumpButton = "Jump";

    private Vector3 Velocity = Vector3.zero;
    private Vector3 BodyRotation = Vector3.zero;
    private Quaternion CameraRotation;

    private Rigidbody rb;
    private Camera PlayerCamera;

    private bool CanJump = false;
    private bool isGrounded = true;

    float XRotation = 0f;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PlayerCamera = GetComponentInChildren<Camera>();

        if (PlayerCamera == null)
        {
            Debug.LogError("No Player Camera found on " + this.transform.name);
        }

        Speed = WalkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Body Movement
        float hValue = Input.GetAxisRaw(HorizontalAxisName);
        float vValue = Input.GetAxisRaw(VerticalAxisName);

        Vector3 HorizontalMove = transform.right * hValue;
        Vector3 VerticalMove = transform.forward * vValue;

        Velocity = (HorizontalMove + VerticalMove).normalized * Speed;

        // Body Rotation
        float YRotation = Input.GetAxisRaw(XLookAxisName);
        BodyRotation = new Vector3(0f, YRotation, 0f) * Sensitivity;

        // Camera Rotation
        XRotation += Input.GetAxisRaw(YLookAxisName) * Sensitivity;
        XRotation = Mathf.Clamp(XRotation, -75f, 75f);

        CameraRotation = Quaternion.Euler(-XRotation, 0f, 0f);

        if (CanJump)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1.5f))
                isGrounded = true;
            else
                isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        if (Velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + Velocity * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(BodyRotation));

        if (PlayerCamera != null)
        {
            PlayerCamera.transform.localRotation = CameraRotation;
        }

        if (isGrounded && Input.GetButtonDown(JumpButton))
        {
            rb.AddForceAtPosition(new Vector3(0f, JumpForce, 0f), transform.position, ForceMode.Impulse);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        Speed = newSpeed;
    }

    public void ResetSpeed()
    {
        Speed = WalkSpeed;
    }

    public void SetJump(bool canJump, float jumpForce)
    {
        CanJump = canJump;
        JumpForce = jumpForce;
    }
}

