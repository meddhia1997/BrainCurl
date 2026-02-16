using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    [SerializeField] private Button button;

    private int _cardId;
    private IEventBus _bus;
    private bool _isInteractable;

    public void Init(int cardId, IEventBus bus)
    {
        _cardId = cardId;
        _bus = bus;
        _isInteractable = true;

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
        if (!_isInteractable) return;
        _bus.Publish(new CardFlipRequested(_cardId));
    }

    public void SetInteractable(bool value)
    {
        _isInteractable = value;
        if (button != null) button.interactable = value;
    }
}
