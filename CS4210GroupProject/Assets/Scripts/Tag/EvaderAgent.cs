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
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private TextMeshPro rewardText;
    [SerializeField] private float maxSurvivalTime = 15f;
    private float survivalTimer = 0f;
    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-8,-1), 0, Random.Range(-8,8));
        transform.eulerAngles = new Vector3(0,90,0);
        survivalTimer = 0f;
    }
    
    // Define actions that the agent can do
    public override void OnActionReceived(ActionBuffers actions)
    {
        survivalTimer += Time.deltaTime;
        
        
        if (survivalTimer >= maxSurvivalTime)
        {
            // SUCCESS! Agent survived long enough
            AddReward(+10f);  // Give big reward
            Debug.Log("Evader won by surviving! " + GetCumulativeReward());

            // Optionally punish the chaser
            target.GetComponent<ChaserAgent>().AddReward(-10f);

            // End episodes for both agents
            EndEpisode();
            target.GetComponent<ChaserAgent>().EndEpisode();
        }
        
        AddReward(0.001f);
        rewardText.text = GetCumulativeReward().ToString("0.000");
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        // Debug.Log($"Move X: {moveX} | Move Z: {moveZ}" );
        rb.linearVelocity += new Vector3(moveX, 0, moveZ) * moveSpeed;
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVel.sqrMagnitude > 0.25f)
        {
            transform.forward = flatVel.normalized;
        }
    }

    // Collects information about the world
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's own position and velocity (x, z only)
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);

        // Target's position and velocity (x, z only)
        sensor.AddObservation(target.transform.localPosition.x);
        sensor.AddObservation(target.transform.localPosition.z);
        sensor.AddObservation(target.linearVelocity.x);
        sensor.AddObservation(target.linearVelocity.z);

        Debug.Log("Observation count: " + sensor.ObservationSize());
    }

    
    // For testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }
    
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f);
        }
    }
    
}
