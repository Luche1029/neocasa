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
        // Se c'è già un movimento in corso, lo fermiamo per evitare "tremolii"
        if (coverCoroutine != null) StopCoroutine(coverCoroutine);
        
        coverCoroutine = StartCoroutine(AnimateCover(coverObject, type, targetValue));
    }

IEnumerator AnimateCover(GameObject obj, DeviceType type, float targetValue)
    {
        float duration = 2.0f; // Durata dell'animazione in secondi (regolabile)
        float elapsed = 0;

        // Normalizziamo il target: HA 100 (aperto) -> Unity Valore Basso (0 per scala, 0.1f per alpha)
        // HA 0 (chiuso) -> Unity Valore Alto (1 per scala, 1.0f per alpha)
        float normalizedTarget = targetValue / 100f;
        float invertedTarget = 1f - normalizedTarget;

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
            float t = elapsed / duration; // Fattore di completamento (0.0f a 1.0f)

            // Usiamo una curva di smoothing (opzionale, rende il movimento più naturale)
            // t = t * t * (3f - 2f * t); 

            if (type == DeviceType.RollerShutter)
            {
                // Interpolazione lineare della scala Y
                float currentY = Mathf.Lerp(startYScale, invertedTarget, t);
                Vector3 s = obj.transform.localScale;
                s.y = currentY;
                obj.transform.localScale = s;
            }
            else if (type == DeviceType.DarkeningGlass)
            {
                if (rend != null)
                {
                    // Interpolazione lineare dell'Alpha
                    // Target Alpha: 0.1f (trasparente) se aperto, 1.0f (opaco) se chiuso
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
            s.y = invertedTarget;
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

public enum DeviceType
{
    Light,
    RollerShutter,
    DarkeningGlass,
    GenericBinary,
    GenericRange
}

[System.Serializable]
public class DeviceMapping
{
    public string haDeviceName;          
    public string haRoomName;
    public DeviceType deviceType;    
    public GameObject deviceObject; 
    public Button testButton;
}