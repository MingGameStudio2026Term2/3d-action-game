
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shootRange = 100f;
    public float impactForce = 500f;
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public LayerMask shootableLayers;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("PlayerShooting: No camera assigned and no main camera found.");
            }
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Left mouse button pressed (Input System)");
            Shoot();
        }
#endif
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Left mouse button pressed (Input Manager)");
            Shoot();
        }
    }

    void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerShooting: No camera assigned.");
            return;
        }

        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Use the camera's forward direction for a true center shot
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, shootRange, shootableLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"Hit: {hit.collider.name}");
            // Apply force if the object has a Rigidbody
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                // Apply force in the direction the shot is going
                rb.AddForce(ray.direction * impactForce, ForceMode.Impulse);
            }

            // Spawn impact effect
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
        else
        {
            Debug.Log("PlayerShooting: No target hit.");
        }
    }
}
