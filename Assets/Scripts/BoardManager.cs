using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    private static float timer;
    private const float DisappearTimer = 0.667f;
    private const int MinBoardSize = 7;
    private const int MaxBoardSize = 10;

    public static BoardManager instance;

    [SerializeField]
    [Range(7,10)]
    private int columns, rows;

    private int Columns
    {
        get
        {
            if (columns < MinBoardSize) return MinBoardSize;
            if (columns > MaxBoardSize) return MaxBoardSize;
            else return columns;
        }
    }
    private int Rows
    {
        get
        {
            if (rows < MinBoardSize) return MinBoardSize;
            if (rows > MaxBoardSize) return MaxBoardSize;
            else return rows;
        }
    }

    [SerializeField]
    private Vector2 boardSize;
    [SerializeField]
    private GameObject tilePrefab;
    [SerializeField]
    private Color[] tileColors;

    public Vector2 BoardOffset { get; private set; }
    public Vector2 TileSize { get; private set; }
    public Vector2 TileScale { get; private set; }

    public TileScript[,] tiles;
    public List<TileScript> fallingBlocks = new List<TileScript>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);

        if (tileColors == null) throw new Exception("BoardManager: colors array is null.");
        InitializeColors(tileColors);
    }
    
    private void Start () {
        InitializeBoard();
    }

    private void Update()
    {
        if (GameState.Mode == GameState.GameMode.Disappearing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                GameState.Mode = GameState.GameMode.Playing;
                SettleBlocks();
            }
        }

        if (GameState.Mode == GameState.GameMode.Falling && fallingBlocks.Count == 0)
        {
            GameState.Mode = GameState.GameMode.Playing;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, boardSize);
    }

    private void InitializeBoard()
    {
        tiles = new TileScript[Columns, Rows];

        TileSize = tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;

        Vector2 newTileSize = new Vector2(boardSize.x / Columns, boardSize.y / Rows);

        TileScale = new Vector2(newTileSize.x / TileSize.x, newTileSize.y / TileSize.y);
        TileSize = newTileSize;

        BoardOffset = new Vector2(-(boardSize.x / 2) + TileSize.x / 2, -(boardSize.y / 2) + TileSize.y / 2);

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                TileScript spawn = tilePrefab.GetComponent<TileScript>().GetPooledInstance();
                spawn.transform.localScale = TileScale;
                spawn.transform.SetParent(transform);
                tiles[col, row] = spawn;
            }
        }
        UpdateIndexes(true);
    }

    private void UpdateIndexes(bool updatePositions)
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (tiles[x, y] == null) continue;

                UpdateIndex(x, y, updatePositions);
            }
        }
    }

    private void UpdateIndex(int x, int y, bool updatePositions)
    {
        tiles[x, y].Row = y;
        tiles[x, y].Column = x;
        if (updatePositions)
            tiles[x, y].UpdatePosition();
    }
    
    public void SettleBlocks()
    {
        fallingBlocks.Clear();
        for (int x = 0; x < Columns; x++)
        {
            int? firstEmpty = null;
            for (int y = 0; y < Rows; y++)
            {
                if (tiles[x, y] == null && !firstEmpty.HasValue)
                {
                    firstEmpty = y;
                }
                else if (firstEmpty.HasValue && tiles[x, y] != null)
                {
                    tiles[x, y].LastRow = y;
                    fallingBlocks.Add(tiles[x, y]);
                    tiles[x, firstEmpty.Value] = tiles[x, y];
                    tiles[x, y] = null;
                    firstEmpty++;
                }
            }

            if (!firstEmpty.HasValue) continue;
            
            for (int y = firstEmpty.Value; y < Rows; y++)
            {
                var tile = tilePrefab.GetComponent<TileScript>().GetPooledInstance();
                tile.transform.SetParent(transform);
                tile.LastRow = Rows;
                var topLeftPoint = Camera.main.ScreenToWorldPoint(new Vector2(0.0f, Screen.height));
                Vector2 pos = new Vector2(x * TileSize.x + BoardOffset.x + transform.position.x, topLeftPoint.y + (y - firstEmpty.Value) * TileSize.y);
                tile.transform.position = pos;
                tiles[x, y] = tile;
                fallingBlocks.Add(tiles[x, y]);
            }
        }

        UpdateIndexes(false);

        if (fallingBlocks.Count > 0)
        {
            GameState.Mode = GameState.GameMode.Falling;
        }
    }

    private void InitializeColors(Color[] colors)
    {
        TileScript.GenerateMaterials(colors);
    }

    public void TilePressed(int column, int row)
    {
        FindColorRegion(column, row);
    }

    private void FindColorRegion(int column, int row)
    {
        List<TileScript> tilesToRemove = new List<TileScript>();
        List<TileScript> tilesToIterate = new List<TileScript>();
        tilesToIterate.Add(tiles[column, row]);
        
        for (int x = 0; x < tilesToIterate.Count; x++)
        {
            if (tilesToIterate[x].ToBeReturnedToPool) continue;

            var localRow = tilesToIterate[x].Row;
            var localColumn = tilesToIterate[x].Column;

            TryToAddToRegionVertical(tilesToIterate, tiles[column, row], localColumn, localRow);

            for (int i = localColumn - 1; i >= 0; i--)
            {
                if (!CompareColors(tiles[column, row], tiles[i, localRow])) break;

                if (CompareTiles(tiles[column, row], i, localRow))
                {
                    tiles[i, localRow].ToBeReturnedToPool = true;
                    tilesToRemove.Add(tiles[i, localRow]);

                    TryToAddToRegionVertical(tilesToIterate, tiles[column, row], i, localRow);
                }
            }
            for(int i = localColumn + 1; i < Columns; i++)
            {
                if (!CompareColors(tiles[column, row], tiles[i, localRow])) break;

                if (CompareTiles(tiles[column, row], i, localRow))
                {
                    tiles[i, localRow].ToBeReturnedToPool = true;
                    tilesToRemove.Add(tiles[i, localRow]);

                    TryToAddToRegionVertical(tilesToIterate, tiles[column, row], i, localRow);
                }
            }

            tilesToIterate[x].ToBeReturnedToPool = true;
            tilesToRemove.Add(tilesToIterate[x]);
        }

        if(tilesToRemove.Count <= 1)
        {
            for(int i = 0; i < tilesToRemove.Count; i++)
            {
                tilesToRemove[i].ToBeReturnedToPool = false;
            }
            return;
        }

        foreach(var tile in tilesToRemove)
        {
            tile.ToBeReturnedToPool = false;
            tiles[tile.Column, tile.Row] = null;
            tile.ReturnToPool();
        }

        timer = DisappearTimer;
        GameState.Mode = GameState.GameMode.Disappearing;
    }

    private void TryToAddToRegionVertical(List<TileScript> region, TileScript baseTile, int col, int row)
    {
        TryToAddToRegion(region, baseTile, col, row + 1);
        TryToAddToRegion(region, baseTile, col, row - 1);
    }

    private void TryToAddToRegion(List<TileScript> region, TileScript baseTile, int col, int row)
    {
        if (CompareTiles(baseTile, col, row))
        {
            region.Add(tiles[col, row]);
        }
    }

    private bool CompareTiles(TileScript baseTile, int col, int row)
    {
        if (col < 0 || col >= Columns || row < 0 || row >= Rows || tiles[col, row] == null) return false;

        return !tiles[col, row].ToBeReturnedToPool && CompareColors(baseTile, tiles[col, row]);
    }

    private bool CompareColors(TileScript baseTile, TileScript tile)
    {
        if (baseTile == null || tile == null) return false;

        return baseTile.GetMaterialColor() == tile.GetMaterialColor();
    }
}
