using UnityEngine;

public class CarTest : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 15f; // Gaz pedal� g�c�
    [SerializeField] private float brakeForce = 7.5f;   // Fren g�c�
    [SerializeField] private float maxSpeed = 100f;    // Maksimum h�z
    [SerializeField] private float deceleration = 1f; // Gazdan �ekilince yava�lama h�z�
    [SerializeField] private float turnSpeed = 200f;  // D�n�� h�z�

    private float moveInput; // �leri/geri girdisi (W/S)
    private float turnInput; // D�n�� girdisi (A/D)

    private void Update()
    {
        // Kullan�c� girdilerini al
        moveInput = Input.GetAxis("Vertical"); // W/S veya Yukar�/A�a�� ok tu�lar�
        turnInput = Input.GetAxis("Horizontal"); // A/D veya Sol/Sa� ok tu�lar�
    }

    private void FixedUpdate()
    {
        // H�zlanma ve frenleme
        Vector2 forward = transform.up; // Arac�n ileri y�n�

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
        if (rb.linearVelocity.magnitude > 0.1f) // Yeterli h�z varsa d�nebilir
        {
            float rotation = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }
    }

    private void OnValidate()
    {
        // De�erlerin pozitif oldu�undan emin olun
        acceleration = Mathf.Max(0, acceleration);
        brakeForce = Mathf.Max(0, brakeForce);
        maxSpeed = Mathf.Max(0, maxSpeed);
        deceleration = Mathf.Clamp(deceleration, 0f, 1f);
        turnSpeed = Mathf.Max(0, turnSpeed);
    }
}
