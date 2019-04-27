using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Text;
using System.IO;

public class WaypointAgent3 : Agent
{
    public GameObject destination;
    public GameObject plane;
    public GameObject game;
  
    /* Variables */
    private float turnrate = 100f;
    private float velocity = 50f;
    Vector3 rotateDir = Vector3.zero;
    Vector3 spawnone = new Vector3(0, 1, 0);
    private float angle;
    private float radius;
    private int turndirection;
    private float insidecircle;

    // Variables for writing to CSV
    private bool metwaypoint;
    private bool timeout;
    private float score;
    private List<string[]> rowData = new List<string[]>();
    private float scorePrev;
    private float time;
    private float timePrev;


    // necessary for the agent class
    public override void InitializeAgent()
    {
        plane.transform.position = game.transform.position + spawnone;
        SpawnDestination();
        radius = CalcRadius();
        time = 0f;
        score = 0f;
        metwaypoint = false;
        timeout = false;
    }

    private void FixedUpdate()
    {

        plane.transform.Rotate(Vector3.up, turndirection * Time.fixedDeltaTime * turnrate);
        // Constant Velocity forwards for plane
        plane.transform.position += plane.transform.forward * velocity * Time.fixedDeltaTime;
    }


    // Observations required for the training
    public override void CollectObservations()
    {
        AddVectorObs(InsideTurnCircle(radius));
        AddVectorObs(CalcAngle());

    }
    // Class that maps agent's decision to movement
    private int MoveAgent(float[] act)
    {
      
        // setting actions to movement turnAction 1 == right, turnAction 2 == left, turnAction 0 == nothing.
        int turnAction = (int)act[0];
        if(turnAction == 1)
        {
            turndirection = 1;
        }
        else if(turnAction == 2)
        {
            turndirection = -1;
        }
        else
        {
            turndirection = 0;
        }
        return turndirection;
    }

    // Class that provides  agent with rewards and decides what action to take
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        time += 1f;
        MoveAgent(vectorAction);


        if (Vector3.Distance(destination.transform.position,plane.transform.position) <= 2f)
        {
            AddReward(1f);
            score += 1f;
            metwaypoint = true;

            //GetData() is commented out unless collecting the scenario data
            GetData();
            Done();
        }

        // Negative Reward and reset if agent is too far from destination
        if (Vector3.Distance(destination.transform.position, transform.position) >= 1000f)
        {
            AddReward(-1f);
            score += -1f;
            Done();
        }
        // Negative time based reward
        AddReward(-0.0001f);
        score += -0.0001f;

        if(time == 999f)
        {
            timeout = true;
        }

        GetData();
    }

    // Called when max iterations or Done() is called
    public override void AgentReset()
    {
        plane.transform.position = game.transform.position + spawnone;
        plane.transform.eulerAngles = new Vector3(0, 0, 0);
        SpawnDestination();
    }

    //  Calculates the current Heading Angle Error
    float CalcAngle()
    {
        Vector3 waypointdir = destination.transform.position - plane.transform.position;
        Vector3 forward = plane.transform.forward;
        angle = Vector3.SignedAngle(forward, waypointdir, Vector3.up);
        return angle;
    }

    // Calculates the Turn Radius
    float CalcRadius()
    {
        radius = velocity / (turnrate * Mathf.PI / 180f);
        return radius;
    }

    //  Moves the Waypoint
    public void SpawnDestination()
    {
        //Destination Spawning Randomly: Used During Training (commented out during testing)
        //destination.transform.position = new Vector3(Random.Range(-100, 100),
        //1, Random.Range(-100, 100)) + game.transform.position;

        // Scenario Waypoints
        //(Commented Out unless the specific scenario is being tested)

        ////Destination for Scnenario 1:
        //destination.transform.position = new Vector3(-100f * Mathf.Sin(45f), 1f, 100f * Mathf.Cos(45f));
        ////Destiation for Scenario 2:
        //destination.transform.position = new Vector3(0f, 1f, 100f);
        ////Destination for Scenario 3:
        //destination.transform.position = new Vector3(100f * Mathf.Sin(45f), 1f, 100f * Mathf.Cos(45f));
        ////Destination for Scenario 4:
        //destination.transform.position = new Vector3(-100f, 1f, 0f);
        ////Destination for Scenario 5:
        //destination.transform.position = new Vector3(-radius, 1f, 0f);
        ////Destination for Scenario 6:
        //destination.transform.position = new Vector3(radius, 1f, 0f);
        ////Destination for Scenario 7:
        //destination.transform.position = new Vector3(100f, 1f, 0f);
        ////Destination for Scenario 8:
        //destination.transform.position = new Vector3(-100f * Mathf.Sin(45f), 1f, -100f * Mathf.Cos(45f));
        ////Destination for Scenario 9:
        //destination.transform.position = new Vector3(0f, 1f, -100f);
        ////Destination for Scenario 10:
        //destination.transform.position = new Vector3(100f * Mathf.Sin(45f), 1f, -100f * Mathf.Cos(45f));
    }

    //Gives players visual HUD of the Current Heading angle Error and the Boolean Value "insidecircle"
    //(commented out unless doing player tests)

    void OnGUI()
    {
        GUI.Label(new Rect(50, 0, 100, 20), "Angle: " + angle);
        GUI.Label(new Rect(50, 10, 100, 20), "Inside Circle: " + insidecircle);

    }


    // Returns a boolean true if the waypoint is inside either of the circles and false if it is outside
    float InsideTurnCircle(float R)
    {

        // Circle Centres
        Vector3 LeftCircleCentre = plane.transform.position - radius * plane.transform.right;
        Vector3 RightCircleCentre = plane.transform.position + radius * plane.transform.right;

        // Finds if the waypoint lies within the circles using "completing the square" and returns true or false
        if ((Mathf.Pow(LeftCircleCentre.x - destination.transform.position.x, 2) + Mathf.Pow((LeftCircleCentre.z - destination.transform.position.z), 2)) <= Mathf.Pow(radius, 2))
        {
            insidecircle = 1f;
        }
        else if (Mathf.Pow(RightCircleCentre.x - destination.transform.position.x, 2) + Mathf.Pow((RightCircleCentre.z - destination.transform.position.z), 2) <= Mathf.Pow(radius, 2))
        {
            insidecircle = 1f;
        }
        else
        {
            insidecircle = 0f;
        }
        return insidecircle;
    }

    // Methods for writing data to file
    public void GetData()
    {
        if(time == 1f)
        {
            Save();
        }


        float xf = plane.transform.position.x;
        float zf = plane.transform.position.z;
        float xw = destination.transform.position.x;
        float zw = destination.transform.position.z;
        if (metwaypoint)
        {
            score = scorePrev + 1f;
            xf = xw;
            zf = zw;
            time = timePrev + 1f;
        }
        string[] rowDataTemp = new string[6];

        rowDataTemp[0] = xf.ToString();
        rowDataTemp[1] = zf.ToString();
        rowDataTemp[2] = xw.ToString();
        rowDataTemp[3] = zw.ToString();
        rowDataTemp[4] = time.ToString();
        rowDataTemp[5] = score.ToString();
        rowData.Add(rowDataTemp);

        if (metwaypoint || timeout)
        {
            Writetofile();
        }

        timePrev = time;
        scorePrev = score;

    }


    void Save()
    {
        string[] rowDataTemp = new string[6];
        rowDataTemp[0] = "Xf";
        rowDataTemp[1] = "Zf";
        rowDataTemp[2] = "Xw";
        rowDataTemp[3] = "Zw";
        rowDataTemp[4] = "Time";
        rowDataTemp[5] = "Score";
        rowData.Add(rowDataTemp);
    }

    void Writetofile()
    {
        Debug.Log("Writing File");
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));


        string filePath = GetPath();

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();



    }

    private string GetPath()
    {
        return Application.dataPath + "/CSV/" + "Player_4_Data_10.csv";
    }


}


