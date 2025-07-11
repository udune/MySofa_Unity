using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[Serializable]
public class CustomData
{
    public string name;
    public string customName;
    public string color;
    public string material;
    public string size;
    public string model;
}

public class CustomizerUI : MonoBehaviour
{
    private static readonly int _Color = Shader.PropertyToID("_BaseColor");
    private static readonly int _Smoothness = Shader.PropertyToID("_Smoothness");

    [Header("제품명")] 
    public TMP_InputField productNameField;
    
    [Header("색상")] 
    public Toggle beigeToggle;
    public Toggle grayToggle;
    public Toggle blackToggle;
    
    [Header("소재")]
    public Toggle fabricToggle;
    public Toggle leatherToggle;

    [Header("사이즈")] 
    public Toggle smallToggle;
    public Toggle largeToggle;

    [Header("모델")] 
    public Toggle aToggle;
    public Toggle bToggle;
    
    [Header("토스트")]
    public GameObject toast;
    public TMP_Text toastText;

    private string selectedColor = "";
    private string selectedMaterial = "";
    private string selectedSize = "";
    private string selectedModelType = "";

    private CustomData data = new CustomData();

    private GameObject modelGo;
    public SofaModel model;
    
    private readonly Color beigeColor = new(0.96f, 0.86f, 0.71f);
    private readonly Color grayColor = new(0.5f, 0.5f, 0.5f);
    private readonly Color blackColor = new(0.2f, 0.2f, 0.2f);

    public void InitData(BridgeCustomData bridgeCustomData)
    {
        modelGo = Instantiate(Resources.Load<GameObject>($"{bridgeCustomData.name}_{bridgeCustomData.modelType}"));
        
        selectedColor = bridgeCustomData.color;
        selectedMaterial = bridgeCustomData.material;
        selectedSize = bridgeCustomData.size;
        selectedModelType = bridgeCustomData.modelType;
        
        productNameField.text = bridgeCustomData.customName;
        SetColor(bridgeCustomData.color);
        SetMaterial(bridgeCustomData.material);
        SetSize(bridgeCustomData.size);
        SwitchModel(bridgeCustomData.modelType);
    }
    
    private void Start()
    {
        RegisterToggle(beigeToggle, () => SetColor("beige"));
        RegisterToggle(grayToggle, () => SetColor("gray"));
        RegisterToggle(blackToggle, () => SetColor("black"));
        
        RegisterToggle(fabricToggle, () => SetMaterial("fabric"));
        RegisterToggle(leatherToggle, () => SetMaterial("leather"));
        
        RegisterToggle(smallToggle, () => SetSize("small"));
        RegisterToggle(largeToggle, () => SetSize("large"));
        
        RegisterToggle(aToggle, () => SwitchModel("a"));
        RegisterToggle(bToggle, () => SwitchModel("b"));
    }

    private void Update()
    {
#if UNITY_EDITOR
        for (int i = 0; i <= 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                LoadTest(i);
            }
        }
#endif
    }

    private void LoadTest(int idx)
    {
        string[] modelNames =
        {
            "privateSofa_a", "privateSofa_b",
            "classicSofa_a", "classicSofa_b",
            "modularSofa_a", "modularSofa_b",
            "roungeSofa_a", "roungeSofa_b"
        };

        if (idx >= modelNames.Length)
        {
            return;
        }

        if (modelGo != null)
        {
            Destroy(modelGo);
        }

        modelGo = Instantiate(Resources.Load<GameObject>(modelNames[idx]));
        model = modelGo.GetComponent<SofaModel>();
        modelGo.name = modelNames[idx].Split('_')[0];
    }

    private void RegisterToggle(Toggle toggle, Action onSelect)
    {
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                onSelect?.Invoke();
            }
        });
    }
    
    private void SetColor(string colorName)
    {
        switch (colorName)
        {
            case "beige":
                SetColor("beige", beigeColor);
                break;
            case "gray":
                SetColor("gray", grayColor);
                break;
            case "black":
                SetColor("black", blackColor);
                break;
        }
    }

    private void SetColor(string colorName, Color color)
    {
        selectedColor = colorName;
        SetToggleState(colorName, "color");
        
        if (model == null)
        {
            return;
        }

        foreach (var mesh in model.meshs)
        {
            foreach (var material in mesh.materials)
            {
                material.SetColor(_Color, color);
            }
        }
    }

    private void SetMaterial(string materialName)
    {
        float value = materialName.Equals("leather") ? 1.0f : 0.0f;
        SetMaterial(materialName, value);
    }

    private void SetMaterial(string materialName, float value)
    {
        selectedMaterial = materialName;
        SetToggleState(materialName, "material");

        if (model == null)
        {
            return;
        }
        
        foreach (var mesh in model.meshs)
        {
            foreach (var material in mesh.materials)
            {
                material.SetFloat(_Smoothness, value);
            }
        }
    }

    private void SetSize(string sizeName)
    {
        float size = sizeName.Equals("large") ? 1.2f : 1.0f;
        SetSize(sizeName, size);
    }

    private void SetSize(string sizeName, float size)
    {
        selectedSize = sizeName;
        SetToggleState(sizeName, "size");

        if (modelGo != null)
        {
            modelGo.transform.localScale = Vector3.one * size;
        }
    }

    private void SwitchModel(string modelType)
    {
        selectedModelType = modelType;
        SetToggleState(modelType, "modelType");
        
        if (modelGo == null)
        {
            return;
        }
        
        string newModelName = modelGo.name;
        ReplaceModel(Resources.Load<GameObject>($"{newModelName}_{modelType}"), newModelName);

        SetColor(selectedColor);
        SetMaterial(selectedMaterial);
        SetSize(selectedSize);
    }

    private void ReplaceModel(GameObject prefab, string newModelName)
    {
        if (modelGo != null)
        {
            Destroy(modelGo);
        }

        modelGo = Instantiate(prefab);
        model = modelGo.GetComponent<SofaModel>();
        modelGo.name = newModelName;
    }
    
    private void SetToggleState(string name, string category)
    {
        switch (category)
        {
            case "color":
                beigeToggle.SetIsOnWithoutNotify(name.Equals("beige"));
                grayToggle.SetIsOnWithoutNotify(name.Equals("gray"));
                blackToggle.SetIsOnWithoutNotify(name.Equals("black"));
                break;
            case "material":
                fabricToggle.SetIsOnWithoutNotify(name.Equals("fabric"));
                leatherToggle.SetIsOnWithoutNotify(name.Equals("leather"));
                break;
            case "size":
                smallToggle.SetIsOnWithoutNotify(name.Equals("small"));
                largeToggle.SetIsOnWithoutNotify(name.Equals("large"));
                break;
            case "modelType":
                aToggle.SetIsOnWithoutNotify(name.Equals("a"));
                bToggle.SetIsOnWithoutNotify(name.Equals("b"));
                break;
        }
    }

    public void SaveData()
    {
        string name = productNameField.text;
        
        Debug.Log("[저장 완료]");
        Debug.Log($"제품명: {name}");
        Debug.Log($"색상: {selectedColor}");
        Debug.Log($"소재: {selectedMaterial}");
        Debug.Log($"사이즈: {selectedSize}");
        Debug.Log($"모델: {selectedModelType}");

        if (string.IsNullOrEmpty(name))
        {
            OpenToast("제품명을 입력해주세요.", 3.0f);
            return;
        }

        if (name.Length >= 10)
        {
            OpenToast("제품명은 10자 이하여야 합니다.", 3.0f);
            return;
        }
        
        data = new CustomData
        {
            name = name,
            color = selectedColor,
            material = selectedMaterial,
            size = selectedSize,
            model = selectedModelType
        };

        Debug.Log($"Save Success {JsonUtility.ToJson(data)}");
        StartCoroutine(Post(""));
    }

    private void OpenToast(string message, float time)
    {
        toast.SetActive(true);
        toastText.text = message;
        
        Invoke(nameof(CloseToast), time);
    }

    private void CloseToast()
    {
        toast.SetActive(false);
    }

    private IEnumerator Get(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
    }

    private IEnumerator Post(string url)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.error != null)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
    }
}
