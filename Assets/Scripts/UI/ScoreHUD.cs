using TMPro;
using UnityEngine;

public sealed class ScoreHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text deltaText; // optional

    private IEventBus _bus;

    public void Init(IEventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<ScoreChanged>(OnScoreChanged);

        // Initialize with zeros if UI exists (in case score service publishes later)
        Apply(0, 0, 0);
    }

    private void OnDestroy()
    {
        if (_bus != null)
            _bus.Unsubscribe<ScoreChanged>(OnScoreChanged);
    }

    private void OnScoreChanged(ScoreChanged evt)
    {
        Apply(evt.Score, evt.Combo, evt.Delta);
    }

    private void Apply(int score, int combo, int delta)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (comboText != null)
            comboText.text = combo > 0 ? $"Combo: x{combo}" : "Combo: -";

        if (deltaText != null)
        {
            if (delta == 0) deltaText.text = "";
            else deltaText.text = delta > 0 ? $"+{delta}" : $"{delta}";
        }
    }
}
