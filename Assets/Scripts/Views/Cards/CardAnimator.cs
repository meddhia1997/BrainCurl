using UnityEngine;

public sealed class CardAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject backGO;
    [SerializeField] private GameObject faceGO;

    private int _cardId;
    private IEventBus _bus;

    private static readonly int FlipTrigger = Animator.StringToHash("Flip");

    public void Init(int cardId, IEventBus bus)
    {
        _cardId = cardId;
        _bus = bus;

        // default state: showing back
        if (backGO != null) backGO.SetActive(true);
        if (faceGO != null) faceGO.SetActive(false);

        _bus.Subscribe<CardFlipStarted>(OnFlipStarted);
    }

    private void OnDestroy()
    {
        if (_bus != null)
            _bus.Unsubscribe<CardFlipStarted>(OnFlipStarted);
    }

    private void OnFlipStarted(CardFlipStarted evt)
    {
        if (evt.CardId != _cardId) return;

        animator.ResetTrigger(FlipTrigger);
        animator.SetTrigger(FlipTrigger);
    }

    // Animation Event at 50% of the flip clip
    public void OnFlipSwapSide()
    {
        if (backGO != null) backGO.SetActive(false);
        if (faceGO != null) faceGO.SetActive(true);
    }

    // Animation Event at the end of the flip clip
    public void OnFlipAnimationFinished()
    {
        _bus.Publish(new CardFlipCompleted(_cardId));
    }
}
