using UnityEngine;

public class WorldCompass : MonoBehaviour
{
    [Header("Gắn Object vào đây")]
    public Transform shipTransform;     // Kéo model Con Tàu của bạn vào ô này
    public RectTransform compassNeedle; // Kéo cái Kim La Bàn (UI) vào ô này

    [Header("Cài đặt độ mượt")]
    public float smoothSpeed = 5f;      // Tốc độ lướt của kim (tạo cảm giác la bàn nước)

    void Update()
    {
        // 1. Lấy góc xoay hiện tại của tàu so với trục thế giới (Trục Y trong 3D)
        // eulerAngles.y chính là độ lệch của tàu so với hướng thẳng (Vector3.forward)
        float shipWorldRotation = shipTransform.eulerAngles.y;

        // 2. Tính toán góc xoay cho kim UI (UI xoay quanh trục Z)
        // Dùng dấu trừ (-) để kim quay ngược lại hướng xoay của tàu
        Quaternion targetRotation = Quaternion.Euler(0, 0, -shipWorldRotation);

        // 3. Áp dụng xoay mượt (Lerp) để ra chất game hải chiến
        compassNeedle.localRotation = Quaternion.Lerp(
            compassNeedle.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}