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
    public string color;
    public string material;
    public string size;
    public string model;
}

public class CustomizerUI : MonoBehaviour
{
    private static readonly int _Color = Shader.PropertyToID("_BaseColor");

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
    private string selectedModel = "";

    private CustomData data = new CustomData();

    private GameObject modelGo;
    public SofaModel model;

    private void Start()
    {
        beigeToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedColor = "beige";
                SetColor(new Color(0.96f, 0.86f, 0.71f));
            }
        });
        
        grayToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedColor = "gray";
                SetColor(Color.gray);
            }
        });
        
        blackToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedColor = "black";
                SetColor(Color.black);
            }
        });
        
        fabricToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectedMaterial = "fabric";
        });
        
        leatherToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectedMaterial = "leather";
        });
        
        smallToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedSize = "small";
                modelGo.transform.localScale = Vector3.one;
            }
        });
        
        largeToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedSize = "large";
                modelGo.transform.localScale = Vector3.one * 1.2f;
            }
        });
        
        aToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectedModel = "a";
            }
        });
        
        bToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectedModel = "b";
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("privateSofa_a"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("privateSofa_b"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("classicSofa_a"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("classicSofa_b"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("modularSofa_a"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("modularSofa_b"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("roungeSofa_a"));
            model = modelGo.GetComponent<SofaModel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (modelGo != null)
            {
                Destroy(modelGo);
            }
            
            modelGo = Instantiate(Resources.Load<GameObject>("roungeSofa_b"));
            model = modelGo.GetComponent<SofaModel>();
        }
    }

    private void SetColor(Color color)
    {
        foreach (var mesh in model.meshs)
        {
            foreach (var material in mesh.materials)
            {
                material.SetColor(_Color, color);
            }
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
        Debug.Log($"모델: {selectedModel}");

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
            model = selectedModel
        };

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
