using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    private const float FallSpeed = 3f;

    static List<Material> materials = new List<Material>();
    
    public int LastRow { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public ObjectPool Pool { get; set; }
    public bool ToBeReturnedToPool { get; set; }
    public TileType tileType;

    private SpriteRenderer sprRenderer;
    private ObjectPool poolInstanceForPrefab;

    Vector2 tmpPos;

    private void Awake()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if(tileType == TileType.Color)
            sprRenderer.sharedMaterial = GetRandomMaterial();
        
        // Temorary bomb visual
        if (tileType == TileType.BombRadial)
            sprRenderer.sharedMaterial = materials[0];
        else if (tileType == TileType.BombVertical)
            sprRenderer.sharedMaterial = materials[1];
        else if (tileType == TileType.BombHorizontal)
            sprRenderer.sharedMaterial = materials[2];
    }

    private void Update()
    {
        if (GameState.Mode == GameState.GameMode.Falling && Row != LastRow)
        {
            var board = BoardManager.instance;
            var targetY = Row * board.TileSize.y + board.BoardOffset.y + board.transform.position.y;

            tmpPos = transform.position;
            tmpPos.y -= FallSpeed * Time.deltaTime;
            if (tmpPos.y <= targetY)
            {
                board.fallingBlocks.Remove(this);
                UpdatePosition();
            }
            else
            {
                transform.position = tmpPos;
            }
        }
    }

    protected virtual void OnMouseDown()
    {
        if (sprRenderer.sprite == null)
        {
            return;
        }

        if (GameState.Mode == GameState.GameMode.Playing)
        {
            BoardManager.instance.TilePressed(Column, Row);
            GameState.ActionsTaken++;
        }
    }

    public void UpdatePosition()
    {
        var board = BoardManager.instance;
        var tileSize = board.TileSize;
        var boardOffset = board.BoardOffset;
        var boardPos = board.transform.position;
        Vector2 pos = new Vector2(Column * tileSize.x + boardOffset.x + boardPos.x, 
                                  Row    * tileSize.y + boardOffset.y + boardPos.y);
        transform.position = pos;
    }

    public TileScript GetPooledInstance(TileType type)
    {
        if (!poolInstanceForPrefab)
        {
            poolInstanceForPrefab = ObjectPool.GetPool(this);
        }
        return poolInstanceForPrefab.GetObject(type);
    }

    public void ReturnToPool()
    {
        if (Pool)
        {
            Pool.AddObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void GenerateMaterials(List<Color> colors)
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

    public Material GetRandomMaterial()
    {
        if (materials.Count == 0) throw new Exception("TileScript: no materials generated.");

        int index = UnityEngine.Random.Range(0, materials.Count);
        return materials[index];
    }

    public Color GetMaterialColor()
    {
        return sprRenderer.sharedMaterial.color;
    }
}
