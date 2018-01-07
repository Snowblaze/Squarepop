using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileScript : MonoBehaviour
{
    private const float FallSpeed = 3f;
    
    public TileType tileType;

    private SpriteRenderer spriteRenderer;
    private ObjectPool poolInstanceForPrefab;
    private Vector2 tmpPos;

    public int LastRow { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public ObjectPool Pool { get; set; }
    public bool ToBeReturnedToPool { get; set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if(tileType == TileType.Color)
            spriteRenderer.sharedMaterial = GetRandomMaterial();
        
        // Temporary bomb visuals
        // To be deleted
        if (tileType == TileType.BombRadial)
            spriteRenderer.sharedMaterial = BoardManager.instance.materials[0];
        else if (tileType == TileType.BombVertical)
            spriteRenderer.sharedMaterial = BoardManager.instance.materials[1];
        else if (tileType == TileType.BombHorizontal)
            spriteRenderer.sharedMaterial = BoardManager.instance.materials[2];
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
                StartCoroutine(Bounce(1));
            }
            else
            {
                transform.position = tmpPos;
            }
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (spriteRenderer.sprite == null)
        {
            return;
        }

        if (GameState.Mode == GameState.GameMode.Playing)
        {
            BoardManager.instance.TilePressed(Column, Row);
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
        LastRow = Row;
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

    public Material GetRandomMaterial()
    {
        var materials = BoardManager.instance.materials;
        if (materials.Count == 0) throw new Exception("TileScript: no materials generated.");
        int index = UnityEngine.Random.Range(0, materials.Count);
        return materials[index];
    }

    public Color GetMaterialColor()
    {
        return spriteRenderer.sharedMaterial.color;
    }

    private IEnumerator Bounce(int bounceTimes)
    {
        var bounceSpeed = 1.0f;
        var bounceAmount = 0.1f;
        var cachedPos = transform.position;
        for (int i = 0; i < bounceTimes; i++)
        {
            float t = 0.0f;
            while (t < 1.0)
            {
                t += Time.deltaTime * bounceSpeed;
                transform.position = new Vector2(cachedPos.x, cachedPos.y + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * bounceAmount);
                yield return null;
            }
        }
    }
}
