using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    [SerializeField]
    [Range(7,10)]
    private int columns, rows;

    private int Columns
    {
        get
        {
            if (columns < 7) return 7;
            if (columns > 10) return 10;
            else return columns;
        }
    }

    private int Rows
    {
        get
        {
            if (rows < 7) return 7;
            if (rows > 10) return 10;
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

    public GameObject[,] tiles;

    private void Awake()
    {
        if (tileColors == null) throw new Exception("BoardManager: colors array is null.");
        InitializeColors(tileColors);
    }

    // Use this for initialization
    private void Start () {
        InitializeBoard();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void InitializeBoard()
    {
        tiles = new GameObject[Columns, Rows];

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
                spawn.transform.position = pos;
                spawn.transform.localScale = tileScale;
                spawn.transform.SetParent(transform);
                tiles[col, row] = spawn.gameObject;
                //GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                //newTile.transform.localScale = tileScale;
                //tiles[col, row] = newTile;
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
}
