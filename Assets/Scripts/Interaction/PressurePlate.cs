using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Single Door")]
    public DoorController door;

    [Header("Multi-Plate Door")]
    public MultiPlateDoor multiPlateDoor;
    public bool isPlateA = true;

    [Header("Fire Rule")]
    public bool requireFirelight;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip activationClip;
    public Renderer indicatorRenderer;
    public Light indicatorLight;

    private readonly HashSet<Collider> validCollidersOnPlate = new HashSet<Collider>();
    private bool lastActive;
    private Material runtimeMaterial;

    void Awake()
    {
        if (indicatorRenderer != null)
            runtimeMaterial = indicatorRenderer.material;
        RefreshVisual(false);
    }

    void OnTriggerEnter(Collider other) => RefreshCollider(other);
    void OnTriggerStay(Collider other) => RefreshCollider(other);

    void OnTriggerExit(Collider other)
    {
        if (validCollidersOnPlate.Remove(other))
            UpdateOutput();
    }

    private void RefreshCollider(Collider other)
    {
        bool relevant = other.CompareTag("Player") || other.CompareTag("Shadow");
        if (!relevant) return;

        bool valid = !requireFirelight;
        if (requireFirelight)
        {
            if (other.CompareTag("Shadow"))
            {
                ShadowState state = other.GetComponent<ShadowState>();
                valid = state != null && state.IsLit;
            }
            else
            {
                valid = FireLightZone.IsPointInsideAnyFirelight(other.transform.position + Vector3.up * 0.6f);
            }
        }

        bool contains = validCollidersOnPlate.Contains(other);
        if (valid && !contains)
        {
            validCollidersOnPlate.Add(other);
            UpdateOutput();
        }
        else if (!valid && contains)
        {
            validCollidersOnPlate.Remove(other);
            UpdateOutput();
        }
    }

    private void UpdateOutput()
    {
        bool active = validCollidersOnPlate.Count > 0;

        if (active && !lastActive && audioSource != null && activationClip != null)
            audioSource.PlayOneShot(activationClip, 0.8f);

        lastActive = active;
        RefreshVisual(active);

        if (multiPlateDoor != null)
        {
            if (isPlateA) multiPlateDoor.SetPlateA(active);
            else multiPlateDoor.SetPlateB(active);
        }
        else if (door != null)
        {
            door.SetOpen(active);
        }
    }

    public void ResetPlate()
    {
        validCollidersOnPlate.Clear();
        lastActive = false;
        UpdateOutput();
    }

    private void RefreshVisual(bool active)
    {
        Color idle = requireFirelight ? new Color(1.4f, .14f, .015f, 1f) : new Color(.02f, .3f, 1.25f, 1f);
        Color lit = requireFirelight ? new Color(4.5f, 1.0f, .04f, 1f) : new Color(.12f, 1.25f, 3.8f, 1f);

        if (runtimeMaterial != null)
        {
            runtimeMaterial.EnableKeyword("_EMISSION");
            if (runtimeMaterial.HasProperty("_EmissionColor"))
                runtimeMaterial.SetColor("_EmissionColor", active ? lit : idle);
        }

        if (indicatorLight != null)
        {
            indicatorLight.enabled = true;
            indicatorLight.color = requireFirelight ? new Color(1f, .23f, .02f) : new Color(.08f, .42f, 1f);
            indicatorLight.intensity = active ? 2.8f : .35f;
            indicatorLight.shadows = LightShadows.None;
        }
    }
}
