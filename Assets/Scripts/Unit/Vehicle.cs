using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Vehicle : MonoBehaviour
{
    static List<Vehicle> vehicleList = new List<Vehicle>();
    protected static float AllowedOverlap = 0.3f;
    public static float frictionScale = 1f;

    [HideInInspector]
    public Transform targetTransform;
    
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

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    private bool flying = false;
    public bool Flying
    {
        get => flying;
        set
        {
            flying = value;
            rb.useGravity = !value;
        }
    }

    [HideInInspector]
    public UnityEvent<PathNode> PathNodeReachedEvent = new UnityEvent<PathNode>();

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

        float sphereCastDistance = 0.2f;

        if(Physics.SphereCast(sphereCollider.transform.position + sphereCollider.center + sphereCastDistance * Vector3.up, sphereCollider.radius * 2/3, Vector3.down, out RaycastHit hit, 2 * sphereCastDistance + sphereCollider.radius * 1/3, LayerMask.GetMask("Default")))
        {
            rb.useGravity = false;
        }
        else
        {
            if(!Flying) rb.useGravity = true;
        }
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

    public Vector3 Seek(bool localAvoidance = true)
    {
        Vector3 desired = GetDesiredVelocity();

        if (localAvoidance) desired = LocalAvoidance(desired);

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    public Vector3 Seek(Vector3 position, bool localAvoidance = true)
    {
        Vector3 desired = GetDesiredVelocity(position);

        if (localAvoidance) desired = LocalAvoidance(desired);

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    public Vector3 SeekPath(Path path, bool localAvoidance = true)
    {
        if(path.nodeList.Count < 1)
        {
            return Vector3.zero;// Seek();
        }

        Vector3 targetPoint = H.Flatten(path.PeekNode().GridSpace.Center);

        Vector3 desired = GetDesiredVelocity(targetPoint, true);

        if(localAvoidance) desired = LocalAvoidance(desired);

        Vector3 currentPosition = H.Flatten(transform.position);

        if(Vector3.Distance(targetPoint, currentPosition) < sphereCollider.radius + .5f)
        {
            PathNode removedNode = path.PopNode();
            PathNodeReachedEvent.Invoke(removedNode);
        }

        Vector3 steer = desired - rb.velocity;
        steer.y = 0;

        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    public Vector3 AvoidEdges()
    {
        Vector3 dropOffDirectionsAdded = Vector3.zero;
        bool dropOffDetected = false;
        for(int i = 0; i < 8; i++)
        {
            Vector3 rotated = Quaternion.Euler(0, i * 360f / 8f, 0) * Vector3.forward * (sphereCollider.radius + 1.5f);

            if (Physics.SphereCast(transform.position + rotated + 0.1f * Vector3.up, sphereCollider.radius, Vector3.down, out RaycastHit hitInfo, 100))
            {
                if (hitInfo.distance > 0.11)
                {
                    // There is a drop off in this direction
                    dropOffDirectionsAdded += rotated;
                    dropOffDetected = true;
                }
                else
                {
                    // There is solid ground in this direction
                }
            }
            else
            {
                // There is a drop off in this direction
                dropOffDirectionsAdded += rotated;
                dropOffDetected = true;
            }
        }

        if (dropOffDetected)
        {
            Vector3 backTowardsSafety = -dropOffDirectionsAdded.normalized;

            Vector3 desired = backTowardsSafety * maxSpeed * 1.5f;

            Vector3 steer = desired - rb.velocity;
            steer.y = 0;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }
        else
        {
            return Vector3.zero;
        }
    }

    protected Vector3 Arrive(bool localAvoidance = true, float arriveDistance = -1)
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

        if (localAvoidance) desired = LocalAvoidance(desired);

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
            steer.y = 0;

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
            steer.y = 0;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }


    public void FixPosition()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + sphereCollider.center, sphereCollider.radius, 1 << gameObject.layer);

        foreach (Collider c in colliders)
        {
            if (c.isTrigger) continue;
            if (c == sphereCollider) continue;
            if (Physics.ComputePenetration(sphereCollider, transform.position + sphereCollider.center, transform.rotation, c, c.transform.position, c.transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 penetrationVector = direction * (distance);
                transform.position = transform.position + penetrationVector;
            }
        }
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
            steer.y = 0;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    protected Vector3 LocalAvoidance(Vector3 intendedMove)
    {
        bool HittingOtherFarmon = Physics.SphereCast(transform.position + sphereCollider.center, sphereCollider.radius, intendedMove.normalized, out RaycastHit hitInfo, sphereCollider.radius, LayerMask.GetMask("Farmon"));

        if (HittingOtherFarmon)
        {
            Vector3 rotatedIntendedMove = Quaternion.Euler(0, 90, 0) * intendedMove;

            return rotatedIntendedMove;
        }

        return intendedMove;
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
            steer.y = 0;

            steer = Vector3.ClampMagnitude(steer, maxForce);

            return steer;
        }

        return Vector3.zero;
    }

    public Vector3 Friction(Vector3 desiredVelocity)
    {
        Vector3 desired = H.Flatten(desiredVelocity);

        if(desired.magnitude < 0.1f)
        {
            Vector3 steer = -rb.velocity * frictionScale;
            steer.y = 0;

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
