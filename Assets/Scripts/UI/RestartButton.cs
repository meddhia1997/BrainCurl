using UnityEngine;
using UnityEngine.UI;
using Events;

public sealed class RestartButton : MonoBehaviour
{
    [SerializeField] private Button button;

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
        _bus.Publish(new RestartRequested());
    }
}
