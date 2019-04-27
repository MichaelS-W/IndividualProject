using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class HeuristicLogic : Decision
{
    public override float[] Decide(List<float> vectorObs,List<Texture2D> visualObs,float reward,bool done,List<float> memory)
    {
        float[] act = new float[brainParameters.vectorActionSize.Length];
        if (vectorObs[1] > 2.5f && vectorObs[0] == 1f)
        {
            act[0] = 2f;
        }
        else if (vectorObs[1] < -2.5f && vectorObs[0] == 0f)
        {
            act[0] = 2f;
        }
        else if (vectorObs[1] > 2.5f && vectorObs[0] == 0f)
        {
            act[0] = 1f;
        }
        else if (vectorObs[1] < -2.5f && vectorObs[0] == 1f)
        {
            act[0] = 1f;
        }
        else
        {
            act[0] = 0f;
        }
        return act;
        }


    public override List<float> MakeMemory(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        return new List<float>();
    }
}
