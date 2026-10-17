using UnityEngine;

public class ShadowState : MonoBehaviour
{
    private int lightSourcesTouching = 0;

    public bool IsLit
    {
        get { return lightSourcesTouching > 0; }
    }

    public void EnterFirelight()
    {
        lightSourcesTouching++;
    }

    public void ExitFirelight()
    {
        lightSourcesTouching--;

        if (lightSourcesTouching < 0)
            lightSourcesTouching = 0;

    }

    public void ResetLightState()
    {
        lightSourcesTouching = 0;
    }
}