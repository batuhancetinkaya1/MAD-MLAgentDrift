using UnityEngine;

public class CarTest : MonoBehaviour
{
    [SerializeField] private Car car;

    private float moveInput; // �leri/geri girdisi (W/S)
    private float turnInput; // D�n�� girdisi (A/D)

    private void Update()
    {
        // Kullan�c� girdilerini al
        moveInput = Input.GetAxis("Vertical"); // W/S veya Yukar�/A�a�� ok tu�lar�
        turnInput = Input.GetAxis("Horizontal"); // A/D veya Sol/Sa� ok tu�lar�
        car.SetInputs(moveInput, turnInput);
    }
}