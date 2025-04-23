using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TMPro;
public class ChaserAgent : Agent
{
    [SerializeField] private Rigidbody target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer floorRenderer;
    [SerializeField] private float moveSpeed = 4f, rotateSpeed = 4f, jumpForce = 10f, gravity = 9.81f;
    [SerializeField] private TextMeshPro episodeText, evaderText, chaserText, timerText;
    private float maxSurvivalTime = 10f, survivalTimer;
    private int episodeCounter = -1, chaserCounter, evaderCounter;
    public override void OnEpisodeBegin()
    {
        HandleStartEpisodeText();
        rb.linearVelocity = Vector3.zero;
        // transform.localPosition = new Vector3(Random.Range(1,8), 0, Random.Range(-8,8));
        transform.localPosition = new Vector3(7, 0, 4.75f);
        transform.eulerAngles = new Vector3(0,-90,0);
        survivalTimer = 0f;
    }

    private void HandleStartEpisodeText()
    {
        episodeCounter++;
        episodeText.text = episodeCounter.ToString();
        if (episodeCounter == 0)
        {
            evaderText.text = "50.0";
            chaserText.text = "50.0";
            return;
        }
        

        evaderText.text = ((evaderCounter / (float)episodeCounter) * 100f).ToString("00.0");
        chaserText.text = ((chaserCounter / (float)episodeCounter) * 100f).ToString("00.0");

        if (evaderCounter / (float)episodeCounter > 0.5f)
        {
            evaderText.color = Color.green;
            chaserText.color = Color.red;
        }
        else
        {
            evaderText.color = Color.red;
            chaserText.color = Color.green;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }
    
    // Define actions that the agent can do
    public override void OnActionReceived(ActionBuffers actions)
    {
        survivalTimer += Time.deltaTime;
        
        timerText.text = Mathf.RoundToInt(maxSurvivalTime - survivalTimer).ToString();
        
        if (survivalTimer >= maxSurvivalTime)
        {
            // Agent survived long enough
            AddReward(-1f);  // Give big reward
            evaderCounter += 1;
            
            target.GetComponent<EvaderAgent>().AddReward(1f);

            // End episodes for both agents
            EndEpisode();
            target.GetComponent<EvaderAgent>().EndEpisode();
        }
        
        AddReward(-0.001f);
        
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
        sensor.AddObservation(IsGrounded() ? 1f : 0f);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, target.transform.localPosition)); // 3
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

    private void OnCollisionEnter(Collision other)
    {
        // When we hit the goal trigger
        if (other.gameObject.CompareTag("Evader"))
        {
            // Give reward for both
            AddReward(1f);
            other.gameObject.GetComponent<EvaderAgent>().AddReward(-1f);
            
            chaserCounter += 1;
            
            // End episode for both
            EndEpisode();
            other.gameObject.GetComponent<EvaderAgent>().EndEpisode();
        }
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
