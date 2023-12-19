using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // Start is called before the first frame update

    public Circuit circuit;
    Drive ds;
    public float steeringSensitivity = 0.01f;
    Vector3 target;
    int currentWp =0;
    private QLearningAgent qLearningAgent;

    void Start()
    {
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWp].transform.position;
        qLearningAgent = GetComponent<QLearningAgent>();
        if (qLearningAgent == null)
        {
            Debug.LogError("QLearningAgent component is not attached to the GameObject.");
        }
        if (ds == null)
        {
            Debug.LogError("Drive component is not attached to the GameObject.");
        }
        if (circuit == null)
        {
            Debug.LogError("Circuit is not assigned.");
        }

    }


    void Update()
    {
        Vector3 localTarget = ds.rb.gameObject.transform.InverseTransformPoint(target);
        float distanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);
        float accel = 0f;
        float brake = 0;

        int action = qLearningAgent.ChooseAction(); // Q-Learning 에이전트로부터 행동 결정
        if (action == 0)
            accel = 0.5f; // 가속
        else
            brake = 1f; // 정지

        ds.Go(accel, steer, brake);

        if (distanceToTarget < 5)
        {
            currentWp++;
            if (currentWp >= circuit.waypoints.Length)
                currentWp = 0;
            target = circuit.waypoints[currentWp].transform.position;
        }

        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }

    // Update is called once per frame

}
