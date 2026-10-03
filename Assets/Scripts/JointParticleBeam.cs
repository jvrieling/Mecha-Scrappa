using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class JointParticleBeam : MonoBehaviour
{
    [Header("Edit Mode Preview")]
    [SerializeField] private bool previewInEditMode = false;
    [SerializeField] private Transform testStartPoint;
    [SerializeField] private Transform testEndPoint;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();

#if UNITY_EDITOR
        // Ensure the editor scene view continuously repaints while editing particles
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }

    private void OnEditorUpdate()
    {
        // Preview particle line in Scene view during Edit Mode when enabled
        if (!Application.isPlaying && previewInEditMode && testStartPoint != null && testEndPoint != null)
        {
            RenderBeam(testStartPoint.position, testEndPoint.position);
        }
    }

    /// <summary>
    /// Emits and places particles evenly along a straight line between two points.
    /// Works in both Play Mode and Edit Mode.
    /// </summary>
    public void RenderBeam(Vector3 startPoint, Vector3 endPoint, int particleCount = 12)
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();

        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }

        float step = 1f / Mathf.Max(1, particleCount - 1);
        Color startColor = Color.cyan;
        Color endColor = Color.white;

        for (int i = 0; i < particleCount; i++)
        {
            float t = i * step;
            particles[i].position = Vector3.Lerp(startPoint, endPoint, t);
            particles[i].startColor = Color.Lerp(startColor, endColor, t);
            particles[i].startSize = 0.08f;
            particles[i].remainingLifetime = 0.1f;
            particles[i].startLifetime = 0.1f;
        }

        ps.SetParticles(particles, particleCount);

#if UNITY_EDITOR
        // Force the Scene View to repaint immediately in Edit Mode
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
#endif
    }

    public void ClearBeam()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        ps.Clear();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
#endif
    }
}