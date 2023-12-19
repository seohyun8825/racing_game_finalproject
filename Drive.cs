using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour

{

    public WheelCollider[] WCs;
    public string driverName = "";
    public float torque = 200;
    public float maxSteerAngle = 30;
    public float maxBrakeTorque = 500;

    public GameObject[] Wheels;

    public AudioSource skidSound;
    public AudioSource highAccel;

    public Transform SkidTrailPrefab;
    public GameObject brakeLight;
    public Rigidbody rb;
    public float gearLength = 3;
    public float currentSpeed {
        get{return rb.velocity.magnitude *gearLength;}
    }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public float maxSpeed = 200;
    string[] aiNames = {"Lee", "Park", "Kim", "Son"};
    public GameObject playerNamePrefab;

    public Renderer JeepMesh;
    public string networkName = "";
    public int numGears = 5;
    float rpm;
    int currentGear =1;
    float currentGearPerc;
    //public string driverName ="";


    Transform[] skidTrails = new Transform[4];

    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public void StartSkidTrail(int i)
    {
        if(skidTrails[i] == null)
            skidTrails[i] = Instantiate(SkidTrailPrefab);

        skidTrails[i].parent = WCs[i].transform;
        skidTrails[i].localPosition = -Vector3.up *WCs[i].radius;
        skidTrails[i].localRotation = Quaternion.Euler(90,0,0);

    }

    public void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null) return;
        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.rotation = Quaternion.Euler(90,0,0);
        Destroy(holder.gameObject,30);
    }


        // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i<4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }

        brakeLight.SetActive(false);
        GameObject playerName = Instantiate(playerNamePrefab);
        playerName.GetComponent<NameUIController>().target = rb.gameObject.transform;
        Aicontroll aiController = this.GetComponent<Aicontroll>();
        NameUIController nameUIController = playerName.GetComponent<NameUIController>();

        // AI 컨트롤러가 활성화되어 있으면 AI 이름을, 그렇지 않으면 PlayerPrefs에서 가져온 이름을 사용합니다.
        if(aiController && aiController.enabled)
            nameUIController.driverName = aiNames[Random.Range(0, aiNames.Length)];
        else
            nameUIController.driverName = PlayerPrefs.GetString("PlayerName");
        
        playerName.GetComponent<NameUIController>().carRend = JeepMesh;



        
    }

    public void CalculateEngineSound()
    {
        float gearPercentage = (1/(float)numGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage*currentGear, gearPercentage*(currentGear+1), Mathf.Abs(currentSpeed/maxSpeed));
     
        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime *5f);
        var gearNumFactor = currentGear/ (float) numGears;
        rpm = Mathf.Lerp(gearNumFactor,1,currentGearPerc);
        float speedPercentage = Mathf.Abs(currentSpeed/ maxSpeed);
        float upperGearMax = (1/(float)numGears) * (currentGear+1);
        float downGearMax = (1/(float)numGears)* currentGear;

        if(currentGear>0 && speedPercentage<downGearMax)
            currentGear--;
        if(speedPercentage > upperGearMax &&(currentGear<(numGears -1)))
            currentGear++;
        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.225f;
    }
    public void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1)*maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1)*maxBrakeTorque;

        if (brake!= 0)
            brakeLight.SetActive(true);
        else
            brakeLight.SetActive(false);


        float thrustTorque = 0;
        if(currentSpeed<maxSpeed)
            thrustTorque = accel*torque;
        for(int i =0; i<4; i++){
            WCs[i].motorTorque = thrustTorque;
            WCs[i].brakeTorque = brake;

            if(i<2)

                WCs[i].steerAngle = steer;

            Quaternion quat;
            Vector3 position;
            WCs[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.localRotation = quat;
        }


    }


    public void CheckForSkid()
    {
        int numSkidding = 0;
        for(int i = 0; i<4; i++)
        {
            WheelHit wheelHit;
            WCs[i].GetGroundHit(out wheelHit);
            if(Mathf.Abs(wheelHit.forwardSlip)>=0.4f || Mathf.Abs(wheelHit.sidewaysSlip)>=0.4f)
            {
                numSkidding++;
                if(!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                //StartSkidTrail(i);
                skidSmoke[i].transform.position = WCs[i].transform.position - WCs[i].transform.up *WCs[i].radius;
                skidSmoke[i].Emit(1);


            }
            else{
                //EndSkidTrail(i);
            }
        }
        if(numSkidding==0 && skidSound.isPlaying)
        {
            skidSound.Stop();
        }
    }

    // Update is called once per frame

}
