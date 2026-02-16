using UnityEngine;
using UnityEngine.UI;

public sealed class LayoutButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private LayoutPresetSO preset;

    private IEventBus _bus;

    public void Init(IEventBus bus)
    {
        _bus = bus;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_bus == null) return;
        if (preset == null) return;

        _bus.Publish(new LayoutChangeRequested(preset));
    }
}
