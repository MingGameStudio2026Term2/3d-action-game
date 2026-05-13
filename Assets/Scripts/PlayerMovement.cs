using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    // Camera follow settings
    public Vector3 cameraOffset = new Vector3(0f, 2f, -5f);
    public float impactForce = 2000f;
    public float rayDistance = 100f;
    public LayerMask shootMask = ~0; // Default to everything

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Automatically ignore the Player layer if it exists
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            shootMask = ~(1 << playerLayer);
            Debug.Log($"Ignoring Player layer {playerLayer} in shootMask");
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate player (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Shooting
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("cameraTransform is not assigned!");
            return;
        }
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1.0f);
        Debug.Log($"Shooting with LayerMask: {shootMask.value}");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance, shootMask))
        {
            // Ignore self-collision
            if (hit.collider.gameObject == this.gameObject)
            {
                Debug.Log("Raycast hit self, ignoring.");
                return;
            }
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            // Apply force if the object has a rigidbody
            Rigidbody hitRb = hit.collider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddForce(ray.direction * impactForce);
                Debug.Log("Applied force to " + hitRb.gameObject.name);
            }
            else
            {
                Debug.Log("Hit object has no Rigidbody");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object");
        }
    }

    void FixedUpdate()
    {
        // WASD movement relative to camera
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector3 move = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            move = (forward * moveZ + right * moveX).normalized * moveSpeed;
        }
        else
        {
            move = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;
        }

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    void LateUpdate()
    {
        // Camera follows from behind the player and looks at the player
        if (cameraTransform != null)
        {
            // Calculate desired camera position (behind and above the player)
            Vector3 desiredPosition = transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * cameraOffset;
            cameraTransform.position = desiredPosition;
            // Make the camera look at the player (optionally add an offset for better framing)
            Vector3 lookTarget = transform.position + Vector3.up * 1.5f; // 1.5f to look at upper body/head
            cameraTransform.LookAt(lookTarget);
        }
    }
}
