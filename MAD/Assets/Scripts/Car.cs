using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 15f; 
    [SerializeField] private float brakeForce = 7.5f;   
    [SerializeField] public float maxSpeed = 100f;    
    [SerializeField] private float deceleration = 1f; 
    [SerializeField] public float turnSpeed = 200f;  

    private float moveInput; 
    private float turnInput; 

    public void SetInputs(float move, float turn)
    {
        moveInput = move;
        turnInput = turn;
    }

    private void FixedUpdate()
    {
        // H�zlanma ve frenleme
        Vector2 forward = transform.up; 

        if (moveInput > 0)
        {
            // Gaz pedal�yla h�zlanma
            rb.AddForce(forward * moveInput * acceleration, ForceMode2D.Force);
        }
        else if (moveInput < 0)
        {
            // Frenleme veya geri gitme
            rb.AddForce(forward * moveInput * brakeForce, ForceMode2D.Force);
        }

        // Maksimum h�z s�n�r�
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // D�n�� hareketi
        if (rb.linearVelocity.magnitude > 0.1f) 
        {
            float rotation = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }

        // If no gas or brake input
        if (Mathf.Approximately(moveInput, 0f))
        {
            // Move the velocity towards zero
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero,
                                              deceleration * Time.fixedDeltaTime);
        }

    }
    public void ResetPhysics()
    {
        moveInput = 0f;
        turnInput = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}