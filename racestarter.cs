using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;


public class racestarter : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject[] carPrefabs;
    public Transform[] spawnPos;
    private float startTime;

    public GameObject[] countDownItems;
    CheckpointManage[] carsCPM;
    public static bool racing = false;
    public static int totalLaps = 1;
    public GameObject GameOverPanel;
    public GameObject HUD;
    int playerCar;
    public GameObject startRace;
    public GameObject waitingText;


    void Start()
    {
    // Debug.Log("Photon Network 연결 상태: " + PhotonNetwork.IsConnected);
    if (PhotonNetwork.IsConnected)
    {
        // Debug.Log("룸 내 플레이어 수: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }
        racing = false;
        foreach (GameObject g in countDownItems)
            g.SetActive(false);

        GameOverPanel.SetActive(false);
        startRace.SetActive(false);
        waitingText.SetActive(false);

        playerCar = PlayerPrefs.GetInt("PlayerCar");    
        int randomStartPos = Random.Range(0,spawnPos.Length);
        Vector3 startPos = spawnPos[randomStartPos].position;
        Quaternion startRot = spawnPos[randomStartPos].rotation;
        GameObject pcar = null;

        if(PhotonNetwork.IsConnected)
        {
            startPos = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber-1].position;
            startRot = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber-1].rotation;
            
            if(NetworkedPlayer.LocalPlayerInstance == null)
            {
                pcar = PhotonNetwork.Instantiate(carPrefabs[playerCar].name, startPos, startRot, 0);
                // PhotonView ID 로그 추가
                PhotonView pv = pcar.GetComponent<PhotonView>();
                if (pv != null)
                {
                    // Debug.Log("인스턴스화된 차량의 PhotonView ID: " + pv.ViewID);
                }
                else
                {
                    // Debug.LogError("인스턴스화된 차량에 PhotonView가 없습니다.");
                }
            }

            if (PhotonNetwork.IsMasterClient)
            {
                startRace.SetActive(true);
            }
            else{
                waitingText.SetActive(true);
            }
        }

        else
        {

            pcar = Instantiate(carPrefabs[playerCar]);
            pcar.transform.position = startPos;
            pcar.transform.rotation = startRot; 
            foreach(Transform t in spawnPos)
            {
                if(t==spawnPos[randomStartPos]) continue;
                GameObject car = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)]);
                car.transform.position = t.position;
                car.transform.rotation = t.rotation;
            }
            StartGame();
        }

        SmoothFollow.playerCar = pcar.gameObject.GetComponent<Drive>().rb.transform;
        pcar.GetComponent<Aicontroll>().enabled = false;
        pcar.GetComponent<Drive>().enabled = true;
        pcar.GetComponent<PlayerController>().enabled =true;






    }

    public void BeginGame()
    
    {
        string[] aiNames = {"Lee", "Park", "Kim", "Son"};
        /*int numAIPlayers = PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;*/
        for(int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            Vector3 startPos = spawnPos[i].position;
            Quaternion startRot = spawnPos[i].rotation;
            int r = Random.Range(0, carPrefabs.Length);

            object[]instanceData = new object[1];
            instanceData[0] = (string)aiNames[Random.Range(0,aiNames.Length)];

            GameObject AIcar = PhotonNetwork.Instantiate(carPrefabs[r].name, startPos, startRot, 0, instanceData);
            AIcar.GetComponent<Aicontroll>().enabled = true;
            AIcar.GetComponent<Drive>().enabled = true;
            AIcar.GetComponent<Drive>().networkName = (string)instanceData[0];
            AIcar.GetComponent<PlayerController>().enabled = false;

        }
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All, null);

        }

    }
    [PunRPC]
    public void StartGame()
    {
        StartCoroutine(PlayCountDown());
        startRace.SetActive(false);
        waitingText.SetActive(false);
        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
        carsCPM = new CheckpointManage[cars.Length];
        for(int i =0; i< cars.Length; i++)
            carsCPM[i] = cars[i].GetComponent<CheckpointManage>();
    }
    IEnumerator PlayCountDown()
    {
        yield return new WaitForSeconds(2);
        foreach (GameObject g in countDownItems)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(1);
            g.SetActive(false);
;
        }
        racing = true;
        startTime = Time.time;
    }
    [PunRPC]
    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("final_project");
    }
    public void LoadToMap()
    {
        SceneManager.LoadScene("map");
    }
    public void RestartLevel()
    {
        racing = false;
        if(PhotonNetwork.IsConnected)
            photonView.RPC("RestartGame", RpcTarget.All, null);
        else
            SceneManager.LoadScene("final_project");

    }


    bool raceOver = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            raceOver = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!racing) return;
        foreach (CheckpointManage cpm in carsCPM)
        {
           // Debug.Log("차량: " + cpm.name + ", 랩: " + cpm.lap);
        }

        int finishedCount = 0;
        foreach (CheckpointManage cpm in carsCPM)
        {
            if (cpm.lap == totalLaps+1)
                finishedCount++;
        }
        if (finishedCount == carsCPM.Length || raceOver)
        {
            HUD.SetActive(false);
            GameOverPanel.SetActive(true);
        }
    }
}
