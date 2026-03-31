using UnityEngine;
using TMPro;

public class BoatSpeedMeter : MonoBehaviour
{
    [SerializeField] private Rigidbody targetRigidbody;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Display")]
    [SerializeField] private string prefix = "Speed: ";
    [SerializeField] private string suffix = " km/h";
    [SerializeField] private int decimalPlaces = 2;

    public float SpeedKmh { get; private set; }

    private void Update()
    {
        if (targetRigidbody == null)
            return;

        float speedMS = targetRigidbody.linearVelocity.magnitude;
        // Unity cũ thì dùng targetRigidbody.velocity.magnitude;

        SpeedKmh = speedMS * 3.6f;

        if (speedText != null)
        {
            speedText.text = prefix + SpeedKmh.ToString($"F{decimalPlaces}") + suffix;
        }
    }
}