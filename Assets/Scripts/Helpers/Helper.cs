using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class H
{
    public static Vector3 Flatten(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3Int Vector3ToGridPosition(Vector3 v, float gridSize)
    {
        int x = Mathf.FloorToInt(v.x / gridSize);
        int y = Mathf.FloorToInt(v.y / gridSize);
        int z = Mathf.FloorToInt(v.z / gridSize);

        return new Vector3Int(x, y, z);
    }

    public static Vector3 GridPositionToVector3(Vector3Int v, float gridSize)
    {
        float x = v.x * gridSize;
        float y = v.y * gridSize;
        float z = v.z * gridSize;

        return new Vector3(x, y, z);
    }

    //When navigating, farmon use the first navigable terrain below themself/below their target.
    //This function quickly returns the gridSpace position of the naviagable terrain below the provided position.
    public static Vector3Int GetNavigationPosition(Vector3 realPosition)
    {
        //First get gridSpace this position is in
        Vector3Int gridPosition = H.Vector3ToGridPosition(realPosition, LevelController.Instance.gridSize);

        //Next, check if our gridspace is valid movement terrain. If not, keep moving
        //down until we find valid terrain.
        while (true)
        {
            if (gridPosition.y < 0)
            {
                Debug.Log("A farmon is trying to navigate from or to a position with no valid navigation terrain beneath it.");
                return Vector3Int.zero;
                //throw new Exception("A farmon is trying to navigate from or to a position with no valid navigation terrain beneath it.");
            }

            GridSpace gridSpace = NavMesh.instance.GetGridSpaceArray(gridPosition);

            // If the current gridSpace exists and has valid terrain, use it!
            if (gridSpace != null && gridSpace.CenterValid)
            {
                break;
            }

            //otherwise look downward in case this farmon is airborne
            gridPosition += Vector3Int.down;
        }

        return gridPosition;
    }

    public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        }
        objects.Sort();
        return objects;
    }

    /// <summary>
    /// This function returns how submerged in water an object is based on it's position and height
    /// </summary>
    /// <param name="center">The center of the object we are checking.</param>
    /// <param name="height">The height of the obejct we are checking</param>
    /// <param name="waterCheckMaxCastDistance">How far upwards we should check. Would only ever need to be changed in extreme cases.</param>
    /// <returns>Returns how submerged the object is expressed as a float on the range of 0.0f (not submerged) to 1.0f (fully submerged)</returns>
    public static float SubmergenceCheck(Vector3 center, float height, float waterCheckMaxCastDistance = 10000)
    {
        Physics.queriesHitBackfaces = true;
        int l = LayerMask.GetMask("Water");

        RaycastHit[] hitBufferList = Physics.RaycastAll(center - (height / 2 * Vector3.up), Vector3.up, waterCheckMaxCastDistance, LayerMask.GetMask("Water"));
        Physics.queriesHitBackfaces = false;

        // The submerged variable tracks how many objects we are submerged in.
        int submergedCount = 0;

        // SubmergedHighest Keeps track of our highest recorded submerged value.
        // When this reaches a new highest value, that is the new surface of the water.
        int submergedCountHighest = 0;

        float distanceToSurface = -1;

        for (int i = 0; i < hitBufferList.Length; i++)
        {
            // Hitting the inside of a water body increased submerged while hitting the outside decreases submerged
            if ((Vector3.Dot(Vector3.up, hitBufferList[i].normal) > 0))
            {
                submergedCount++;
            }
            else
            {
                submergedCount--;
            }

            // When we hit a new highest submerged value, consider that the new surface
            if (submergedCount > submergedCountHighest)
            {
                submergedCountHighest = submergedCount;

                distanceToSurface = hitBufferList[i].distance;
            }
        }

        // If we are submerged, set submergence accordingly
        if (submergedCount > 0)
        {
            if (distanceToSurface > height)
            {
                return 1f;
            }
            else
            {
                return distanceToSurface / height;
            }
        }
        else
        {
            return 0;
        }
    }

    public static void RestartCoroutine(MonoBehaviour self, ref Coroutine coroutineVariable, IEnumerator coroutineMethod)
    {
        if (coroutineVariable != null) self.StopCoroutine(coroutineVariable);
        coroutineVariable = self.StartCoroutine(coroutineMethod);
    }

    public static int GetLayerMaskFromGameObject(GameObject gameObject)
    {
        int layerMask = 0;

        //Build layerMask
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
            {
                layerMask = layerMask | 1 << i;
            }
        }

        return layerMask;
    }
}

class WeightedRandomBag<T>
{

    private struct Entry
    {
        public double accumulatedWeight;
        public T item;
    }

    private List<Entry> entries = new List<Entry>();
    private double accumulatedWeight;
    private System.Random rand = new System.Random();

    public void AddEntry(T item, double weight)
    {
        accumulatedWeight += weight;
        entries.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
    }

    public T GetRandom()
    {
        double r = rand.NextDouble() * accumulatedWeight;

        foreach (Entry entry in entries)
        {
            if (entry.accumulatedWeight >= r)
            {
                return entry.item;
            }
        }
        return default(T); //should only happen when there are no entries
    }
}

