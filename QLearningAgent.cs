using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.IO;

public class QLearningAgent : MonoBehaviour
{
    public Circuit circuit; // 참조를 위한 Circuit 객체
    public float learningRate = 0.5f;
    public float discountFactor = 0.7f;
    public float explorationRate = 0.8f;
    public float explorationDecay = 0.995f;
    private Drive drive;
    private int currentState;
    private int nextWaypointIndex;
    private float[,] qTable;
    
    private string qTableFilePath = "qTable.txt";
    
    void Start()
    {

    }

    void Update()
    {
        int action = ChooseAction();
        float reward = GetReward(action);

        // 선택된 액션에 따라 차량에 명령을 내립니다.
        if (action == 0) // 가속
        {
            drive.Go(1f, 0f, 0f); // 가속, 조향 0, 브레이크 0
        }
        else // 브레이크
        {
            drive.Go(0f, 0f, 1f); // 가속 0, 조향 0, 브레이크 적용
        }

        int nextState = nextWaypointIndex % circuit.waypoints.Length;
        float bestFutureQ = Mathf.Max(qTable[nextState, 0], qTable[nextState, 1]);
        
        qTable[currentState, action] = (1 - learningRate) * qTable[currentState, action] +
                                        learningRate * (reward + discountFactor * bestFutureQ);
        
        currentState = nextState;
        nextWaypointIndex++;
        explorationRate *= explorationDecay;
        
        if (Time.frameCount % 1000 == 0)
        {
            SaveQTable();
        }
    }

        
    void Awake()
    {
        drive = GetComponent<Drive>();
        currentState = 0;
        nextWaypointIndex = 1;
        int numStates = circuit.waypoints.Length;
        int numActions = 2; // 가속과 정지
        qTable = new float[numStates, numActions];
        
        if (File.Exists(qTableFilePath))
        {
            LoadQTable();
        }
        if (drive == null)
        {
            Debug.LogError("Drive component is not attached to the GameObject.");
        }
        if (circuit == null)
        {
            Debug.LogError("Circuit is not assigned.");
        }   
    }

    public int ChooseAction()
    {
        if (qTable == null)
        {
            Debug.LogError("qTable is not initialized!");
            return 0; // or handle appropriately
        }

        if (currentState < 0 || currentState >= qTable.GetLength(0))
        {
            Debug.LogError($"Invalid currentState: {currentState}");
            return 0; // or handle appropriately
        }

        if (Random.value < explorationRate)
        {
            return Random.Range(0, 2);
        }
        else
        {
            return qTable[currentState, 0] > qTable[currentState, 1] ? 0 : 1;
        }
    }





    float GetReward(int action)
    {
        // 드리프팅 보상

        //int action = ChooseAction();
        float angleToNextWaypoint = Vector3.Angle(transform.forward, 
            circuit.waypoints[nextWaypointIndex % circuit.waypoints.Length].transform.position - transform.position);
        float driftReward = Mathf.Cos(angleToNextWaypoint * Mathf.Deg2Rad);
        
        // 거리 보상
        float distanceToNextWaypoint = Vector3.Distance(transform.position,
            circuit.waypoints[nextWaypointIndex % circuit.waypoints.Length].transform.position);
        float distanceReward = 1.0f / (1.0f + distanceToNextWaypoint);
        
        // 언덕 보상
        float heightDifference = circuit.waypoints[nextWaypointIndex % circuit.waypoints.Length].transform.position.y - transform.position.y;
        float hillReward = heightDifference > 0 ? 1.0f : 1.0f + heightDifference;

        float totalReward = driftReward * distanceReward * hillReward;
        if (action == 0) // 가속인 경우
        {
            totalReward *= 1.9f; // 가속 보상 증가
        }
        else // 브레이크인 경우
        {
            totalReward *= 0.1f; // 브레이크 페널티 적용
        }
        
        return totalReward;    
    // 현재 액션에 따라 추가 보상/페널티 적용

        
        // 종합적인 보상 계산
        //float totalReward = driftReward * distanceReward * hillReward;
        //return totalReward;


    }


    void SaveQTable()
    {
        using (StreamWriter writer = new StreamWriter(qTableFilePath))
        {
            for (int i = 0; i < circuit.waypoints.Length; i++)
            {
                writer.WriteLine($"{qTable[i, 0]} {qTable[i, 1]}");
            }
        }
    }

    void LoadQTable()
    {
        using (StreamReader reader = new StreamReader(qTableFilePath))
        {
            int i = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(' ');
                qTable[i, 0] = float.Parse(values[0]);
                qTable[i, 1] = float.Parse(values[1]);
                i++;
            }
        }
    }
}

