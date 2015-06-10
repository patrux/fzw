using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class GenerateFloorNoise : MonoBehaviour 
{
    public static float pixelsToUnits = 100f;
    public int sortingOrder;
    public Color color;
    public Sprite sprite;
    public int count = 500;
    public Vector2 xSize = Vector2.zero;
    public Vector2 ySize = Vector2.zero;
    public Vector2 mapSize = new Vector2(-30f, 30f);

	void Start () 
    {
        Generate();
	}

    void Generate()
    {
        GameObject holder = new GameObject("FloorTiles");
        holder.transform.localScale = Vector3.one;

        for (int i = 0; i < count; i++)
        {
            CreateGO(holder);
        }
    }

    GameObject CreateGO(GameObject _parent)
    {
        GameObject go = new GameObject("FloorTile");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        go.transform.localPosition = new Vector3((int)Random.Range(mapSize.x, mapSize.y), (int)Random.Range(mapSize.x, mapSize.y), 0f);
        go.transform.localScale = new Vector3((int)Random.Range(xSize.x, xSize.y), (int)Random.Range(ySize.x, ySize.y), 1f);
        go.transform.parent = _parent.transform;
        return go;
    }
}
