using System;
using UnityEngine;

[Serializable]
public class BridgeCustomData
{
    public string token;
    public string id;
    public string name;
    public string customName;
    public string color;
    public string material;
    public string size;
    public string modelType;
}

public class BridgeObject : MonoBehaviour
{
    public CustomizerUI customizerUI;
    public BridgeCustomData bridgeCustomData;
    
    public void ReceiveCustomData(string customData)
    {
        bridgeCustomData = JsonUtility.FromJson<BridgeCustomData>(customData);
        customizerUI.InitData(bridgeCustomData);
    }
}
