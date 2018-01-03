using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    public static BoardManager instance;

    private const int MinSize = 7;
    private const int MaxSize = 10;

    [SerializeField]
    [Range(7,10)]
    private int columns, rows;

    private int Columns
    {
        get
        {
            if (columns < MinSize) return MinSize;
            if (columns > MaxSize) return MaxSize;
            else return columns;
        }
    }

    private int Rows
    {
        get
        {
            if (rows < MinSize) return MinSize;
            if (rows > MaxSize) return MaxSize;
            else return rows;
        }
    }

    [SerializeField]
    private Vector2 boardSize;
    private Vector2 boardOffset;

    [SerializeField]
    private GameObject tilePrefab;
    [SerializeField]
    private Color[] tileColors;

    private Vector2 tileSize;
    private Vector2 tileScale;

    public TileScript[,] tiles;

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

    }

    private void InitializeBoard()
    {
        tiles = new TileScript[Columns, Rows];

        tileSize = tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;

        Vector2 newTileSize = new Vector2(boardSize.x / Columns, boardSize.y / Rows);
        tileScale.x = newTileSize.x / tileSize.x;
        tileScale.y = newTileSize.y / tileSize.y;

        tileSize = newTileSize;

        boardOffset.x = -(boardSize.x / 2) + tileSize.x / 2;
        boardOffset.y = -(boardSize.y / 2) + tileSize.y / 2;

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Vector2 pos = new Vector2(col * tileSize.x + boardOffset.x + transform.position.x, row * tileSize.y + boardOffset.y + transform.position.y);

                TileScript spawn = tilePrefab.GetComponent<TileScript>().GetPooledInstance();
                spawn.Row = row;
                spawn.Column = col;
                spawn.transform.position = pos;
                spawn.transform.localScale = tileScale;
                spawn.transform.SetParent(transform);
                tiles[col, row] = spawn;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, boardSize);
    }

    private void InitializeColors(Color[] colors)
    {
        TileScript.GenerateMaterials(colors);
    }

    public void FindColorRegion(int column, int row)
    {
        List<TileScript> regionTiles = new List<TileScript>();
        regionTiles.Add(tiles[column, row]);
        
        for (int x = 0; x < regionTiles.Count; x++)
        {
            if (regionTiles[x].ToBeReturnedToPool) continue;

            var localRow = regionTiles[x].Row;
            var localColumn = regionTiles[x].Column;

            TryToAddToRegionVertical(regionTiles, tiles[column, row], localColumn, localRow);

            for (int i = localColumn - 1; i >= 0; i--)
            {
                if (!CompareColors(tiles[column, row], tiles[i, localRow])) break;

                if (CompareTiles(tiles[column, row], i, localRow))
                {
                    tiles[i, localRow].ToBeReturnedToPool = true;
                    regionTiles.Add(tiles[i, localRow]);

                    TryToAddToRegionVertical(regionTiles, tiles[column, row], i, localRow);
                }
            }
            for(int i = localColumn + 1; i < columns; i++)
            {
                if (!CompareColors(tiles[column, row], tiles[i, localRow])) break;

                if (CompareTiles(tiles[column, row], i, localRow))
                {
                    tiles[i, localRow].ToBeReturnedToPool = true;
                    regionTiles.Add(tiles[i, localRow]);

                    TryToAddToRegionVertical(regionTiles, tiles[column, row], i, localRow);
                }
            }

            regionTiles[x].ToBeReturnedToPool = true;
        }

        if(regionTiles.Count <= 1)
        {
            for(int i = 0; i < regionTiles.Count; i++)
            {
                regionTiles[i].ToBeReturnedToPool = false;
            }
            return;
        }

        foreach(var tile in regionTiles)
        {
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
        if (col < 0 || col >= columns || row < 0 || row >= rows) return false;

        return !tiles[col, row].ToBeReturnedToPool && CompareColors(baseTile, tiles[col, row]);
    }

    private bool CompareColors(TileScript baseTile, TileScript tile)
    {
        return baseTile.GetMaterialColor() == tile.GetMaterialColor();
    }
}
