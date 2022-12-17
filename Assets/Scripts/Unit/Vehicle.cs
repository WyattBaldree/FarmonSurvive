using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Vehicle : MonoBehaviour
{
    static List<Vehicle> vehicleList = new List<Vehicle>();
    protected static float AllowedOverlap = 0.3f;

    [HideInInspector]
    public Transform targetTransform;
    [HideInInspector]
    public SphereCollider sphereCollider;

    float startingRadius;

    [HideInInspector]
    public float maxSpeed;

    protected float maxForce = 50;

    float wanderDistance = 3;
    float wanderAngle = 2;
    float wanderRadius = 1;
    float wanderAngleDelta = 30;
    Vector3 wanderCenter;
    Vector3 wanderVector;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        vehicleList.Add(this);
    }

    protected virtual void OnDestroy()
    {
        vehicleList.Remove(this);
    }
    
    public void SetSize(float newSize)
    {
        if (sphereCollider)
        {
            sphereCollider.radius = startingRadius + (startingRadius * newSize / 40);
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        Assert.IsNotNull(rb);

        sphereCollider = GetComponent<SphereCollider>();
        Assert.IsNotNull(sphereCollider);

        startingRadius = sphereCollider.radius;
    }

    protected virtual void Update()
    {
        wanderAngle += Random.Range(-wanderAngleDelta, wanderAngleDelta) * Time.deltaTime;
    }

    Vector3 GetDesiredVelocity(bool flat = true)
    {
        return GetDesiredVelocity(targetTransform.position, flat);
    }

    Vector3 GetDesiredVelocity(Vector3 targetPosition, bool flat = true)
    {
        Vector3 desired = targetPosition - transform.position;
        if (flat) desired.y = 0;
        return desired.normalized * maxSpeed;
    }

    public void MoveInDirection(Vector3 direction)
    {
        Vector3 v = direction.normalized * maxSpeed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }

    public Vector3 Seek()
    {
        Vector3 desired = GetDesiredVelocity();

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    protected Vector3 Arrive(float arriveDistance = -1)
    {
        if (arriveDistance == -1) arriveDistance = maxSpeed / 2;

        Vector3 desired = targetTransform.position - transform.position;
        desired.y = 0;
        float distanceFromTarget = desired.magnitude;
        desired = desired.normalized;

        if(distanceFromTarget < arriveDistance)
        {
            desired *= (maxSpeed * distanceFromTarget/arriveDistance);
        }
        else
        {
            desired *= maxSpeed;
        }

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    public Vector3 MinDistance(Vector3 point, float minDistance)
    {
        float softDistance = 3f;

        Vector3 myPositionFlat = H.Flatten(transform.position);
        Vector3 targetPositionFlat = H.Flatten(point);

        float currentDistance = Vector3.Distance(targetPositionFlat, myPositionFlat);
        if (currentDistance < minDistance)
        {
            Vector3 desired = (myPositionFlat - targetPositionFlat).normalized * (minDistance - currentDistance);
            desired.y = 0;
            float distanceFromTarget = desired.magnitude;
            desired = desired.normalized;

            if (distanceFromTarget < softDistance)
            {
                desired *= maxSpeed * distanceFromTarget / softDistance;
            }
            else
            {
                desired *= maxSpeed;
            }

            Vector3 steer = desired - rb.velocity;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    public Vector3 MaxDistance(Vector3 point, float MaxDistance)
    {
        float softDistance = 3f;

        Vector3 myPositionFlat = H.Flatten(transform.position);
        Vector3 targetPositionFlat = H.Flatten(point);

        float currentDistance = Vector3.Distance(targetPositionFlat, myPositionFlat);
        if (currentDistance > MaxDistance)
        {
            Vector3 desired = (targetPositionFlat - myPositionFlat).normalized * (currentDistance - MaxDistance);
            desired.y = 0;
            float distanceFromTarget = desired.magnitude;
            desired = desired.normalized;

            if(distanceFromTarget < softDistance)
            {
                desired *= maxSpeed * distanceFromTarget/softDistance;
            }
            else
            {
                desired *= maxSpeed;
            }

            Vector3 steer = desired - rb.velocity;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    protected Vector3 Flee()
    {
        Vector3 desired = GetDesiredVelocity();

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return -steer;
    }

    protected Vector3 FleeTransform()
    {
        return Flee();
    }

    protected Vector3 Wander()
    {
        wanderCenter = transform.position + (rb.velocity.normalized * wanderDistance);
        wanderVector = new Vector3(Mathf.Cos(wanderAngle) * wanderRadius, 0, Mathf.Sin(wanderAngle) * wanderRadius);

        Vector3 desired = GetDesiredVelocity(wanderCenter + wanderVector);

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    protected Vector3 Seperate(List<Vehicle> vehicles, float desiredSeparation)
    {
        Vector3 vectorSum = new Vector3();
        int count = 0;
        foreach(Vehicle v in vehicles)
        {
            if (v == this) continue;

            float d = Vector3.Distance(transform.position, v.transform.position);
            if(d < desiredSeparation + v.sphereCollider.radius)
            {
                Vector3 toMe = transform.position - v.transform.position;
                toMe.Normalize();

                vectorSum += toMe;
                count++;
            }
        }

        if (count > 0)
        {
            vectorSum /= count;

            Vector3 steer = (vectorSum.normalized * maxSpeed) - rb.velocity;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    protected Vector3 SoftSeperate(List<Vehicle> vehicles, float desiredSeparation, float scale = 4)
    {
        Vector3 vectorSum = new Vector3();
        int count = 0;
        foreach (Vehicle v in vehicles)
        {
            if (v == this) continue;

            float d = Vector3.Distance(transform.position, v.transform.position);
            if (d < desiredSeparation + v.sphereCollider.radius)
            {
                Vector3 toMe = transform.position - v.transform.position;
                toMe = toMe.normalized * (1 - (d/((desiredSeparation + v.sphereCollider.radius - AllowedOverlap) * 2)));

                vectorSum += toMe;
                count++;
            }
        }

        if (count > 0)
        {
            vectorSum /= count;

            Vector3 steer = Vector3.ClampMagnitude(vectorSum*scale, maxSpeed) - rb.velocity;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if(rb) Debug.DrawLine(transform.position, transform.position + rb.velocity);
        if (rb)
        {

            //DebugExtension.DrawCircle(wanderCenter, Vector3.back, Color.red, wanderRadius);


            Debug.DrawLine(wanderCenter, wanderCenter + wanderVector, Color.red);
        }
    }
}
