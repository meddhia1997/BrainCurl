using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private BoardView boardView;

    private void Start()
    {
        boardView.BuildBoard();
    }
}
