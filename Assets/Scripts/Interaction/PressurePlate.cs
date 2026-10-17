using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Single Door")]
    public DoorController door;

    [Header("Multi-Plate Door")]
    public MultiPlateDoor multiPlateDoor;
    public bool isPlateA = true;

    [Header("Shadow Rules")]
    public bool requireFirelight = true;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip activationClip;
    public Renderer indicatorRenderer;
    public Light indicatorLight;
    [ColorUsage(true, true)] public Color inactiveColor = new Color(0.08f, 0.07f, 0.06f, 1f);
    [ColorUsage(true, true)] public Color activeColor = new Color(2.8f, 0.65f, 0.08f, 1f);

    private readonly HashSet<Collider> validCollidersOnPlate = new HashSet<Collider>();
    private bool lastActive;
    private Material runtimeMaterial;

    void Awake()
    {
        if (indicatorRenderer != null) runtimeMaterial = indicatorRenderer.material;
        RefreshVisual(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AddValidCollider(other);
            return;
        }

        if (other.CompareTag("Shadow"))
        {
            if (!requireFirelight)
            {
                AddValidCollider(other);
                return;
            }

            ShadowState shadowState = other.GetComponent<ShadowState>();
            if (shadowState != null && shadowState.IsLit)
                AddValidCollider(other);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Shadow") || !requireFirelight)
            return;

        ShadowState shadowState = other.GetComponent<ShadowState>();
        bool valid = shadowState != null && shadowState.IsLit;
        if (valid && !validCollidersOnPlate.Contains(other))
            AddValidCollider(other);
        else if (!valid && validCollidersOnPlate.Remove(other))
            UpdateOutput();
    }

    void OnTriggerExit(Collider other)
    {
        if (validCollidersOnPlate.Remove(other))
            UpdateOutput();
    }

    void AddValidCollider(Collider other)
    {
        if (validCollidersOnPlate.Add(other))
            UpdateOutput();
    }

    void UpdateOutput()
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
            return;
        }

        if (door != null)
            door.SetOpen(active);
    }

    public void ResetPlate()
    {
        validCollidersOnPlate.Clear();
        lastActive = false;
        UpdateOutput();
    }

    private void RefreshVisual(bool active)
    {
        if (runtimeMaterial != null)
        {
            if (runtimeMaterial.HasProperty("_BaseColor"))
                runtimeMaterial.SetColor("_BaseColor", active ? new Color(0.32f, 0.12f, 0.03f, 1f) : inactiveColor);
            if (runtimeMaterial.HasProperty("_EmissionColor"))
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
                runtimeMaterial.SetColor("_EmissionColor", active ? activeColor : Color.black);
            }
        }
        if (indicatorLight != null)
        {
            indicatorLight.enabled = active;
            indicatorLight.intensity = active ? 2.8f : 0f;
        }
    }
}
