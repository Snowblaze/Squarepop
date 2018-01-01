using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    static List<Material> materials = new List<Material>();

	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().material = GetRandomMaterial();
	}
	
	// Update is called once per frame
	void Update () {
		
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
