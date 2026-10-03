using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D starfield with randomized parallax.
/// - Each star is its own sprite object, picked at random from a sprite list.
/// - Each star gets a random parallax factor (copies a fraction of camera movement).
///   Higher factor = further away = drifts slower on screen, smaller and dimmer.
/// - Stars that leave the camera view (plus a margin) disappear. Optionally they
///   respawn outside the view so the field never runs out.
/// Works with an orthographic camera.
/// </summary>
[DefaultExecutionOrder(100)] // run after most camera-follow scripts
public class StarfieldParallax : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private List<Sprite> starSprites = new List<Sprite>();

    [Header("Population")]
    [SerializeField] private int starCount = 150;
    [Tooltip("Stars further than this (world units) beyond the viewport edge disappear.")]
    [SerializeField] private float despawnMargin = 4f;
    [Tooltip("ON: despawned stars respawn off-screen. OFF: they are gone for good.")]
    [SerializeField] private bool respawnStars = true;

    [Header("Parallax")]
    [Tooltip("Fraction of camera movement a star copies. 0 = fixed in world, 1 = glued to screen.")]
    [SerializeField, Range(0f, 1f)] private float minParallax = 0.5f;
    [SerializeField, Range(0f, 1f)] private float maxParallax = 0.95f;

    [Header("Look (near -> far)")]
    [SerializeField] private Vector2 nearScaleRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private Vector2 farScaleRange = new Vector2(0.2f, 0.5f);
    [SerializeField, Range(0f, 1f)] private float nearAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float farAlpha = 0.35f;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private bool randomFlip = true;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = -100;

    private class Star
    {
        public Transform t;
        public SpriteRenderer sr;
        public float parallax;
    }

    private readonly List<Star> stars = new List<Star>();
    private Vector3 lastCamPos;

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        if (targetCamera == null || starSprites == null || starSprites.Count == 0)
        {
            Debug.LogWarning("StarfieldParallax: assign a camera and at least one star sprite.", this);
            enabled = false;
            return;
        }

        lastCamPos = targetCamera.transform.position;

        Rect spawnArea = GetCameraRect(despawnMargin);
        for (int i = 0; i < starCount; i++)
        {
            Star s = CreateStar();
            // Initial fill: scatter across the whole area, including inside the view.
            Place(s, RandomPointIn(spawnArea));
            stars.Add(s);
        }
    }

    private void LateUpdate()
    {
        Vector3 camPos = targetCamera.transform.position;
        Vector2 camDelta = camPos - lastCamPos;
        lastCamPos = camPos;

        Rect keepArea = GetCameraRect(despawnMargin);

        for (int i = 0; i < stars.Count; i++)
        {
            Star s = stars[i];
            if (!s.t.gameObject.activeSelf) continue;

            // Parallax: copy a fraction of the camera's movement.
            Vector3 p = s.t.position;
            p.x += camDelta.x * s.parallax;
            p.y += camDelta.y * s.parallax;
            s.t.position = p;

            // Too far outside the viewport? It disappears.
            if (!keepArea.Contains(p))
            {
                if (respawnStars)
                    Randomize(s, RandomPointOutsideView(keepArea));
                else
                    s.t.gameObject.SetActive(false);
            }
        }
    }

    // ---------- Star creation / randomization ----------

    private Star CreateStar()
    {
        GameObject go = new GameObject("Star");
        go.transform.SetParent(transform, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        return new Star { t = go.transform, sr = sr };
    }

    private void Place(Star s, Vector2 position)
    {
        Randomize(s, position);
    }

    private void Randomize(Star s, Vector2 position)
    {
        s.parallax = Random.Range(minParallax, maxParallax);

        // depth: 0 = nearest, 1 = farthest (based on this star's parallax)
        float depth = Mathf.Approximately(maxParallax, minParallax)
            ? 0.5f
            : Mathf.InverseLerp(minParallax, maxParallax, s.parallax);

        s.sr.sprite = starSprites[Random.Range(0, starSprites.Count)];

        float nearScale = Random.Range(nearScaleRange.x, nearScaleRange.y);
        float farScale = Random.Range(farScaleRange.x, farScaleRange.y);
        float scale = Mathf.Lerp(nearScale, farScale, depth);
        s.t.localScale = new Vector3(scale, scale, 1f);

        Color c = s.sr.color;
        c.a = Mathf.Lerp(nearAlpha, farAlpha, depth);
        s.sr.color = c;

        s.sr.flipX = randomFlip && Random.value > 0.5f;
        s.sr.flipY = randomFlip && Random.value > 0.5f;
        s.t.rotation = randomRotation
            ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
            : Quaternion.identity;

        s.t.position = new Vector3(position.x, position.y, 0f);
        s.t.gameObject.SetActive(true);
    }

    // ---------- Camera / area helpers ----------

    private Rect GetCameraRect(float margin)
    {
        float halfH = targetCamera.orthographicSize + margin;
        float halfW = targetCamera.orthographicSize * targetCamera.aspect + margin;
        Vector3 c = targetCamera.transform.position;
        return new Rect(c.x - halfW, c.y - halfH, halfW * 2f, halfH * 2f);
    }

    private static Vector2 RandomPointIn(Rect r)
    {
        return new Vector2(
            Random.Range(r.xMin, r.xMax),
            Random.Range(r.yMin, r.yMax));
    }

    /// <summary>Random point inside the keep-area but outside the visible viewport,
    /// so respawned stars never pop in on screen.</summary>
    private Vector2 RandomPointOutsideView(Rect keepArea)
    {
        Rect view = GetCameraRect(0.5f); // small buffer so sprite edges don't pop in

        for (int i = 0; i < 12; i++)
        {
            Vector2 p = RandomPointIn(keepArea);
            if (!view.Contains(p)) return p;
        }

        // Fallback: just outside the left edge.
        return new Vector2(
            view.xMin - Random.Range(0f, despawnMargin * 0.5f),
            Random.Range(keepArea.yMin, keepArea.yMax));
    }
}