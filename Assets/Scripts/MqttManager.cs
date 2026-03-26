using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using System;

public class MqttManager : MonoBehaviour
{
    private MqttClient client;
    public string brokerIp = "100.84.227.69"; // Cambia con l'IP del tuo n8n/Broker
    public string topic = "home/unity/commands";

    public ActuatorsManager actuatorsManager;
    public List<DeviceMapping> registry; // Il tuo "dizionario" di ieri

    // Coda per gestire i messaggi tra i thread
    private Queue<string> messageQueue = new Queue<string>();

    void Awake()
    {
        Application.runInBackground = true;
    }

    void Start()
    {
        try {
            client = new MqttClient(brokerIp);
            client.MqttMsgPublishReceived += OnMessageReceived;
            client.Connect(Guid.NewGuid().ToString());
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            Debug.Log("Connesso al Broker MQTT!");
        } catch (Exception e) {
            Debug.LogError("Errore connessione MQTT: " + e.Message);
        }
    }

    // Questo corre sul thread MQTT (NON toccare Unity qui!)
    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        lock (messageQueue) {
            messageQueue.Enqueue(msg);
        }
    }

    void Update()
    {
        // Questo corre sul Main Thread (Qui Unity è al sicuro)
        lock (messageQueue) {
            while (messageQueue.Count > 0) {
                ProcessMessage(messageQueue.Dequeue());
            }
        }
    }

    void ProcessMessage(string json)
    {
        Debug.Log("Elaborazione comando: " + json);
        try {
            MqttCommand cmd = JsonUtility.FromJson<MqttCommand>(json);
            
            // Cerchiamo nel tuo registry l'oggetto che ha quel nome HA
            DeviceMapping map = registry.Find(
                x => 
                     x.haRoomName == cmd.haRoomName && 
                     x.haDeviceName == cmd.haDeviceName);

            if (map != null) {

                switch (map.deviceType) {
                    case DeviceType.Light:
                        actuatorsManager.SetLight(map.deviceObject.GetComponent<Light>(), true, cmd.intensity, cmd.colorTemperature);
                        break;
                    case DeviceType.RollerShutter:
                    case DeviceType.DarkeningGlass:
                        actuatorsManager.SetCover(map.deviceObject, map.deviceType, cmd.percentage);
                        break;
                    case DeviceType.GenericBinary:
                        actuatorsManager.SetBinaries(map.deviceObject, cmd.state);
                        break;
                }
            }
        } catch (Exception e) {
            Debug.LogError("Errore parsing JSON: " + e.Message);
        }
    }
}

[Serializable]
public class MqttCommand 
{
    public string haDeviceName;
    public string haRoomName;
    public bool state = false;    
    public int intensity = -1;   
    public float percentage = 0f;   
    public int colorTemperature = -1;    
    public string effect = "";    
}
