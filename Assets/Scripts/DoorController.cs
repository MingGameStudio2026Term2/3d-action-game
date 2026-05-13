using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform door; // Assign the door child in the inspector
    public float openAngle = 90f;
    public float openSpeed = 2f;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        if (door == null)
        {
            Debug.LogError("DoorController: Door Transform not assigned.");
            return;
        }
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, 0, openAngle);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }

    void Update()
    {
        if (door == null) return;
        if (isOpen)
        {
            door.localRotation = Quaternion.Slerp(door.localRotation, openRotation, Time.deltaTime * openSpeed);
        }
        else
        {
            door.localRotation = Quaternion.Slerp(door.localRotation, closedRotation, Time.deltaTime * openSpeed);
        }
    }
}
