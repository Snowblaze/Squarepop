using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private const float DisappearTimer = 0.667f;
    private const int MinBoardSize = 7;
    private const int MaxBoardSize = 10;
    private const int MinColorNumber = 4;
    private const int MaxColorNumber = 7;

    public static BoardManager instance;
    private static float timer;

    public TileProvider provider;
    [NonSerialized]
    public List<Material> materials = new List<Material>();
    public TileScript[,] tiles;
    public List<TileScript> fallingBlocks = new List<TileScript>();
    
    [SerializeField]
    private Sprite[] tileSprites; // Tile sprites should be in the same order as in the enum
    [SerializeField]
    [Range(7,10)]
    private int columns, rows;
    [SerializeField]
    private Vector2 boardSize;
    [SerializeField]
    private TileScript tilePrefab;
    [SerializeField]
    private List<Color> tileColors = new List<Color>(MaxColorNumber);
    
    public Vector2 BoardOffset { get; private set; }
    public Vector2 TileSize { get; private set; }
    public Vector2 TileScale { get; private set; }
    private int Columns
    {
        get
        {
            if (columns < MinBoardSize) return MinBoardSize;
            if (columns > MaxBoardSize) return MaxBoardSize;
            return columns;
        }
    }
    private int Rows
    {
        get
        {
            if (rows < MinBoardSize) return MinBoardSize;
            if (rows > MaxBoardSize) return MaxBoardSize;
            return rows;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        if (tileColors == null) throw new Exception("BoardManager: colors array is null.");
        InitializeColors(tileColors);
        provider = new TileProvider();
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
    
    private void OnValidate()
    {
        if (tileColors.Count > MaxColorNumber)
        {
            Debug.LogWarning("Colors' max size is of " + MaxColorNumber);
            tileColors.RemoveRange(MaxColorNumber, tileColors.Count - MaxColorNumber);
        }
        else if (tileColors.Count < MinColorNumber)
        {
            Debug.LogWarning("Colors' min size is of " + MinColorNumber);
            for (int i = 0; i < MinColorNumber - tileColors.Count; i++)
            {
                tileColors.Add(new Color());
            }
        }
    }
    
    public void TilePressed(int column, int row)
    {
        switch (tiles[column, row].tileType)
        {
            case TileType.Color:
                FindColorRegion(column, row);
                break;
            case TileType.BombRadial:
                ExplodeRadial(column, row);
                break;
            case TileType.BombVertical:
                ExplodeLine(column, row, true);
                break;
            case TileType.BombHorizontal:
                ExplodeLine(column, row, false);
                break;
        }
    }

    private void InitializeBoard()
    {
        tiles = new TileScript[Columns, Rows];

        TileSize = tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;

        Vector2 newTileSize = new Vector2(boardSize.x / Columns, boardSize.y / Rows);

        TileScale = new Vector2(newTileSize.x / TileSize.x, newTileSize.y / TileSize.y);
        TileSize = newTileSize;
        
        BoardOffset = new Vector2((TileSize.x - boardSize.x) / 2, (TileSize.y - boardSize.y) / 2);

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                TileScript spawn = tilePrefab.GetPooledInstance(TileType.Color);
                spawn.LastRow = row;
                spawn.transform.localScale = TileScale;
                spawn.transform.SetParent(transform);
                tiles[col, row] = spawn;
            }
        }
        UpdateAllIndexes(true);
    }

    private void UpdateAllIndexes(bool updatePositions)
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (tiles[x, y] == null) continue;

                UpdateIndexes(x, y, updatePositions);
            }
        }
    }

    private void UpdateIndexes(int x, int y, bool updatePositions)
    {
        var tile = tiles[x, y];
        tile.Row = y;
        tile.Column = x;
        if (updatePositions)
            tile.UpdatePosition();
    }
    
    private void SettleBlocks()
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

            AddTilesForColumn(x, firstEmpty.Value);
        }

        UpdateAllIndexes(false);

        if (fallingBlocks.Count > 0)
        {
            GameState.Mode = GameState.GameMode.Falling;
        }
    }

    private void AddTilesForColumn(int x, int initialY)
    {
        for (int y = initialY; y < Rows; y++)
        {
            var tile = tilePrefab.GetPooledInstance(TileType.Color);
            tile.transform.SetParent(transform);
            tile.LastRow = Rows;
            var topLeftPoint = Camera.main.ScreenToWorldPoint(new Vector2(0.0f, Screen.height));
            Vector2 pos = new Vector2(x * TileSize.x + BoardOffset.x + transform.position.x, topLeftPoint.y + (y - initialY) * TileSize.y);
            tile.transform.position = pos;
            tiles[x, y] = tile;
            fallingBlocks.Add(tiles[x, y]);
        }
    }

    private void InitializeColors(List<Color> colors)
    {
        foreach (var color in colors)
        {
            var mat = new Material(Shader.Find("Sprites/Default"))
            {
                color = color
            };
            materials.Add(mat);
        }
    }

    private void ExplodeRadial(int column, int row)
    {
        List<TileScript> tilesToRemove = new List<TileScript>();
        tilesToRemove.Add(tiles[column, row]);
        tiles[column, row] = null;

        var lower = Mathf.Clamp(column - 1, 0, Columns);
        var upper = Mathf.Clamp(column + 2, 0, Columns);
        for (int i = lower; i < upper; i++)
        {
            var innerLower = Mathf.Clamp(row - 1, 0, Rows);
            var innerUpper = Mathf.Clamp(row + 2, 0, Rows);
            for (int j = innerLower; j < innerUpper; j++)
            {
                var tile = tiles[i, j];

                if (tile == null) continue;
                
                if (tile.tileType != TileType.Color)
                {
                    TilePressed(i, j);
                    continue;
                }
                else
                {

                    tiles[i, j] = null;
                }

                tilesToRemove.Add(tile);
            }
        }

        ReturnTilesToPool(tilesToRemove);

        TilePressSuccess();
    }

    private void ExplodeLine(int column, int row, bool vertical)
    {
        List<TileScript> tilesToRemove = new List<TileScript>();
        tilesToRemove.Add(tiles[column, row]);
        tiles[column, row] = null;

        if(vertical)
        {
            for (int i = 0; i < Rows; i++)
            {
                var tile = tiles[column, i];

                if (tile == null) continue;
                
                if (tile.tileType != TileType.Color)
                {
                    TilePressed(column, i);
                    continue;
                }
                else
                {
                    tiles[column, i] = null;
                }

                tilesToRemove.Add(tile);
            }
        }
        else
        {
            for (int i = 0; i < Columns; i++)
            {
                var tile = tiles[i, row];

                if (tile == null) continue;
                
                if (tile.tileType != TileType.Color)
                {
                    TilePressed(i, row);
                    continue;
                }
                else
                {

                    tiles[i, row] = null;
                }

                tilesToRemove.Add(tile);
            }
        }
        
        ReturnTilesToPool(tilesToRemove);

        TilePressSuccess();
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

        ReturnTilesToPool(tilesToRemove);

        if(tilesToRemove.Count == 5)
        {
            var chance = UnityEngine.Random.value > 0.5f;
            var tileType = chance ? TileType.BombHorizontal : TileType.BombVertical;
            var newTile = tilePrefab.GetPooledInstance(tileType);
            newTile.transform.SetParent(transform);
            tiles[column, row] = newTile;
            UpdateIndexes(column, row, true);
        }
        else if (tilesToRemove.Count >= 6)
        {
            var newTile = tilePrefab.GetPooledInstance(TileType.BombRadial);
            newTile.transform.SetParent(transform);
            tiles[column, row] = newTile;
            UpdateIndexes(column, row, true);
        }

        TilePressSuccess();
    }

    private void TilePressSuccess()
    {
        timer = DisappearTimer;
        GameState.Mode = GameState.GameMode.Disappearing;
        GameManager.instance.MoveCounter--;
    }

    private void ReturnTilesToPool(IEnumerable<TileScript> list)
    {
        foreach (var tile in list)
        {
            tile.ToBeReturnedToPool = false;
            tiles[tile.Column, tile.Row] = null;
            provider.ObserverUpdate(tile);
            tile.ReturnToPool();
        }
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
        if (col < 0 || col >= Columns || row < 0 || row >= Rows || tiles[col, row] == null || tiles[col, row].tileType != baseTile.tileType) return false;

        return !tiles[col, row].ToBeReturnedToPool && CompareColors(baseTile, tiles[col, row]);
    }

    private bool CompareColors(TileScript baseTile, TileScript tile)
    {
        if (baseTile == null || tile == null) return false;

        return baseTile.GetMaterialColor() == tile.GetMaterialColor();
    }

    public Sprite GetTileSprite(TileType type)
    {
        return tileSprites[(int)type];
    }
}
