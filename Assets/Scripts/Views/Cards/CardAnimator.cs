using UnityEngine;

public sealed class CardAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject backGO;
    [SerializeField] private GameObject faceGO;

    private int _cardId;
    private IEventBus _bus;
    private bool _targetFaceUp;

    private static readonly int FlipTrigger = Animator.StringToHash("Flip");

    public void Init(int cardId, IEventBus bus, bool startFaceUp)
    {
        _cardId = cardId;
        _bus = bus;

        _targetFaceUp = startFaceUp;
        ApplyImmediateVisual(startFaceUp);

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

        _targetFaceUp = evt.ToFaceUp;
        animator.ResetTrigger(FlipTrigger);
        animator.SetTrigger(FlipTrigger);
    }

    private void ApplyImmediateVisual(bool faceUp)
    {
        if (faceUp)
        {
            if (backGO != null) backGO.SetActive(false);
            if (faceGO != null) faceGO.SetActive(true);
        }
        else
        {
            if (faceGO != null) faceGO.SetActive(false);
            if (backGO != null) backGO.SetActive(true);
        }
    }

    // Animation Event at ~50%
    public void OnFlipSwapSide()
    {
        ApplyImmediateVisual(_targetFaceUp);
    }

    // Animation Event at the end
    public void OnFlipAnimationFinished()
    {
        _bus.Publish(new CardFlipCompleted(_cardId, _targetFaceUp));
    }
}
