using UnityEngine;

public class CarTest : MonoBehaviour
{
    [SerializeField] private Car car;

    private float moveInput; // Ýleri/geri girdisi (W/S)
    private float turnInput; // Dönüþ girdisi (A/D)

    private void Update()
    {
        // Kullanýcý girdilerini al
        moveInput = Input.GetAxis("Vertical"); // W/S veya Yukarý/Aþaðý ok tuþlarý
        turnInput = Input.GetAxis("Horizontal"); // A/D veya Sol/Sað ok tuþlarý
        car.SetInputs(moveInput, turnInput);
    }
}