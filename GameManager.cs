
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

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
        // 지연 시간 (예: 1초)
        yield return new WaitForSeconds(1.0f);

        // 플레이어 인스턴스화
        if (PhotonNetwork.IsConnected && playerPrefab != null)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        // 이벤트 리스너 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
