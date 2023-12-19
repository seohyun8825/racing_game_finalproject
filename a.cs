
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;


public class a : MonoBehaviourPunCallbacks
{
    public GameObject[] carPrefabs; // 차량 프리팹 배열

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedInstantiate());
    }

    IEnumerator DelayedInstantiate()
    {
        yield return new WaitForSeconds(1.0f); // 지연 시간

        if (PhotonNetwork.IsConnected && carPrefabs.Length > 0)
        {
            int selectedCarIndex = PlayerPrefs.GetInt("PlayerCar", 0); // 차량 인덱스 불러오기, 기본값은 0
            if (selectedCarIndex >= 0 && selectedCarIndex < carPrefabs.Length)
            {
                PhotonNetwork.Instantiate(carPrefabs[selectedCarIndex].name, new Vector3(0, 0, 0), Quaternion.identity);
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}