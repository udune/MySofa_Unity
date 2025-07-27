using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// 저장할 커스텀 데이터
[Serializable]
public class CustomData
{
    // 소파 종류
    public string name;
    // 사용자가 입력한 제품명
    public string customName;
    // 색상
    public string color;
    // 소재
    public string material;
    // 사이즈
    public string size;
    // 타입
    public string model;
}

// 서버에서 받아온 세션 데이터
[Serializable]
public class SessionData
{
    // 세션 고유ID
    public string id;
    // 소파 이름
    public string name;
    // 사용자가 지은 제품 이름
    public string custom_name;
    // 색상
    public string color;
    // 소재
    public string material;
    // 사이즈
    public string size;
    // 타입
    public string model_type;
    // 생성 시간
    public string created_at;
    // 마지막 수정 시간
    public string updated_at;
    // 사용자 정보
    public UserData user;
    // 제품 정보
    public ProductData product;
}

// 사용자 정보
[Serializable]
public class UserData
{
    // 사용자 ID
    public string id;
}

// 상품 정보
[Serializable]
public class ProductData
{
    // 상품 ID
    public string id;
}

public class CustomizerUI : MonoBehaviour
{
    // 프리팹 모델의 색상과 재질을 바꾸기 위한 셰이더 속성
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

    [Header("버튼")] 
    public Button saveButton;
    public Button backButton;

    // 현재 선택된 색상
    private string selectedColor = "";
    // 현재 선택된 소재
    private string selectedMaterial = "";
    // 현재 선택된 사이즈
    private string selectedSize = "";
    // 현재 선택된 모델 타입
    private string selectedModelType = "";
    // URL 쿼리에서 추출한 세션 ID
    private string sessionId = "";

    // 서버에서 받아온 세션 데이터
    private SessionData currentSessionData;
    // 현재 화면에 나온 프리팹 모델
    private GameObject modelGo;
    // 프리팹 모델의 컴포넌트 스크립트
    public SofaModel model;
    
    private CustomData data = new CustomData();
    
    // 미리 정해둔 색상 RGB
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
        // sessionId를 찾아서 가져온다.
        GetSessionIdFromURL();

        // 토글들의 인터랙션 함수들을 연결한다.
        RegisterToggles();
        
        // 저장/돌아가기 버튼 클릭 시 실행될 함수들을 연결한다.
        RegisterButtons();

        // sessionId가 있으면 서버에서 데이터를 가져온다.
        if (!string.IsNullOrEmpty(sessionId))
        {
            StartCoroutine(LoadSessionData());
        }
        else
        {
            OpenToast("세션 ID가 없습니다.", 3.0f);
        }
    }

    private void GetSessionIdFromURL()
    {
        // URL 주소를 가져온다.
        string url = Application.absoluteURL;
        
        // sessionId 쿼리가 있는지 확인한다.
        if (url.Contains("sessionId="))
        {
            // ? 기준으로 나눈다.
            string[] parts = url.Split('?');
            if (parts.Length > 1)
            {
                // = 기준으로 나누어서 키와 값을 분리한다.
                string[] values = parts[1].Split("=");
                if (values[0] == "sessionId" && values.Length == 2)
                {
                    sessionId = values[1];
                    Debug.Log($"sessionId: {sessionId}");
                }
            }
        }
    }

    // 토글 실행 함수 연결
    private void RegisterToggles()
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

    // 저장 버튼과 돌아가기 버튼을 클릭했을 때 실행될 함수들을 연결하는 함수
    private void RegisterButtons()
    {
        // 저장 버튼 있을 경우에 SaveData 함수 연결
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveData);
        }

        // 돌아가기 버튼 있을 경우에 GoBack 함수 연결
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }

    private IEnumerator LoadSessionData()
    {
        string url = $"https://api.my-sofa.org/custom-session/{sessionId}";
        
        // 서버에 데이터 요청
        UnityWebRequest request = UnityWebRequest.Get(url);
        // 응답 기다림
        yield return request.SendWebRequest();
        // 성공했는지 확인
        if (request.result == UnityWebRequest.Result.Success)
        {
            // Json 데이터를 파싱한다.
            string response = request.downloadHandler.text;
            currentSessionData = JsonUtility.FromJson<SessionData>(response);
            
            // 받은 데이터로 UI를 갱신한다.
            UpdateUI();
            
            Debug.Log("세션 데이터 불러오기 성공");
        }
        else
        {
            Debug.Log($"세션 데이터 로드 실패: {request.error}");
            OpenToast("데이터를 불러오는데 실패했습니다.", 3.0f);
        }
    }

    // 서버에서 받은 데이터로 버튼들과 UI를 업데이트한다.
    private void UpdateUI()
    {
        // 데이터가 없으면 return
        if (currentSessionData == null)
        {
            return;
        }

        // 제품명 업데이트
        productNameField.text = currentSessionData.custom_name;

        // 모델 불러오고 색상, 소재, 사이즈 설정
        LoadModel(currentSessionData.name, currentSessionData.model_type);
        SetColor(currentSessionData.color);
        SetMaterial(currentSessionData.material);
        SetSize(currentSessionData.size);
    }

    private void LoadModel(string name, string model_type)
    {
        // 모델 파일 이름 만들기
        string modelName = $"{name}_{model_type}";

        // 기존 모델이 있으면 삭제한다.
        if (modelGo != null)
        {
            Destroy(modelGo);
        }
        
        // Resources에서 불러온다.
        GameObject prefab = Resources.Load<GameObject>(modelName);
        if (prefab != null)
        {
            // 프리팹을 생성한다.
            modelGo = Instantiate(prefab);
            model = modelGo.GetComponent<SofaModel>();
            modelGo.name = name;
            
            // 현재 선택된 모델 타입 저장
            selectedModelType = model_type;
            // 해당 모델 타입 토글 활성화
            SetToggleState(model_type, "modelType");
        }
        else
        {
            OpenToast("모델을 찾을 수 없습니다.", 3.0f);
            Debug.LogError($"모델을 찾을 수 없습니다: {modelName}");
        }
    }

    // 토글 트리거시 실행될 함수 연결
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
    
    // 색상 적용
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

    // 실제 모델에 적용
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

    // 소재 적용
    private void SetMaterial(string materialName)
    {
        float value = materialName.Equals("leather") ? 1.0f : 0.0f;
        SetMaterial(materialName, value);
    }

    // 실제 모델에 적용
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

    // 사이즈 적용
    private void SetSize(string sizeName)
    {
        float size = sizeName.Equals("large") ? 1.2f : 1.0f;
        SetSize(sizeName, size);
    }

    // 실제 모델에 적용
    private void SetSize(string sizeName, float size)
    {
        selectedSize = sizeName;
        SetToggleState(sizeName, "size");

        if (modelGo != null)
        {
            modelGo.transform.localScale = Vector3.one * size;
        }
    }

    // 모델 타입을 바꾼다.
    private void SwitchModel(string modelType)
    {
        // 현재 선택된 모델 타입으로 저장
        selectedModelType = modelType;
        // 해당 모델 타입 토글 선택 활성화
        SetToggleState(modelType, "modelType");
        
        // 현재 보여지는 모델 없을 경우 return
        if (modelGo == null)
        {
            return;
        }
        
        // 모델 교체
        string newModelName = modelGo.name;
        ReplaceModel(Resources.Load<GameObject>($"{newModelName}_{modelType}"), newModelName);

        // 교체한 모델에 색상, 소재, 사이즈 적용
        SetColor(selectedColor);
        SetMaterial(selectedMaterial);
        SetSize(selectedSize);
    }

    // 모델 교체
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
    
    // 토글들의 상태를 설정
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

    // 저장 버튼
    public void SaveData()
    {
        string selectedCustomName = productNameField.text;
        
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
        
        // 현재 모델의 이름
        string selectedName = modelGo != null ? modelGo.name : "";
        
        var saveData = new 
        {
            user_id = currentSessionData?.user?.id ?? "",
            product_id = currentSessionData?.product?.id ?? "",
            name = selectedName,
            custom_name = selectedCustomName,
            color = selectedColor,
            material = selectedMaterial,
            size = selectedSize,
            model_type = selectedModelType
        };

        StartCoroutine(PostSaveData(saveData));
    }

    private IEnumerator PostSaveData(object saveData)
    {
        string url = "https://api.my-sofa.org/myitems";
        
        // 데이터를 JSON 형태로 변환
        string jsonData = JsonUtility.ToJson(saveData);
        
        Debug.Log($"요청 데이터: {jsonData}");
        Debug.Log($"요청 URL: {url}");
        
        // POST 방식으로 서버에 데이터 보내기 준비 (올바른 방법)
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        
        // JSON 데이터를 바이트 배열로 변환해서 업로드 핸들러에 설정
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // 헤더 설정
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        
        // 서버 응답을 기다리기
        yield return request.SendWebRequest();
        
        // 결과 확인
        Debug.Log($"응답 코드: {request.responseCode}");
        Debug.Log($"응답 내용: {request.downloadHandler.text}");
        
        // 서버에 성공적으로 저장되었는지 확인
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("저장 완료");
            // 성공 메시지 보여주기
            OpenToast("저장되었습니다.", 3.0f);
        }
        else
        {
            Debug.LogError($"저장 실패 - 코드: {request.responseCode}, 에러: {request.error}");
            
            // 구체적인 에러 메시지 표시
            if (request.responseCode == 400)
            {
                OpenToast("잘못된 데이터입니다. 모든 항목을 선택해주세요.", 3.0f);
            }
            else if (request.responseCode == 500)
            {
                OpenToast("서버 오류입니다. 잠시 후 다시 시도해주세요.", 3.0f);
            }
            else
            {
                // 기본 실패 메시지
                OpenToast("저장에 실패했습니다.", 3.0f);
            }
        }
        
        // 메모리 정리
        request.Dispose();
    }

    // 돌아가기 버튼
    public void GoBack()
    {
#if !UNITY_EDITOR
        Application.ExternalEval("window.location.href = 'https://my-sofa.org';");
#else
        Application.OpenURL("https://my-sofa.org");
#endif
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
    
    // 매 프레임마다 실행되는 함수 (주로 키보드 입력 확인용)
    private void Update()
    {
#if UNITY_EDITOR
        // 에디터에서만 실행되는 테스트 코드
        // 숫자 키 0~7을 누르면 다른 모델들 테스트해볼 수 있음
        for (int i = 0; i <= 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                LoadTest(i);  // 해당 번호의 테스트 모델 불러오기
            }
        }
#endif
    }

    // 테스트용 모델을 불러오는 함수 (에디터에서만 사용)
    private void LoadTest(int idx)
    {
        // 테스트할 수 있는 모델들의 이름 목록
        string[] modelNames =
        {
            "privateSofa_a", "privateSofa_b",    // 0, 1번 키
            "classicSofa_a", "classicSofa_b",    // 2, 3번 키
            "modularSofa_a", "modularSofa_b",    // 4, 5번 키
            "roungeSofa_a", "roungeSofa_b"       // 6, 7번 키
        };

        // 번호가 범위를 벗어나면 아무것도 하지 않기
        if (idx >= modelNames.Length)
        {
            return;
        }

        // 기존 모델이 있으면 삭제하기
        if (modelGo != null)
        {
            Destroy(modelGo);
        }

        // 새로운 테스트 모델 불러와서 화면에 보여주기
        modelGo = Instantiate(Resources.Load<GameObject>(modelNames[idx]));
        model = modelGo.GetComponent<SofaModel>();
        modelGo.name = modelNames[idx].Split('_')[0];  // "_" 앞부분만 이름으로 사용
    }
}
