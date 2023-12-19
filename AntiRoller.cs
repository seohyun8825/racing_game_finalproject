using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRoller : MonoBehaviour
{

    public float antiRoll = 5000.0f;
    public WheelCollider wheelLfront;
    public WheelCollider wheelRfront;
    public WheelCollider wheelLback;
    public WheelCollider wheelRback;
    public GameObject COM;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass =COM.transform.localPosition;

    }
    void DifferentWheel(WheelCollider WL, WheelCollider WR) {

        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = WL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WL.transform.InverseTransformPoint(hit.point).y - WL.radius) / WL.suspensionDistance;
        
        bool groundedR = WR.GetGroundHit(out hit); // 여기를 수정하였습니다.
        if (groundedR)
            travelR = (-WR.transform.InverseTransformPoint(hit.point).y - WR.radius) / WR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;
        
        if (groundedL)
            rb.AddForceAtPosition(WL.transform.up * -antiRollForce, WL.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(WR.transform.up * antiRollForce, WR.transform.position); // 여기를 수정하였습니다.
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        DifferentWheel(wheelLfront, wheelRfront);
        DifferentWheel(wheelLback, wheelRback);



    }
}
