using UnityEngine;

public class BoardCell : MonoBehaviour
{
     // Sets 0-8 for the board cells inspector index
    public int cellIndex;
    private GameManager gameManager;

    private void Start()
    {
        // Automatically finds the active GameManager in the scene
        gameManager = FindFirstObjectByType<GameManager>();
    }
}
