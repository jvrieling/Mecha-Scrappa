using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class JointParticleBeam : MonoBehaviour
{
    [Header("Beam Color Settings")]
    public Color startColor = Color.yellow;
    public Color endColor = Color.white;

    [Header("Edit Mode Preview")]
    [SerializeField] private bool previewInEditMode = false;
    [SerializeField] private Transform testStartPoint;
    [SerializeField] private Transform testEndPoint;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void OnEnable()
    {
        InitializeParticleSystem();

#if UNITY_EDITOR
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }

    private void InitializeParticleSystem()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();

        // Ensure particles render in World Space so world coordinates map correctly
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
    }

    private void OnEditorUpdate()
    {
        if (!Application.isPlaying && previewInEditMode && testStartPoint != null && testEndPoint != null)
        {
            RenderBeam(testStartPoint.position, testEndPoint.position);
        }
    }

    /// <summary>
    /// Emits and places particles evenly along a straight line between two points in World Space.
    /// </summary>
    public void RenderBeam(Vector3 startPoint, Vector3 endPoint, int particleCount = 12)
    {
        if (ps == null) InitializeParticleSystem();

        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }

        float step = 1f / Mathf.Max(1, particleCount - 1);

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
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
#endif
    }

    public void ClearBeam()
    {
        if (ps == null) InitializeParticleSystem();
        ps.Clear();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
#endif
    }
}