using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using System;

public class MqttManager : MonoBehaviour
{
    private MqttClient client;
    public string brokerIp = "100.84.227.69"; 
    public string topic = "home/unity/commands";

    public ActuatorsManager actuatorsManager;
    public List<DeviceMapping> registry; 

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
        Debug.Log($"json: {json}");
        try {
            MqttCommand cmd = JsonUtility.FromJson<MqttCommand>(json);
            
            // Cerchiamo nel tuo registry l'oggetto che ha quel nome HA
            DeviceMapping map = registry.Find(
                x => 
                     x.haRoomName == cmd.haRoomName && 
                     x.haDeviceName == cmd.haDeviceName);

            if (map != null) {
                DeviceType deviceType = map.deviceType;
                CommandAction action = cmd.action;
                switch (deviceType) {
                    case DeviceType.Light:
                        Light light = map.deviceObject.GetComponent<Light>();
                        switch (action)
                        {
                            case CommandAction.TURN_ON:
                                actuatorsManager.SetLight(light, true, 100);
                                break;
                            case CommandAction.TURN_OFF:
                                actuatorsManager.SetLight(light, false);
                                break;
                            case CommandAction.SET_BRIGHTNESS:
                                actuatorsManager.SetLight(light, true, cmd.intensity);
                                break;
                        }
                        break;
                    case DeviceType.RollerShutter:
                    case DeviceType.DarkeningGlass:
                        GameObject ccover = map.deviceObject;
                        switch (action)
                        {
                            case CommandAction.OPEN:
                                actuatorsManager.SetCover(ccover, deviceType, 100);
                                break;
                            case CommandAction.CLOSE:
                                actuatorsManager.SetCover(ccover, deviceType, 0);
                                break;
                            case CommandAction.SET_POSITION:
                                actuatorsManager.SetCover(ccover, deviceType, cmd.percentage);
                                break;
                        }
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
public enum DeviceType
{
    Light,
    RollerShutter,
    DarkeningGlass,
    GenericBinary,
    GenericRange
}

public enum CommandAction
{
    TURN_ON,
    TURN_OFF,
    TOGGLE,
    SET_BRIGHTNESS,
    OPEN,
    CLOSE,
    SET_POSITION,
    SET_TEMPERATURE,
    SET_HVAC_MODE,
    PLAY,
    PAUSE,
    STOP,
    VOLUME_SET
}

[Serializable]
public class DeviceMapping
{
    public string haDeviceName;          
    public string haRoomName;
    public DeviceType deviceType;    
    public GameObject deviceObject; 
}


[Serializable]
public class MqttCommand 
{
    public string haDeviceName;
    public string haRoomName;
    public DeviceType type;
    public CommandAction action;
    public bool state = false;    
    public int intensity = -1;   
    public float percentage = 0f;   
    public int colorTemperature = -1;    
    public string effect = "";    
}
