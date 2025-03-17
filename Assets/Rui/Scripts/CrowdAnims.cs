using UnityEngine;

public class CrowdAnims : MonoBehaviour
{
    [SerializeField] private Animator animator; // Assign this in the Inspector
    [SerializeField] private string speedParameter = "Speed"; // Name of the Animator parameter
    [SerializeField] private float speed; // Visible in Inspector

    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        CalculateSpeed();
        UpdateAnimator();
    }

    private void CalculateSpeed()
    {
        // Calculate speed based on position change
        speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat(speedParameter, speed);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned in SpeedTracker on " + gameObject.name);
        }
    }
}
