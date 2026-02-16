using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    [SerializeField] private Button button;

    private int _cardId;
    private IEventBus _bus;

    public void Init(int cardId, IEventBus bus)
    {
        _cardId = cardId;
        _bus = bus;

        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
            button.interactable = true;
        }

        _bus.Subscribe<CardInteractableChanged>(OnInteractableChanged);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);

        if (_bus != null)
            _bus.Unsubscribe<CardInteractableChanged>(OnInteractableChanged);
    }

    private void OnClicked()
    {
        _bus.Publish(new CardFlipRequested(_cardId));
    }

    private void OnInteractableChanged(CardInteractableChanged evt)
    {
        if (evt.CardId != _cardId) return;
        if (button != null) button.interactable = evt.Interactable;
    }
}
