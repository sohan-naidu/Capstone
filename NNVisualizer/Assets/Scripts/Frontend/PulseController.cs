using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PulseController : MonoBehaviour
{
    private List<List<Vector3>> positions;
    private List<int> correspondingLayer;
    private List<List<Vector3>> layerPositions;
    private int currentLayer = 0;
    private List<GameObject> trailObjects = new List<GameObject>();
    public GameObject NNPulsePrefab;
    bool sendPulse = false;
    private float moveSpeed = 0;
    public int numLayers = 0;
    float elapsedTime;

    // TODO: read all the comments and incorporate changes
    // do it layer by layer: when the current positions array is empty, add the positions from the next layer into the array while there are still positions left
    public void sendNNPulse(List<List<Vector3>> NeuronPositions, List<int> NNCorrespondingLayer, float moveSpeedVal)
    {
        for (int i = 0; i < NeuronPositions.Count; i++)
        {
            positions.Add(NeuronPositions[i]);
            correspondingLayer.Add(NNCorrespondingLayer[i]);
        }
        sendPulse = true;
        moveSpeed = moveSpeedVal;
    }

    void Start()
    {
        elapsedTime = 0;
        positions = new List<List<Vector3>>();
        layerPositions = new List<List<Vector3>>();
        correspondingLayer = new List<int>();
    }

    void Update()
    {
        if (numLayers <= 0) return;

        // TODO: make an assertion to ensure that we're not stuck in an infinite loop
        int runs = 0;
        while (positions.Count > 0 && layerPositions.Count == 0)
        {
            runs++;
            Assert.IsTrue(runs < 100);
            elapsedTime = 0;
            currentLayer--;
            if (currentLayer < 0) currentLayer += numLayers;
            for (int i = 0; i < positions.Count; i++) {
                if (correspondingLayer[i] == currentLayer)
                {
                    layerPositions.Add(positions[i]);
                }
            }
        }

        if (sendPulse)
        {
            int n = layerPositions.Count;
            int currentSize = trailObjects.Count;
            while (currentSize < n)
            {
                GameObject reference = Instantiate(NNPulsePrefab);
                reference.transform.position = layerPositions[currentSize][0];
                GameObject temp = new GameObject();
                Transform target = temp.transform;
                target.position = layerPositions[currentSize][1];
                reference.transform.LookAt(target);
                Destroy(temp);
                trailObjects.Add(reference);
                currentSize++;
            }

            List<bool> marked = new List<bool>(new bool[n]);
            for (int i = 0; i < n; i++)
            {
                Vector3 EndPoint = layerPositions[i][1];
                if (trailObjects[i].transform.position != EndPoint)
                {
                    Vector3 directionToMove = EndPoint - trailObjects[i].transform.position;
                    directionToMove = directionToMove.normalized;
                    float stepSize = moveSpeed * Time.fixedDeltaTime;
                    directionToMove = directionToMove * stepSize;
                    float maxDistance = Vector3.Distance(trailObjects[i].transform.position, EndPoint);
                    //Vector3 distTravelled = Vector3.ClampMagnitude(directionToMove, maxDistance);
                    //Vector3 LineDistanceVector = EndPoint - layerPositions[i][0];
                    //float totalDistance = LineDistanceVector.magnitude;
                    //float timeRatio = elapsedTime / 2.0f;
                    //float expectedDistance = totalDistance * timeRatio;
                    //float distanceThisFrame = expectedDistance - (trailObjects[i].transform.position - layerPositions[i][0]).magnitude;
                    trailObjects[i].transform.position = trailObjects[i].transform.position + Vector3.ClampMagnitude((directionToMove * 8.0f), maxDistance);
                    //trailObjects[i].transform.position = trailObjects[i].transform.position + Vector3.ClampMagnitude(directionToMove, distanceThisFrame);
                    //trailObjects[i].transform.position = trailObjects[i].transform.position + (directionToMove * distanceThisFrame);
                } else {
                    marked[i] = true;
                } 
            }

            int cur = 0;
            while (cur < trailObjects.Count)
            {
                if (marked[cur]) 
                {
                    Destroy(trailObjects[cur]);
                    marked.RemoveAt(cur);
                    trailObjects.RemoveAt(cur);
                    layerPositions.RemoveAt(cur); // make this happen one layer at a time
                } 
                else 
                {
                    ++cur;
                }
            }

            elapsedTime += Time.deltaTime;
        }
    }
}
