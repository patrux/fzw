using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour
{
    // Size of the minimap
    public Vector2 minimapSize = Vector3.zero;

    // Offset of the minimap
    public Vector2 minimapOffset = Vector3.zero;

    // World size
    Vector2 worldDimension;

    // The player position to track in relation to worldSize
    GameObject player;

    // The background minimap sprite
    UISprite spriteBG;

    // Size of the map in x and y, from center to side
    Vector2 dimMinimap;

    // The player minimap sprite
    UISprite spritePlayer;

    // Position last frame
    Vector3 lastPosition = Vector3.zero;

    // Force an update on first frame to determine player position
    bool forceUpdate = true;

    // Initialize before calling update
    bool isInitialized = false;

    void Awake()
    {
        LocalUI.OnEnableUI += Initialize;
    }

    void Initialize()
    {
        // Set sprites
        spriteBG = GameObject.Find("Minimap/Sprite (BG)").GetComponent<UISprite>();
        spritePlayer = GameObject.Find("Minimap/Sprite (Minimap_Player)").GetComponent<UISprite>();

        // Set minimap background position and dimension
        InitializeBackground();

        // Get player
        player = GameObject.Find("LocalPlayer");

        // Get dimensions
        worldDimension = GameObject.Find("Map").GetComponent<MapGenerationLogic>().GetWorldDimension();

        // Store minimap bg size
        dimMinimap = spriteBG.localSize / 2f;

        isInitialized = true;
    }

    void FixedUpdate()
    {
        if (!isInitialized)
            return;

        UpdateMinimap();
    }

    void UpdateMinimap()
    {
        // Do not update if the position hasn't changed
        if ((lastPosition.x == player.transform.localPosition.x && lastPosition.y == player.transform.localPosition.y) && !forceUpdate)
            return;

        // Size of the world in x and y, from center to side
        //Vector2 dimWorld = mgl.GetWorldDimension();
        Vector2 dimWorld = worldDimension;

        // Get the scale factor between player position and world dimension
        // Clamp the value so the icon won't go off the minimap
        Vector2 scaleWorld = new Vector2(
            Mathf.Clamp(player.transform.position.x / dimWorld.x, -1.0f, 1.0f),
            Mathf.Clamp(player.transform.position.y / dimWorld.y, -1.0f, 1.0f));

        // Center of minimap
        Vector2 minimapOrigin = new Vector2(spriteBG.transform.localPosition.x, spriteBG.transform.localPosition.y);

        // Offset scale between scaleWorld and map size
        Vector2 scaleWorldToMapOffset = new Vector2(dimMinimap.x * scaleWorld.x, dimMinimap.y * scaleWorld.y);

        // Set position
        spritePlayer.transform.localPosition = minimapOrigin + scaleWorldToMapOffset;

        // Store last position so we don't have to update if there is no movement
        lastPosition = player.transform.localPosition;
    }

    /// <summary>
    /// Forces a synchronization the next update.
    /// </summary>
    public void ForcePositionUpdate()
    {
        forceUpdate = true;
    }

    /// <summary>
    /// Scale the background sprite
    /// </summary>
    void InitializeBackground()
    {
        Transform root = GameObject.Find("UIRoot").transform;

        // X
        spriteBG.leftAnchor.target = root;
        spriteBG.leftAnchor.absolute = (int)minimapOffset.x;

        spriteBG.rightAnchor.target = root;
        spriteBG.rightAnchor.absolute = spriteBG.leftAnchor.absolute + (int)minimapSize.x;

        // Y
        spriteBG.topAnchor.target = root;
        spriteBG.topAnchor.absolute = -(int)minimapOffset.y;

        spriteBG.bottomAnchor.target = root;
        spriteBG.bottomAnchor.absolute = spriteBG.topAnchor.absolute - (int)minimapSize.y;

        spriteBG.UpdateAnchors();
    }
}
