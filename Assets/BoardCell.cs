using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public int cellIndex; // Set 0-8 in the Prefab
    private GameManager gameManager; // No longer needs to be public

    private void Start()
    {
        // Automatically finds the active GameManager in the scene
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // private void OnMouseDown()
    // {
    //     Debug.Log("Cell " + cellIndex + " clicked at position: " + transform.position);

    //     if (gameManager != null)
    //     {
    //         gameManager.HandleCellClick(cellIndex, transform.position);
    //     }
    // }
}
