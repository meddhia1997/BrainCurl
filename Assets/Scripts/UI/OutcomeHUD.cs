using TMPro;
using UnityEngine;

public sealed class OutcomeHUD : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;

    private IEventBus _bus;

    public void Init(IEventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<GameEnded>(OnGameEnded);

        if (root != null) root.SetActive(false);
        if (messageText != null) messageText.text = "";
    }

    private void OnDestroy()
    {
        if (_bus != null)
            _bus.Unsubscribe<GameEnded>(OnGameEnded);
    }

    private void OnGameEnded(GameEnded evt)
    {
        if (root != null) root.SetActive(true);
        if (messageText != null) messageText.text = evt.IsWin ? "VICTORY!" : "DEFEAT!";
    }
}
