using UnityEngine;

// Zelda-style status bar drawn along the top of the screen.
// Renders a black band containing a bordered box (sized to exactly one grid
// cell) that displays whatever item the player is currently holding.
//
// To make real room for the band, the level camera's viewport is confined to
// the area below the bar (shrink-to-fit): the camera keeps its full original
// view, so nothing is ever cropped or clipped, and the whole playfield simply
// scales down to fit beneath the bar.
//
// Like PauseMenuController, this is a persistent singleton rendered with
// immediate-mode GUI, so it works across every level without per-scene setup.
// It leaves scenes without a Player (e.g. the title screen) completely alone.
public class StatusBarController : MonoBehaviour
{
    // Padding above and below the item box, as a fraction of the box size.
    private const float ClearanceFraction = 0.4f;

    private static StatusBarController instance;

    // Auto-create instance when accessed (mirrors PauseMenuController).
    public static StatusBarController Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("StatusBarController");
                instance = obj.AddComponent<StatusBarController>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Texture2D barTexture;
    private Texture2D boxBackgroundTexture;
    private Texture2D boxBorderTexture;

    private PlayerController player;
    private Camera adjustedCamera;

    void Awake()
    {
        // Singleton pattern - ensure only one instance exists.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        barTexture = CreateColorTexture(Color.black);
        boxBackgroundTexture = CreateColorTexture(new Color(0.08f, 0.08f, 0.08f));
        boxBorderTexture = CreateColorTexture(new Color(0.85f, 0.85f, 0.85f));
    }

    Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void Update()
    {
        // Only reshape the camera in scenes that actually have a player
        // (so menus / the title screen are never touched).
        if (GetPlayer() != null)
        {
            EnsureCameraReserved();
        }
    }

    PlayerController GetPlayer()
    {
        // Re-find after a scene reload (death/restart) destroys the old player.
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
            }
        }
        return player;
    }

    // Fraction of the screen height the bar occupies. Derived so the bar is
    // exactly one rendered grid cell tall plus clearance on each side, once the
    // camera has been shrunk into the remaining (1 - fraction) of the screen.
    static float BarFraction(float orthographicSize)
    {
        // box = (1 - f) * H / (2 * ortho),  barHeight = f * H
        // barHeight = box * (1 + 2 * clearance)
        // => f = (1 + 2 * clearance) / (2 * ortho + 1 + 2 * clearance)
        float c = 1f + 2f * ClearanceFraction;
        float fraction = c / (2f * orthographicSize + c);
        return Mathf.Clamp(fraction, 0f, 0.45f);
    }

    // Confine the current scene's camera to the area below the bar.
    // Shrink-to-fit: keep the full view, just render it into the bottom strip.
    void EnsureCameraReserved()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            return;
        }
        if (cam == adjustedCamera)
        {
            return; // already reserved this camera
        }
        adjustedCamera = cam;

        float fraction = BarFraction(cam.orthographicSize);
        cam.rect = new Rect(0f, 0f, 1f, 1f - fraction);
    }

    void OnGUI()
    {
        PlayerController currentPlayer = GetPlayer();

        // No player in this scene (e.g. title screen) -> no status bar.
        if (currentPlayer == null)
        {
            return;
        }

        // Make sure the camera has already yielded its top strip.
        EnsureCameraReserved();

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        // One grid cell in screen pixels, at the camera's rendered scale.
        // cam.pixelHeight already accounts for the reserved viewport rect, so
        // this stays exactly one on-screen grid square.
        float boxSize = cam.pixelHeight / (2f * cam.orthographicSize);

        // The reserved strip is everything above the camera's viewport.
        float barHeight = Screen.height - cam.pixelHeight;
        float clearance = (barHeight - boxSize) * 0.5f;

        // Black status band across the top of the screen.
        GUI.DrawTexture(new Rect(0, 0, Screen.width, barHeight), barTexture);

        // Item box: inset from the left edge, vertically centered in the band.
        Rect boxRect = new Rect(clearance, clearance, boxSize, boxSize);

        // Draw the border, then the inner background inset by the border width.
        float border = Mathf.Max(2f, boxSize * 0.06f);
        GUI.DrawTexture(boxRect, boxBorderTexture);
        Rect innerRect = new Rect(
            boxRect.x + border,
            boxRect.y + border,
            boxRect.width - border * 2f,
            boxRect.height - border * 2f);
        GUI.DrawTexture(innerRect, boxBackgroundTexture);

        // Held item sprite, if any.
        Sprite itemSprite = GetHeldItemSprite(currentPlayer);
        if (itemSprite != null)
        {
            DrawSprite(itemSprite, innerRect);
        }
    }

    Sprite GetHeldItemSprite(PlayerController currentPlayer)
    {
        if (currentPlayer.currentItem == Item.None || currentPlayer.currentItemController == null)
        {
            return null;
        }

        // The pickup interface doesn't expose a sprite, so pull it from the
        // controller's SpriteRenderer. Works for any current/future pickup.
        MonoBehaviour controller = currentPlayer.currentItemController as MonoBehaviour;
        if (controller == null)
        {
            return null;
        }

        SpriteRenderer renderer = controller.GetComponent<SpriteRenderer>();
        return renderer != null ? renderer.sprite : null;
    }

    void DrawSprite(Sprite sprite, Rect area)
    {
        Texture texture = sprite.texture;
        Rect tr = sprite.textureRect;
        Rect texCoords = new Rect(
            tr.x / texture.width,
            tr.y / texture.height,
            tr.width / texture.width,
            tr.height / texture.height);

        // Inset slightly so the sprite doesn't touch the box border.
        float margin = area.width * 0.12f;
        Rect fit = new Rect(
            area.x + margin,
            area.y + margin,
            area.width - margin * 2f,
            area.height - margin * 2f);
        GUI.DrawTextureWithTexCoords(fit, texture, texCoords);
    }
}
