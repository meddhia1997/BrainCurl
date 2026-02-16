using TMPro;
using UnityEngine;

public sealed class AttemptsHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text triesText;

    private IEventBus _bus;

    public void Init(IEventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<AttemptsChanged>(OnAttemptsChanged);
        Apply(0, 0);
    }

    private void OnDestroy()
    {
        if (_bus != null)
            _bus.Unsubscribe<AttemptsChanged>(OnAttemptsChanged);
    }

    private void OnAttemptsChanged(AttemptsChanged evt)
    {
        Apply(evt.TriesRemaining, evt.TriesMax);
    }

    private void Apply(int remaining, int max)
    {
        if (triesText != null)
            triesText.text = $"Tries: {remaining}/{max}";
    }
}
