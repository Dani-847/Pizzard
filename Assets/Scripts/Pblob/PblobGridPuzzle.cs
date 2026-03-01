using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PblobGridPuzzle : MonoBehaviour
{
    [Header("Prefabs & Config")]
    public GameObject tilePrefab;
    public int gridWidth = 20;
    public int gridHeight = 5;
    public float tileSize = 1.0f;

    [Header("State")]
    private GameObject[,] grid;
    private List<Vector2Int> safePath = new List<Vector2Int>();
    private bool isGenerating = false;

    public void GenerateGrid(Vector3 startWorldPos)
    {
        if (isGenerating) return;
        isGenerating = true;

        // Make grid tiles 1.0f, spanning a 23x6 manual grid layout
        gridWidth = 23;
        gridHeight = 6;
        tileSize = 1.0f;

        if (grid != null)
        {
            DestroyGrid();
        }

        grid = new GameObject[gridWidth, gridHeight];
        
        // Spawn tiles
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = startWorldPos + new Vector3(x * tileSize, y * tileSize, 0);
                // Center the grid around startWorldPos
                pos.x -= (gridWidth - 1) * tileSize / 2f;
                pos.y -= (gridHeight - 1) * tileSize / 2f;

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                
                // Keep them invisible/gray at first
                PblobGridTile gt = tile.GetComponent<PblobGridTile>();
                if (gt != null)
                {
                    gt.SetColor(Color.gray);
                }

                grid[x, y] = tile;
            }
        }

        CalculateSafePath();
        StartCoroutine(AnimateGridReveal());
    }

    private void CalculateSafePath()
    {
        safePath.Clear();
        int currentX = gridWidth / 2;
        int currentY = 0;

        safePath.Add(new Vector2Int(currentX, currentY));

        // Drunkard's walk up to the boss
        while (currentY < gridHeight - 1)
        {
            int dir = Random.Range(0, 3); // 0 = Left, 1 = Up, 2 = Right
            
            if (dir == 0 && currentX > 0) currentX--;
            else if (dir == 2 && currentX < gridWidth - 1) currentX++;
            else currentY++; // Always push up if we hit a wall or roll 1

            // Prevent going backwards down
            Vector2Int nextStep = new Vector2Int(currentX, currentY);
            if (!safePath.Contains(nextStep))
            {
                safePath.Add(nextStep);
            }
            
            // For safety, force upward if we loop too much horizontally
            if (safePath.Count > gridWidth * gridHeight) break;
        }

        // Apply path to tiles
        foreach (var p in safePath)
        {
            PblobGridTile gt = grid[p.x, p.y].GetComponent<PblobGridTile>();
            if (gt != null)
            {
                gt.isSafePath = true;
            }
        }
    }

    private IEnumerator AnimateGridReveal()
    {
        // 1. Flash
        for (int i = 0; i < 3; i++)
        {
            SetAllTilesColor(Color.white);
            yield return new WaitForSeconds(0.2f);
            SetAllTilesColor(Color.gray);
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Reveal
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                PblobGridTile gt = grid[x, y].GetComponent<PblobGridTile>();
                if (gt != null)
                {
                    gt.SetColor(gt.isSafePath ? Color.green : Color.red);
                }
            }
        }

        isGenerating = false;
    }

    private void SetAllTilesColor(Color c)
    {
        if (grid == null) return;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    PblobGridTile gt = grid[x, y].GetComponent<PblobGridTile>();
                    if (gt != null) gt.SetColor(c);
                }
            }
        }
    }

    public void DestroyGrid()
    {
        if (grid != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null) Destroy(grid[x, y]);
                }
            }
            grid = null;
        }
        safePath.Clear();
    }
}
