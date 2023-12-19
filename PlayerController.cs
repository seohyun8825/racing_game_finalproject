using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Drive ds;

    float lastTimeToMove = 0;
    Vector3 lastPosition;
    Quaternion lastRotation;

    CheckpointManage cpm;
    float finishSteer;
    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
        //ds = this.GetComponent<Drive>();
        //this.GetComponent<Ghost>().enabled = false;
        lastPosition = ds.rb.gameObject.transform.position;
        lastRotation = ds.rb.gameObject.transform.rotation;
        finishSteer = Random.Range(-1.0f, 1.0f);
    }

    // Start is called before the first frame update
    void Start()
    {

        ds = this.GetComponent <Drive> ();
        this.GetComponent<Ghost>().enabled = false;
        lastTimeToMove = Time.time;
        cpm = GetComponent<CheckpointManage>();
        //lastPosition = ds.rb.gameObject.transform.position;
        //lastRotation = ds.rb.gameObject.transform.rotation;
    }

    // Update is called once per frame


    GameObject FindClosestRoad()
    {
        GameObject[] roads = GameObject.FindGameObjectsWithTag("Road");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject road in roads)
        {
            Vector3 diff = road.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = road;
                distance = curDistance;
            }
        }

        return closest;
    }
    void Update()
    {
        if(cpm == null)
            cpm = ds.rb.GetComponent<CheckpointManage>();

        if(cpm.lap == racestarter.totalLaps +1)
        {
            ds.highAccel.Stop();
            ds.Go(0,finishSteer,0);
            return;
        }
        if(!racestarter.racing){
            lastTimeToMove = Time.time;
            return;
        }
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        if(ds.rb.velocity.magnitude>1 ||!racestarter.racing)
            lastTimeToMove = Time.time;
        RaycastHit hit;
        if(Physics.Raycast(ds.rb.gameObject.transform.position, -Vector3.up, out hit, 10))
        {
            if(hit.collider.gameObject.tag == "Road")
            {
                lastPosition = ds.rb.gameObject.transform.position;
                lastRotation = ds.rb.gameObject.transform.rotation;

            }
                //Debug.Log("off road");
        }
        else
        {
            // 도로 위에 없을 때, 가장 가까운 도로의 위치와 방향을 찾아 업데이트
            GameObject closestRoad = FindClosestRoad();
            if (closestRoad != null)
            {
                lastPosition = closestRoad.transform.position;
                lastRotation = closestRoad.transform.rotation;
            }
        }


        if(Time.time > lastTimeToMove+3.5 && racestarter.racing)
        {

            
            if(cpm.lastCP != null)  // 마지막 체크포인트 정보가 있는지 확인
            {
                Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                ds.rb.gameObject.transform.position = cpm.lastCP.transform.position + randomOffset;
                ds.rb.gameObject.transform.rotation = cpm.lastCP.transform.rotation;
            }
            else  // 체크포인트 정보가 없으면 이전 로직대로 동작
            {
                ds.rb.gameObject.transform.position = lastPosition;
                ds.rb.gameObject.transform.rotation = lastRotation;
            }
            
            ds.rb.gameObject.layer = 6;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }
        if(!racestarter.racing){
            //lastTimeToMove = Time.time;
            a =0;
        }
        ds.Go(a,s,b);
        ds.CheckForSkid();
        ds.CalculateEngineSound();

    }
}
