using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public Transform cameraTransform;
    public float impactForce = 500f;
    public float rayDistance = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && cameraTransform != null)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                Debug.Log($"Shot hit: {hit.collider.gameObject.name}");
                Rigidbody hitRb = hit.collider.attachedRigidbody;
                if (hitRb != null)
                {
                    hitRb.AddForce(ray.direction * impactForce, ForceMode.Impulse);
                }
            }
            else
            {
                Debug.Log("Shot hit nothing.");
            }
        }
    }
}
