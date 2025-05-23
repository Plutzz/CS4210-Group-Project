using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Material winMaterial, loseMaterial;
    [SerializeField] private MeshRenderer floorRenderer;
    [SerializeField] private float moveSpeed = 4f;

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-1,3), 1, Random.Range(-2,2));
        target.localPosition = new Vector3(Random.Range(4,7), 1, Random.Range(-2,2));
    }
    
    // Define actions that the agent can do
    public override void OnActionReceived(ActionBuffers actions)
    {
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
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(rb.linearVelocity);
    }

    
    // For testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        Debug.Log("Heuristic: " + actions[0]);
    }

    private void OnTriggerEnter(Collider other)
    {
        // When we hit the goal trigger
        if (other.gameObject.CompareTag("Goal"))
        {
            SetReward(1f);
            floorRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            SetReward(-1f);
            floorRenderer.material = loseMaterial;
            EndEpisode();
        }
        
    }
}
