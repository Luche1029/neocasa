using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActuatorsManager : MonoBehaviour
{ 

    private Coroutine coverCoroutine;

    public void SetLight(Light light, bool? isTurnedOn = null, float? intensity = null, float? colorTemp = null)
    {
        if (isTurnedOn.HasValue)
            light.enabled = isTurnedOn.Value;

        if (!light.enabled) return;

        if (intensity.HasValue)
            light.intensity = intensity.Value;

        if (colorTemp.HasValue)        
            light.colorTemperature = colorTemp.Value;
        
    }

    public void SetCover(GameObject coverObject, DeviceType type, float targetValue)
    {
        if (coverCoroutine != null) StopCoroutine(coverCoroutine);        
        coverCoroutine = StartCoroutine(AnimateCover(coverObject, type, targetValue));
    }

    IEnumerator AnimateCover(GameObject obj, DeviceType type, float targetValue)
    {
        float duration = 2.0f; 
        float elapsed = 0;

        float normalizedTarget = targetValue / 100f;

        // --- RILEVAMENTO STATO INIZIALE ---
        float startYScale = obj.transform.localScale.y;
        float startAlpha = 1.0f; // Default se non troviamo il renderer

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            startAlpha = rend.material.color.a;
        }

        // --- LOOP DI ANIMAZIONE ---
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; 

            if (type == DeviceType.RollerShutter)
            {
                float currentY = Mathf.Lerp(startYScale, normalizedTarget, t);
                Vector3 s = obj.transform.localScale;
                s.y = currentY;
                obj.transform.localScale = s;
            }
            else if (type == DeviceType.DarkeningGlass)
            {
                if (rend != null)
                {
                    float targetAlpha = Mathf.Lerp(1.0f, 0.1f, normalizedTarget);
                    float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                    Color c = rend.material.color;
                    c.a = currentAlpha;
                    rend.material.color = c;
                }
            }

            yield return null; // Aspetta il frame successivo
        }

        // --- STATO FINALE (per precisione) ---
        if (type == DeviceType.RollerShutter)
        {
            Vector3 s = obj.transform.localScale;
            s.y = normalizedTarget;
            obj.transform.localScale = s;
        }
        else if (type == DeviceType.DarkeningGlass && rend != null)
        {
            float targetAlpha = Mathf.Lerp(1.0f, 0.1f, normalizedTarget);
            Color c = rend.material.color;
            c.a = targetAlpha;
            rend.material.color = c;
        }


    }


    public void SetBinaries(GameObject device, bool isTurnedOn)
    {
        BaseActuator actuator = device.GetComponent<BaseActuator>();

        if (actuator != null)
        {
            actuator.SetState(isTurnedOn);
        }            
    }

    public void SetRange(GameObject device, float value)
    {
        
        BaseActuator actuator = device.GetComponent<BaseActuator>();
        if (actuator != null)
        {
            actuator.SetState(value);
        }      
    }
}

public abstract class BaseActuator : MonoBehaviour
{
    public virtual void SetState(bool isOn) { }
    public virtual void SetState(float value) { }
    public virtual void SetState(Color color) { }
}

