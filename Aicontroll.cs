using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aicontroll : MonoBehaviour
{
    // Start is called before the first frame update

    public Circuit circuit;
    Drive ds;
    public float steeringSensitivity = 0.01f;
    public float brakingSensitivity = 3f;
    public float accelSensitivity = 0.3f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWp =0;

    GameObject tracker;
    public int currentTrackerWp = 0;
    public float lookAhead = 10;

    float totalDistanceToTarget;
    float lastTimeToMove = 0;

    CheckpointManage cpm;
    float finishSteer;


    void Start()
    {
        if(circuit == null)
            circuit = GameObject.FindGameObjectWithTag("circuit").GetComponent<Circuit>();
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWp].transform.position;
        nextTarget = circuit.waypoints[currentWp+1].transform.position;
        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = ds.rb.gameObject.transform.position;
        tracker.transform.rotation = ds.rb.gameObject.transform.rotation;
        totalDistanceToTarget = Vector3.Distance(target,ds.rb.gameObject.transform.position);
        this.GetComponent<Ghost>().enabled = false;
        finishSteer = Random.Range(-1.0f, 1.0f);

    }



    void ProgressTracker()
    {
        
        Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position);
        if(Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position)>lookAhead)return;
        tracker.transform.LookAt(circuit.waypoints[currentTrackerWp].transform.position);
        tracker.transform.Translate(0,0,1.0f);
        if(Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWp].transform.position)<1){
            currentTrackerWp++;
            if(currentTrackerWp >= circuit.waypoints.Length)
                currentTrackerWp = 0;

        }
    }

    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(!racestarter.racing){
            lastTimeToMove = Time.time;
            return;
        }

        if(cpm == null)
            cpm = ds.rb.GetComponent<CheckpointManage>();
        if(cpm.lap == racestarter.totalLaps+1)
        {
            ds.highAccel.Stop();
            ds.Go(0, finishSteer, 0);
            return;
        }
        
        ProgressTracker();
        Vector3 localTarget;
        float targetAngle;
        if(ds.rb.velocity.magnitude > 1)
            lastTimeToMove = Time.time;
        if(Time.time>lastTimeToMove+3.5)
        {
            ds.rb.gameObject.transform.position = circuit.waypoints[currentTrackerWp]. transform.position+Vector3.up*2 + new Vector3(Random.Range(-1,1), 0, Random.Range(-1,1));
            tracker.transform.position = ds.rb.gameObject.transform.position;
            ds.rb.gameObject.layer = 6;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }
        if(Time.time<ds.rb.GetComponent<AvoidDetector>().avoidTime)
        {
            localTarget = tracker.transform.right * ds.rb.GetComponent<AvoidDetector>().avoidPath;

        }
        else{
            localTarget = ds.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);
        }
        
        targetAngle= Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle*steeringSensitivity, -1,1) *Mathf.Sign(ds.currentSpeed);
        float speedFactor = ds.currentSpeed / ds.maxSpeed;
        float corner = Mathf.Clamp(Mathf.Abs(targetAngle),0,90);
        float cornerFactor = corner / 90.0f;

        float brake = 0;
        if(corner>10 && speedFactor >0.1f)
            brake = Mathf.Lerp(0,1*speedFactor*brakingSensitivity, cornerFactor);

        float accel = 1f;
        if(corner>20&& speedFactor >0.2f)
            accel = Mathf.Lerp(0,1*accelSensitivity,1-cornerFactor);

        ds.Go(accel, steer, brake);


        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }



}