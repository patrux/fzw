using UnityEngine;
using System.Collections;

public class MapGenerationLogic : MonoBehaviour
{
    public static float pixelsToUnits = 100f; // this is ptu

    private float valueMod = 10f;
    public Vector2 mapSize = new Vector2(200f, 200f);
    public float lavaThickness = 50f;
    public float wallThickness = 20f;

    public Color floorColor;
    public Color wallColor;
    public Color lavaColor;

    public GameObject floor;
    public GameObject[] walls = new GameObject[4];
    public GameObject[] lava = new GameObject[4];

    private Vector2 playableMap = Vector2.zero;
    private Vector2 floorMap = Vector2.zero;

    public Vector3 GetPlayableMapSize()
    {
        return new Vector3(
            playableMap.x,
            playableMap.y,
            0f);
    }

    public Vector3 GetFloorMapSize()
    {
        return new Vector3(
            floorMap.x,
            floorMap.y,
            0f);
    }

    void Start()
    {
        CalculateMap();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    Debug.Log("- Recalculating Map -");
        //    CalculateMap();
        //}
    }

    public void CalculateMap()
    {
        // Initialize Floor
        floor.transform.localScale = new Vector3(mapSize.x * valueMod, mapSize.y * valueMod, 1f);
        floor.transform.localPosition = Vector3.zero;

        // Initialize Map
        InitializeDimensions();

        // Set colors
        floor.GetComponent<SpriteRenderer>().color = floorColor;
        foreach (GameObject go in walls) { go.GetComponent<SpriteRenderer>().color = wallColor; }
        foreach (GameObject go in lava) { go.GetComponent<SpriteRenderer>().color = lavaColor; }
    }

    /// <summary>
    /// Supports Minimap script, but can't make anything but square layouts.
    /// </summary>
    void CalculateWorldDimension()
    {
        Vector2 dOrigin = floor.transform.position; // origin
        Vector2 dFloorOffset = dOrigin + ((new Vector2(floor.transform.localScale.x, floor.transform.localScale.y) / pixelsToUnits) / 2f); // add floor
        Vector2 dLavaOffset = new Vector2(dFloorOffset.x + ((lavaThickness * valueMod) / pixelsToUnits), dFloorOffset.y + ((lavaThickness * valueMod) / pixelsToUnits)); // add lava

        floorMap = dFloorOffset;
        playableMap = dLavaOffset;
    }

    public Vector2 GetWorldDimension()
    {
        return playableMap;
    }

    void InitializeDimensions()
    {
        // Use the floor as the root object
        AttachableObject root = new AttachableObject(floor, 0f);
        root.isRoot = true;

        // Create an LavaAO for each LavaGO
        AttachableObject[] lavaList = new AttachableObject[lava.Length];
        for (int i = 0; i < lavaList.Length; i++)
        {
            lavaList[i] = new AttachableObject(lava[i], lavaThickness * valueMod);
            lavaList[i].Attach(root, (TranslateNumberToSide(i)));
        }

        // Create an LavaAO for each LavaGO
        AttachableObject[] wallList = new AttachableObject[walls.Length];
        for (int i = 0; i < wallList.Length; i++)
        {
            wallList[i] = new AttachableObject(walls[i], wallThickness * valueMod);
            wallList[i].Attach(lavaList[i], (TranslateNumberToSide(i)));
        }

        CalculateWorldDimension();
    }

    AnchorSide TranslateNumberToSide(int _number)
    {
        switch (_number)
        {
            default:
                return AnchorSide.TOP;
            case 1:
                return AnchorSide.BOTTOM;
            case 2:
                return AnchorSide.LEFT;
            case 3:
                return AnchorSide.RIGHT;
        }
    }

    class AttachableObject
    {
        public GameObject go; // The source GameObject
        public Vector4 dimension; // The position+scale, not ptu
        public float thickness;
        public bool isRoot = false;

        public AttachableObject parent; // The object this object is attached to
        public AnchorSide parentSide; // Which side this object is attached to

        public AttachableObject(GameObject _go, float _thickness)
        {
            go = _go;
            thickness = _thickness;
            CalculateDimension();
        }

        /// <summary>
        /// Attaches the object to the side of a parent.
        /// Note: "ptu" stands for "pixelsToUnits".
        /// </summary>
        public void Attach(AttachableObject _parent, AnchorSide _side)
        {
            if (_parent == null)
                return;

            // Set this as parent
            parent = _parent;
            parentSide = _side;

            // Calculate the scale now that we have a new object
            CalculateScale();

            // Store parent transforms
            Vector3 halfScale = (go.transform.localScale * 0.5f); // not ptu

            // Returns the target position, which needs to be divided by ptu to get the real position
            Vector4 targetPosition = CalculateTargetPosition(_parent, _side, halfScale);

            // Set position based on parent position and divide by ptu
            go.transform.localPosition = new Vector3(
                _parent.go.transform.position.x + ((targetPosition.z + targetPosition.w) / pixelsToUnits),
                _parent.go.transform.position.y + ((targetPosition.x + targetPosition.y) / pixelsToUnits),
                go.transform.localPosition.z);


            // Recalculate dimensions since the object has been changed
            CalculateDimension();
        }

        /// <summary>
        /// Returns target position for a given object.
        /// </summary>
        Vector4 CalculateTargetPosition(AttachableObject _parent, AnchorSide _side, Vector3 _halfScale)
        {
            Vector4 dim = Vector4.zero;
            switch (_side)
            {
                case AnchorSide.TOP:
                    if (_parent.isRoot)
                        dim.x = _parent.dimension.x + _halfScale.y;
                    else
                        dim.x += _halfScale.y + (_parent.thickness / 2f);
                    break;
                case AnchorSide.BOTTOM:
                    if (_parent.isRoot)
                        dim.y = _parent.dimension.y - _halfScale.y;
                    else
                        dim.y -= _halfScale.y + (_parent.thickness / 2f);
                    break;
                case AnchorSide.LEFT:
                    if (_parent.isRoot)
                        dim.z = _parent.dimension.z - _halfScale.x;
                    else
                        dim.z -= _halfScale.x + (_parent.thickness / 2f);
                    break;
                case AnchorSide.RIGHT:
                    if (_parent.isRoot)
                        dim.w = _parent.dimension.w + _halfScale.x;
                    else
                        dim.w += _halfScale.x + (_parent.thickness / 2f);
                    break;
            }
            return dim;
        }

        /// <summary>
        /// Calculates the dimension for this object. And then retuns a PTU dimension.
        /// </summary>
        void CalculateDimension()
        {
            // Get transforms
            Vector3 pos = go.transform.position;
            Vector3 halfScale = (go.transform.localScale * 0.5f);

            // Set bounds of dimension
            Vector4 d = new Vector4(
                (pos.y * pixelsToUnits) + halfScale.y, // top (y-max)
                (pos.y * pixelsToUnits) - halfScale.y, // bottom (y-min)
                (pos.x * pixelsToUnits) - halfScale.x, // left (x-min)
                (pos.x * pixelsToUnits) + halfScale.x); // right (x-max)

            dimension = d;
        }

        /// <summary>
        /// Calculate the scale of this object to match parent.
        /// Top and Bottom sides will cover the corners for the sides by adding 2x thicknesses.
        /// </summary>
        void CalculateScale()
        {
            Vector3 scale = Vector3.zero;
            switch (parentSide)
            {
                case AnchorSide.TOP:
                    CalculateScaleAux(out scale, true);
                    break;
                case AnchorSide.BOTTOM:
                    CalculateScaleAux(out scale, true);
                    break;
                case AnchorSide.LEFT:
                    CalculateScaleAux(out scale, false);
                    break;
                case AnchorSide.RIGHT:
                    CalculateScaleAux(out scale, false);
                    break;
            }
            go.transform.localScale = scale;
        }

        /// <summary>
        /// Aux function to calculate the scale, it just makes the code somewhat cleaner.
        /// </summary>
        void CalculateScaleAux(out Vector3 _scale, bool _isTopOrBottom)
        {
            if (_isTopOrBottom)
                _scale = new Vector3((AbsAdd(parent.dimension.z, parent.dimension.w) + thickness * 2f), thickness, 1f);
            else
                if (parent.isRoot)
                    _scale = new Vector3(thickness, AbsAdd(parent.dimension.x, parent.dimension.y), 1f);
                else
                    _scale = new Vector3(thickness, (AbsAdd(parent.dimension.x, parent.dimension.y) + parent.thickness * 2f), 1f);
        }

        /// <summary>
        /// An absolute addition between two floats.
        /// </summary>
        float AbsAdd(float _a, float _b)
        {
            return Mathf.Abs(_a) + Mathf.Abs(_b);
        }
    }

    enum AnchorSide
    {
        TOP = 0,
        BOTTOM = 1,
        LEFT = 2,
        RIGHT = 3
    }

    public static string WriteVector4(Vector4 _v)
    {
        return "[" + _v.x + ", " + _v.y + ", " + _v.z + ", " + _v.w + "]";
    }
}
