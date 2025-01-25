using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 15f;  // Gaz pedalı gücü
    [SerializeField] private float brakeForce = 7.5f;    // Fren gücü
    [SerializeField] private float maxSpeed = 100f;      // Maksimum hız
    [SerializeField] private float deceleration = 1f;   // Gazdan çekilince yavaşlama hızı
    [SerializeField] private float turnSpeed = 200f;    // Dönüş hızı

    public void SetInputs(float moveInput, float turnInput)
    {
        // Aracın ileri yönü (2D sahnede "transform.up")
        Vector2 forward = transform.up;

        // Gaz / Fren
        if (moveInput > 0)
        {
            // Gaz
            rb.AddForce(forward * moveInput * acceleration, ForceMode2D.Force);
        }
        else if (moveInput < 0)
        {
            // Fren
            rb.AddForce(forward * moveInput * brakeForce, ForceMode2D.Force);
        }
        else
        {
            // Gazdan tamamen çekildiyse, yavaşlama
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Maksimum hız kontrolü
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Dönüş (hız düşükse dönmeyi kısıtlaması için ufak bir eşik veriyoruz)
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float rotation = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }
    }

    // Inspector'da değerleri hatalı girersek kontrol amaçlı
    private void OnValidate()
    {
        acceleration = Mathf.Max(0, acceleration);
        brakeForce = Mathf.Max(0, brakeForce);
        maxSpeed = Mathf.Max(0, maxSpeed);
        deceleration = Mathf.Clamp(deceleration, 0f, 1f);
        turnSpeed = Mathf.Max(0, turnSpeed);
    }
}
