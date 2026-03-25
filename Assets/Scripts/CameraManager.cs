using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public GameObject cameraContainer;
    public Transform buttonParent;
    public GameObject buttonPrefab;

    void Start()
    {
        if (cameraContainer == null || buttonParent == null || buttonPrefab == null)
            return;
        GenerateButtons();
    }

    // Update is called once per frame
    void GenerateButtons()
    {
        foreach(Transform child in cameraContainer.transform)
        {
            Camera camera = child.GetComponent<Camera>();
            if (camera == null) continue;
            GameObject newButton = Instantiate(buttonPrefab, buttonParent);
            
            newButton.GetComponentInChildren<TMP_Text>().text = child.name.Replace("Camera", "");

            newButton.GetComponent<Button>().onClick.AddListener(() => SwitchToCamera(camera));
        }
    }

    void SwitchToCamera(Camera targetCamera)
    {
        // Disattiva tutte le camere nel container
        foreach (Transform child in cameraContainer.transform)
        {
            Camera c = child.GetComponent<Camera>();
            if (c != null) c.enabled = false;
        }

        // Attiva la camera selezionata
        targetCamera.enabled = true;
        Debug.Log("Passato a: " + targetCamera.name);
    }
}
