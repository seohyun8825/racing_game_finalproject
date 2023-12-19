using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManage : MonoBehaviour
{

    public int lap = 0;
    public int checkpoint = -1;
    public float timeEntered = 0;

    int checkPointCount;
    int nextCheckPoint;
    public GameObject lastCP;
    // ?
    //public GameObject raceFinishCanvas;


    // Start is called before the first frame update
    void Start()
    {
        // ?
        // raceFinishCanvas.SetActive(false);
        GameObject[] cps = GameObject.FindGameObjectsWithTag("checkpoint");
        checkPointCount = GameObject.FindGameObjectsWithTag("checkpoint").Length;
        nextCheckPoint = 0;
        foreach(GameObject c in cps)
        {
            if(c.name == "0")
            {
                lastCP = c;
                break;
            }

        }
    }
    public float raceFinishTime = 0f;
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "checkpoint")
        {
            int thisCPNumber = int.Parse(col.gameObject.name);

            if (thisCPNumber == nextCheckPoint)
            {
                checkpoint = thisCPNumber;
                lastCP = col.gameObject;
                timeEntered = Time.time;

                // 랩 카운트 증가 및 레이스 완료 시간 기록
                if (nextCheckPoint == 0)
                {
                    lap++;
                    if (nextCheckPoint == 149) // if (lap == racestarter.totalLaps && gameObject.CompareTag("Player"))
                    {
                        // 플레이어 차량이 마지막 랩을 완료하면 시간 기록
                        float raceFinishTime = Time.time;
                        Debug.Log("Player Race Finish Time: " + raceFinishTime);
                        // I want to make game object canvas visible in here ! ?
                        //GameObject raceFinishCanvas = GameObject.FindWithTag("canvas");
                        //raceFinishCanvas.SetActive(true);
                       
                    }
                }

                nextCheckPoint++;
                if (nextCheckPoint >= checkPointCount)
                    nextCheckPoint = 0;
            }
            else if (thisCPNumber < nextCheckPoint)
            {
                // 플레이어가 체크포인트를 건너뛴 경우
                // 플레이어를 마지막 체크포인트 위치로 리셋
                transform.position = lastCP.transform.position;
            }
        }
    }

}
    // Update is called once per frame


