using UnityEngine;

public class CarTest : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 15f; // Gaz pedalý gücü
    [SerializeField] private float brakeForce = 7.5f;   // Fren gücü
    [SerializeField] private float maxSpeed = 100f;    // Maksimum hýz
    [SerializeField] private float deceleration = 1f; // Gazdan çekilince yavaþlama hýzý
    [SerializeField] private float turnSpeed = 200f;  // Dönüþ hýzý

    private float moveInput; // Ýleri/geri girdisi (W/S)
    private float turnInput; // Dönüþ girdisi (A/D)

    private void Update()
    {
        // Kullanýcý girdilerini al
        moveInput = Input.GetAxis("Vertical"); // W/S veya Yukarý/Aþaðý ok tuþlarý
        turnInput = Input.GetAxis("Horizontal"); // A/D veya Sol/Sað ok tuþlarý
    }

    private void FixedUpdate()
    {
        // Hýzlanma ve frenleme
        Vector2 forward = transform.up; // Aracýn ileri yönü

        if (moveInput > 0)
        {
            // Gaz pedalýyla hýzlanma
            rb.AddForce(forward * moveInput * acceleration, ForceMode2D.Force);
        }
        else if (moveInput < 0)
        {
            // Frenleme veya geri gitme
            rb.AddForce(forward * moveInput * brakeForce, ForceMode2D.Force);
        }

        // Maksimum hýz sýnýrý
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Dönüþ hareketi
        if (rb.linearVelocity.magnitude > 0.1f) // Yeterli hýz varsa dönebilir
        {
            float rotation = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }
    }

    private void OnValidate()
    {
        // Deðerlerin pozitif olduðundan emin olun
        acceleration = Mathf.Max(0, acceleration);
        brakeForce = Mathf.Max(0, brakeForce);
        maxSpeed = Mathf.Max(0, maxSpeed);
        deceleration = Mathf.Clamp(deceleration, 0f, 1f);
        turnSpeed = Mathf.Max(0, turnSpeed);
    }
}
