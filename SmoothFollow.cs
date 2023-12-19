using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothFollow : MonoBehaviour
{
    Transform[] target;
    public static Transform playerCar;
    public float distance = 8.0f;
    public float height = 1.5f;
    public float heightOffset = 1.0f;
    public float heightDamping = 4.0f;
    public float rotationDamping = 2.0f;
    public RawImage rearCamView;
    int index = 0;

    int FP = -1;

    void Start()
    {
        if (PlayerPrefs.HasKey("FP"))
        {
            FP = PlayerPrefs.GetInt("FP");
        }
    }
    void LateUpdate()
    {
        // target 배열 초기화 및 검사
        if (target == null)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
            if (cars.Length == 0)
            {
                Debug.LogError("차량을 찾을 수 없습니다.");
                return;
            }

            target = new Transform[cars.Length];
            for (int i = 0; i < cars.Length; i++)
            {
                target[i] = cars[i].transform;
                if(target[i]==playerCar) index = i;
            }
        }

        // index 범위 검사
        if (index < 0 || index >= target.Length)
        {
            Debug.LogError("Index가 배열 범위를 벗어났습니다: " + index);
            return;
        }

        // 'RearCamera' 찾기 및 카메라 컴포넌트 검사
        var rearCamera = target[index].Find("RearCamera");
        if (rearCamera == null)
        {
            Debug.LogError("RearCamera를 찾을 수 없습니다. 현재 차량: " + target[index].name);
            return;
        }

        var cameraComponent = rearCamera.gameObject.GetComponent<Camera>();
        if (cameraComponent == null)
        {
            Debug.LogError("Camera 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 카메라 타겟 텍스처 설정
        cameraComponent.targetTexture = (rearCamView.texture as RenderTexture);

        // 나머지 카메라 추적 로직
        if (FP == 1)
        {
            transform.position = target[index].position + target[index].forward * 0.4f + target[index].up;
            transform.LookAt(target[index].position + target[index].forward * 3f);
        }
        else
        {
            float wantedRotationAngle = target[index].eulerAngles.y;
            float wantedHeight = target[index].position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            transform.position = target[index].position;
            transform.position -= currentRotation * Vector3.forward * distance;

            transform.position = new Vector3(transform.position.x,
                                        currentHeight + heightOffset,
                                        transform.position.z);

            transform.LookAt(target[index]);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            FP *= -1;
            PlayerPrefs.SetInt("FP", FP);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            target[index].Find("RearCamera").gameObject.GetComponent<Camera>().targetTexture = null;
            index++;
            if (index > target.Length - 1) index = 0;
            target[index].Find("RearCamera").gameObject.GetComponent<Camera>().targetTexture = 
                                                              (rearCamView.texture as RenderTexture);
        }
    }
}
