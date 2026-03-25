using System.Collections.Generic;
using UnityEngine;

public class QuickTestManager : MonoBehaviour
{
    public List<DeviceMapping> registry; 
    public ActuatorsManager actuatorsManager;

    void Start()
    {
        foreach(DeviceMapping dev in registry)
        {
            DeviceMapping currentDev = dev;
            switch(currentDev.deviceType)
            {
                case DeviceType.Light:
                    currentDev.testButton.onClick.RemoveAllListeners();
                    currentDev.testButton.onClick.AddListener(() => {
                        // Recuperiamo il componente Light dall'oggetto
                        Light lightComponent = currentDev.deviceObject.GetComponent<Light>();

                        if (lightComponent != null) {
                            var i = Random.Range(0, 40000);
                            var t = Random.Range(1500, 20000);
                            // Ora passiamo il componente Light vero e proprio
                            actuatorsManager.SetLight(lightComponent, true, i, t);
                        } else {
                            Debug.LogError($"L'oggetto {currentDev.deviceObject.name} non ha un componente Light!");
                        }
                    });
                    break;
                case DeviceType.DarkeningGlass:
                case DeviceType.RollerShutter:
                    currentDev.testButton.onClick.RemoveAllListeners();
                    currentDev.testButton.onClick.AddListener(
                        () => 
                        {
                            var c = Random.Range(0, 100);
                            actuatorsManager.SetCover(currentDev.deviceObject, currentDev.deviceType, c);
                        }
                    );
                    break;
                case DeviceType.GenericBinary:
                    currentDev.testButton.onClick.RemoveAllListeners();
                    currentDev.testButton.onClick.AddListener(
                        () => 
                        {
                            var t = Random.value > 0.5f ? true: false;
                            actuatorsManager.SetBinaries(currentDev.deviceObject, t);
                        }
                    );
                    break;
                case DeviceType.GenericRange:
                    currentDev.testButton.onClick.RemoveAllListeners();
                    currentDev.testButton.onClick.AddListener(
                        () => 
                        {
                            var c = Random.Range(0, 100);
                            actuatorsManager.SetRange(currentDev.deviceObject, c);
                        }
                    );
                    break;
            }
        }
    }

    
}