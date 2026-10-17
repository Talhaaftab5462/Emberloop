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
        Color idle = requireFirelight
            ? new Color(1.2f, .14f, .015f, 1f)
            : new Color(.03f, .25f, 1.1f, 1f);

        Color active = requireFirelight
            ? new Color(4.5f, 1.0f, .08f, 1f)
            : new Color(.15f, 1.25f, 3.6f, 1f);

        if (runtimeMaterial != null)
        {
            runtimeMaterial.EnableKeyword("_EMISSION");
            if (runtimeMaterial.HasProperty("_EmissionColor"))
                runtimeMaterial.SetColor("_EmissionColor", IsActive ? active : idle);
        }

        if (indicatorLight != null)
        {
            indicatorLight.enabled = true;
            indicatorLight.color = requireFirelight ? new Color(1f, .22f, .03f) : new Color(.1f, .42f, 1f);
            indicatorLight.intensity = IsActive ? 4f : .55f;
            indicatorLight.shadows = LightShadows.None;
        }
    }
}
