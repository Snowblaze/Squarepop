using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    [SerializeField]
    private int columns;
    [SerializeField]
    private int rows;
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
        tiles = new GameObject[columns, rows];

        tileSize = tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;

        Vector2 newTileSize = new Vector2(boardSize.x / (float)columns, boardSize.y / (float)rows);
        tileScale.x = newTileSize.x / tileSize.x;
        tileScale.y = newTileSize.y / tileSize.y;

        tileSize = newTileSize;

        boardOffset.x = -(boardSize.x / 2) + tileSize.x / 2;
        boardOffset.y = -(boardSize.y / 2) + tileSize.y / 2;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                Vector2 pos = new Vector2(col * tileSize.x + boardOffset.x + transform.position.x, row * tileSize.y + boardOffset.y + transform.position.y);

                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                newTile.transform.localScale = tileScale;
                tiles[col, row] = newTile;
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
