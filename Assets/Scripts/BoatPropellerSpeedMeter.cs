using UnityEngine;
using TMPro;

public class BoatPropellerSpeedMeter : MonoBehaviour
{
    [SerializeField] private USVEngine targetEngine;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI leftPropellerText;
    [SerializeField] private TextMeshProUGUI rightPropellerText;

    [Header("Display")]
    [SerializeField] private int decimalPlaces = 2;

    public float LeftPropellerRps { get; private set; }
    public float RightPropellerRps { get; private set; }

    private void Update()
    {
        if (targetEngine == null)
            return;

        LeftPropellerRps = CalculatePropellerRps(targetEngine.leftEngine);
        RightPropellerRps = CalculatePropellerRps(targetEngine.rightEngine);

        if (leftPropellerText != null)
        {
            leftPropellerText.text = LeftPropellerRps.ToString($"F{decimalPlaces}");
        }

        if (rightPropellerText != null)
        {
            rightPropellerText.text = RightPropellerRps.ToString($"F{decimalPlaces}");
        }
    }

    private float CalculatePropellerRps(USVEngine.Engine engine)
    {
        if (engine == null)
            return 0f;

        return engine.currentRPM / 60f;
    }
}