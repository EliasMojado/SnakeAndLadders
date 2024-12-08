using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SnakeBody : MonoBehaviour
{
    public int length = 10;
    public LineRenderer body;
    public Transform snakehead;
    private Vector3[] segmentPositions;
    private Vector3[] segmentVelocities;
    public float smoothTime = 0.1f;
    public float gravity = -9.81f;
    public float segmentDistance = 1f; // Distance between segments

    // Start is called before the first frame update
    void Start()
    {
        body.positionCount = length;
        segmentPositions = new Vector3[length];
        segmentVelocities = new Vector3[length];
        segmentPositions[0] = this.snakehead.position;
        body.SetPosition(0, this.snakehead.position);

        for (int i = 1; i < length; i++)
        {
            segmentPositions[i] = segmentPositions[i - 1] - snakehead.forward * segmentDistance;
            body.SetPosition(i, segmentPositions[i]);
        }
    }

    private void OnDestroy()
    {
        body.positionCount = 0;
        segmentPositions = null;
        segmentVelocities = null;
    }
    // Update is called once per frame
    void Update()
    {
        segmentPositions[0] = this.snakehead.position;
        body.SetPosition(0, this.snakehead.position);

        for (int i = 1; i < length; i++)
        {
            // Apply gravity to the segment velocity
            segmentVelocities[i].y += gravity * Time.deltaTime;

            // Smoothly move the segment towards the previous segment's position
            segmentPositions[i] = Vector3.SmoothDamp(segmentPositions[i], segmentPositions[i - 1], ref segmentVelocities[i], smoothTime);

            // Update the LineRenderer position
            body.SetPosition(i, segmentPositions[i]);
        }
    }
}

