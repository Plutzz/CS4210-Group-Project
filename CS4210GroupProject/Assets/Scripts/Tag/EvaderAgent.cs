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
    [SerializeField] private TextMeshPro rewardText;
    [SerializeField] private float maxSurvivalTime = 10f;
    private float survivalTimer = 0f;
    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        // transform.localPosition = new Vector3(Random.Range(-8,-1), 0, Random.Range(-8,8));
        transform.localPosition = new Vector3(-5, 0, 0);
        transform.eulerAngles = new Vector3(0,90,0);
        survivalTimer = 0f;
    }


    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    // Define actions that the agent can do
    public override void OnActionReceived(ActionBuffers actions)
    {
        survivalTimer += Time.deltaTime;
        
        if (survivalTimer >= maxSurvivalTime)
        {
            // SUCCESS! Agent survived long enough
            AddReward(+1f);  // Give big reward
            Debug.Log("Evader won by surviving! " + GetCumulativeReward());

            // Optionally punish the chaser
            target.GetComponent<ChaserAgent>().AddReward(-1f);

            // End episodes for both agents
            EndEpisode();
            target.GetComponent<ChaserAgent>().EndEpisode();
        }
        
        AddReward(0.001f);
        rewardText.text = GetCumulativeReward().ToString("0.000");
        
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
