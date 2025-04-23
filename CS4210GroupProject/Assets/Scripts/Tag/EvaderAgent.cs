using System;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class EvaderAgent : Agent
{
    [SerializeField] private Rigidbody target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer floorRenderer;
    [SerializeField] private float moveSpeed = 4f, rotateSpeed = 4f, jumpForce = 10f, gravity = 9.81f;
    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        // transform.localPosition = new Vector3(Random.Range(-8,-1), 0, Random.Range(-8,8));
        transform.localPosition = new Vector3(-7, 0, 4.75f);
        transform.eulerAngles = new Vector3(0,90,0);
    }


    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    // Define actions that the agent can do
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(0.001f);
        
        float move = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];

        // Debug.Log($"Move X: {moveX} | Move Z: {moveZ}" );
        if (IsGrounded())
        {
            float yVelocity = rb.linearVelocity.y;
            rb.linearVelocity = transform.forward * move * moveSpeed + Vector3.up * yVelocity;
        }
        
        transform.localEulerAngles += Vector3.up * rotate * rotateSpeed;
        
        // Discrete jump
        int jumpAction = actions.DiscreteActions[0];
        Debug.Log("JUMP" + jumpAction);
        if (jumpAction == 1 && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Collects information about the world
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);                  // 3
        sensor.AddObservation(transform.localEulerAngles.y);
        sensor.AddObservation(target.transform.localPosition);           // 3
        sensor.AddObservation(target.transform.localEulerAngles.y);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, target.transform.localPosition)); // 3
        sensor.AddObservation(IsGrounded() ? 1f : 0f);
        Debug.Log("Observation count: " + sensor.ObservationSize());
    }

    
    // For testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
        
        var disc = actionsOut.DiscreteActions;
        disc[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
    
    // private void OnCollisionStay(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Wall"))
    //     {
    //         AddReward(-0.01f);
    //     }
    // }
    private bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * 0.6f, Color.green);
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, LayerMask.GetMask("Ground"));
    }
}
