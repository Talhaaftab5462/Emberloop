using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinalAnchor : MonoBehaviour
{
    public bool requireFirelight;
    public FinalChamberController controller;
    public Renderer indicatorRenderer;
    public Light indicatorLight;
    public AudioSource audioSource;
    public AudioClip activationClip;

    [ColorUsage(true, true)] public Color inactiveEmission = new Color(0.03f, 0.01f, 0.005f, 1f);
    [ColorUsage(true, true)] public Color activeEmission = new Color(4.5f, 1.2f, 0.12f, 1f);

    private readonly HashSet<Collider> activeShadows = new HashSet<Collider>();
    private Material runtimeMaterial;

    public bool IsActive => activeShadows.Count > 0;

    void Awake()
    {
        if (indicatorRenderer != null)
            runtimeMaterial = indicatorRenderer.material;
        RefreshVisual();
    }

    void OnTriggerEnter(Collider other) => TryRefreshShadow(other);
    void OnTriggerStay(Collider other) => TryRefreshShadow(other);

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Shadow")) return;
        if (activeShadows.Remove(other))
        {
            RefreshVisual();
            controller?.NotifyAnchorStateChanged();
        }
    }

    private void TryRefreshShadow(Collider other)
    {
        if (!other.CompareTag("Shadow")) return;

        ShadowState shadowState = other.GetComponent<ShadowState>();
        bool valid = !requireFirelight || (shadowState != null && shadowState.IsLit);
        bool wasActive = IsActive;
        bool changed = valid ? activeShadows.Add(other) : activeShadows.Remove(other);

        if (changed)
        {
            RefreshVisual();
            if (!wasActive && IsActive && audioSource != null && activationClip != null)
                audioSource.PlayOneShot(activationClip, 1f);
            controller?.NotifyAnchorStateChanged();
        }
    }

    public void ResetAnchor()
    {
        activeShadows.Clear();
        RefreshVisual();
        controller?.NotifyAnchorStateChanged();
    }

    private void RefreshVisual()
    {
        if (runtimeMaterial != null)
        {
            runtimeMaterial.EnableKeyword("_EMISSION");
            runtimeMaterial.SetColor("_EmissionColor", IsActive ? activeEmission : inactiveEmission);
        }

        if (indicatorLight != null)
        {
            indicatorLight.enabled = IsActive;
            indicatorLight.intensity = IsActive ? 4f : 0f;
        }
    }
}
