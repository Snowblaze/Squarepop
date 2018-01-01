using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    static List<Material> materials = new List<Material>();

    private SpriteRenderer sprRenderer;

    private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    public ObjectPool Pool { get; set; }

    ObjectPool poolInstanceForPrefab;

    private void Awake()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start () {
        sprRenderer.material = GetRandomMaterial();
	}
    
    private void Update () {
		
	}

    private void OnMouseDown()
    {
        if (sprRenderer.sprite == null)
        {
            return;
        }

        //DoStuff() on click;
    }

    public TileScript GetPooledInstance()
    {
        if (!poolInstanceForPrefab)
        {
            poolInstanceForPrefab = ObjectPool.GetPool(this);
        }
        return poolInstanceForPrefab.GetObject();
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

    public static void GenerateMaterials(Color[] colors)
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
}
