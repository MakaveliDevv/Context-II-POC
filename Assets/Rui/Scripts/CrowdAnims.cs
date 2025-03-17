using UnityEngine;

public class CrowdAnims : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private string speedParameter = "Speed"; // Animator parameter name
    [SerializeField] private float speed; // Visible in Inspector

    private Vector3 lastPosition;

    private void Start()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        CalculateSpeed();
        UpdateAnimator();
    }

    private void CalculateSpeed()
    {
        // Distance traveled since last frame
        speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position; // Store new position
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat(speedParameter, speed);
        }
    }
}
