using System.Collections.Generic; 
using UnityEngine;
using System.Diagnostics;

public class AgentSpawner : MonoBehaviour
{
    public static AgentSpawner instance;
    public List<GameObject> Agents;

    public int prefabSelector = 0; 
    public GameObject ReynoldsPrefab;
    public GameObject SteeringPrefab;
    public GameObject OrcaPrefab; 

    public GameObject target;

    public Vector3 spawnPosition;
    public int numberOfSheep = 1; 
    public float width; 
    public float height;


    public Stopwatch sw;
    bool started; 
    public float evacuationTime; 
    public float MeanExitTime = 0f;
    public int TotalOscillations = 0;
    public float TotalAngle = 0f;
    public int TotalCollisions = 0; 
    public bool spawn = false;
   

    private void Awake()
    {
        Agents = new List<GameObject>();
        instance = this;
        started = false; 
    }



    // Update is called once per frame
    void Update()
    {
        Timer();
        if (spawn)
        {
            sw ??= new Stopwatch();
            sw.Start();
            started = true; 

            MeanExitTime = 0f;
            TotalOscillations = 0;
            TotalAngle = 0f;
            TotalCollisions = 0; 

            SpawnAgents();
            spawn = false;
        }
    }

    void SpawnAgents()
    {
        for (int i = 0; i < numberOfSheep; i++)
        {
            for (int j = 0; j < numberOfSheep; j++)
            {
                GameObject steeringAgent;
                if (prefabSelector == 0f)
                {
                    steeringAgent = Instantiate(ReynoldsPrefab);
                    steeringAgent.GetComponent<ReynoldsSteering>().target = target;
                }
                else if (prefabSelector == 1f)
                {
                    steeringAgent = Instantiate(SteeringPrefab);
                    steeringAgent.GetComponent<ComfortSteering>().target = target;  
                }
                else
                {
                    steeringAgent = Instantiate(OrcaPrefab);
                    steeringAgent.GetComponent<OrcaSteering>().target = target;
                }

                Vector3 rightOffset = Vector3.right * width * ((float)i / (numberOfSheep - 1));
                Vector3 forwardOffset = Vector3.forward * height * ((float)j / (numberOfSheep - 1));
                steeringAgent.transform.position = spawnPosition + rightOffset + forwardOffset;

                steeringAgent.transform.SetParent(transform, true);
                Agents.Add(steeringAgent);
            }
        }
    }

    void Timer()
    {    
        if (Agents.Count == 0 && started)
        {
            sw.Stop();
            evacuationTime = sw.ElapsedMilliseconds / 1000f;
            MeanExitTime /= Mathf.Pow(numberOfSheep + 1, 2); 
            started = false;
            sw.Reset();

            UnityEngine.Debug.Log("-------------------");
            UnityEngine.Debug.Log(GetName() + " with " + numberOfSheep * numberOfSheep + " sheep");
            UnityEngine.Debug.Log("Total time: " + evacuationTime); 
            UnityEngine.Debug.Log("Mean time: " + MeanExitTime);
            UnityEngine.Debug.Log("Mean Oscillations: " + TotalOscillations / (numberOfSheep * numberOfSheep));
            UnityEngine.Debug.Log("Mean Angle: " + TotalAngle / (numberOfSheep * numberOfSheep));
            UnityEngine.Debug.Log("Mean Collisions: " + TotalCollisions / (numberOfSheep * numberOfSheep));
            UnityEngine.Debug.Log("-------------------");
        }    
    }

    string GetName()
    {
        switch (prefabSelector)
        {
            case 0:
                return "Reynolds";
            case 1:
                return "Comfort";
            default:
                return "Orca"; 
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawnPosition, spawnPosition + Vector3.right * width);
        Gizmos.DrawLine(spawnPosition, spawnPosition + Vector3.forward * height);
    }

    public void AddTime()
    {
        MeanExitTime += sw.ElapsedMilliseconds / 1000f; 
    }

    public void AddRotation(int oscillations, float angle)
    {
        TotalAngle += angle; 
        TotalOscillations += oscillations;
    }

    public void AddCollisions(int collisions)
    {
        TotalCollisions += collisions;  
    }
}
