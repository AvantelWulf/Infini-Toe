using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Board Assets")]
    public Transform[] boardCells;
    public LineRenderer winningLine;
    public float lineExtend = 100f;

    [Header("Piece Prefabs")]
    public GameObject xPrefab;
    public GameObject oPrefab;
    public GameObject darkXPrefab;
    public GameObject darkOPrefab;

    [Header("UI")]
    public GameObject restartButton;
    public TMP_Text winnerBanner;
    public TMP_Text blueTurn;
    public TMP_Text redTurn;

    private struct MoveData
    {
        public int index;
        public GameObject pieceObject;
    }

    private string[] board = new string[9];
    private bool isXTurn = true;
    private bool gameOver = false;

    // Using Lists instead of Queues so index [0] stays the oldest piece when swapped
    private List<MoveData> xMoves = new List<MoveData>();
    private List<MoveData> oMoves = new List<MoveData>();

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        if (winnerBanner != null)
        {
            winnerBanner.gameObject.SetActive(false);
        }

        if (blueTurn != null)
        {
            blueTurn.gameObject.SetActive(true);
        }

        if (redTurn != null)
        {
            redTurn.gameObject.SetActive(false);
        }

        if (winningLine != null)
        {
            winningLine.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
            if (hitCollider != null)
            {
                BoardCell cell = hitCollider.GetComponent<BoardCell>();
                if (cell != null)
                {
                    HandleCellClick(cell.cellIndex, cell.transform.position);
                }
            }
        }
    }

    public void HandleCellClick(int index, Vector3 spawnPosition)
    {
        if (gameOver || !string.IsNullOrEmpty(board[index])) return;

        string currentPlayer = isXTurn ? "X" : "O";
        GameObject prefabToSpawn = isXTurn ? xPrefab : oPrefab;

        if (isXTurn)
        {
            if (blueTurn != null) blueTurn.gameObject.SetActive(false);
            if (redTurn != null) redTurn.gameObject.SetActive(true);
        }
        else
        {
            if (blueTurn != null) blueTurn.gameObject.SetActive(true);
            if (redTurn != null) redTurn.gameObject.SetActive(false);
        }

        List<MoveData> currentList = isXTurn ? xMoves : oMoves;

        // 1. Remove the oldest piece (always at index 0) if 3 pieces are already active
        if (currentList.Count == 3)
        {
            MoveData oldestMove = currentList[0];
            board[oldestMove.index] = "";
            Destroy(oldestMove.pieceObject);
            currentList.RemoveAt(0);
        }

        // 2. Spawn the new piece and add it to the end of the list
        GameObject newPiece = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        board[index] = currentPlayer;
        currentList.Add(new MoveData { index = index, pieceObject = newPiece });

        // 3. Check for a win
        if (CheckWin(currentPlayer))
        {
            gameOver = true;
            Debug.Log(currentPlayer + " Wins!");

            if (winnerBanner != null)
            {
                winnerBanner.text = isXTurn ? "Blue Wins!" : "Red Wins!";
                winnerBanner.gameObject.SetActive(true);
            }

            if (restartButton != null)
            {
                restartButton.SetActive(true);
            }

            blueTurn.gameObject.SetActive(false);
            redTurn.gameObject.SetActive(false);
            return;
        }

        // 4. Switch turns
        isXTurn = !isXTurn;

        //5. Update the visual warnings for the oldest pieces
        UpdatePieceFades();

    }

        private void UpdatePieceFades()
    {
        // Restore X's oldest piece to normal if it's O's turn, or darken it if it's X's turn
        if (xMoves.Count == 3)
        {
            GameObject targetXPrefab = isXTurn ? darkXPrefab : xPrefab;
            SwapPiecePrefab(xMoves, 0, targetXPrefab);
        }

        // Restore O's oldest piece to normal if it's X's turn, or darken it if it's O's turn
        if (oMoves.Count == 3)
        {
            GameObject targetOPrefab = !isXTurn ? darkOPrefab : oPrefab;
            SwapPiecePrefab(oMoves, 0, targetOPrefab);
        }
    }

    private void SwapPiecePrefab(List<MoveData> moveList, int listIndex, GameObject newPrefab)
    {
        MoveData targetMove = moveList[listIndex];
        Vector3 pos = targetMove.pieceObject.transform.position;
        Destroy(targetMove.pieceObject);

        GameObject swappedPiece = Instantiate(newPrefab, pos, Quaternion.identity);
        moveList[listIndex] = new MoveData { index = targetMove.index, pieceObject = swappedPiece };
    }

    private bool CheckWin(string player)
    {
        int[,] winConditions = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Columns
            {0, 4, 8}, {2, 4, 6}             // Diagonals
        };

        for (int i = 0; i < 8; i++)
        {
            int a = winConditions[i, 0];
            int b = winConditions[i, 1];
            int c = winConditions[i, 2];

            if (board[a] == player && board[b] == player && board[c] == player)
            {
                DrawWinLine(a, c);
                return true;
            }
        }
        return false;
    }

    private void DrawWinLine(int startIndex, int endIndex)
    {
        if (winningLine == null || boardCells == null || boardCells.Length < 9) return;

        Vector3 startPos = boardCells[startIndex].position;
        Vector3 endPos = boardCells[endIndex].position;

        startPos.z = 0f;
        endPos.z = 0f;

        Vector3 direction = (endPos - startPos).normalized;
        startPos -= direction * lineExtend;
        endPos += direction * lineExtend;

        winningLine.SetPosition(0, startPos);
        winningLine.SetPosition(1, endPos);
        winningLine.gameObject.SetActive(true);

    }

    public void ResetGame()
    {
        foreach (MoveData move in xMoves) Destroy(move.pieceObject);
        foreach (MoveData move in oMoves) Destroy(move.pieceObject);

        xMoves.Clear();
        oMoves.Clear();

        for (int i = 0; i < 9; i++) board[i] = "";

        isXTurn = true;
        gameOver = false;

        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        if (winnerBanner != null)
        {
            winnerBanner.gameObject.SetActive(false);
        }

        if (winningLine != null)
        {
            winningLine.gameObject.SetActive(false);
        }

        if (blueTurn != null)
        {
            blueTurn.gameObject.SetActive(true);
        }
    }
}
