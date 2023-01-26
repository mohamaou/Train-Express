using System;
using System.Collections.Generic;
using UnityEngine;

public class Grade : MonoBehaviour
{
    private enum PointType
    {
        Normal, Obstacle, Full
    }
    public static Grade Instance {get; protected set;}

    [SerializeField] private bool showGizmos;
    [SerializeField] private int with, lenght;
    [SerializeField] private LayerMask obstacleLayer, goalLayer, peopleLayer;
    private static List<Vector3> gradePoints = new List<Vector3>();
    private static List<PointType> pointsState = new List<PointType>();
    private static List<bool> goal = new List<bool>(), crossroads = new List<bool>();


    void Start()
    {
        Instance = this;
        SetGradePoint();
    }
    public void SetGradePoint()
    {
        gradePoints.Clear(); 
        pointsState.Clear();
        goal.Clear();
        crossroads.Clear();
        for (int x = 0; x < with; x++) 
        { 
            for (int z = 0; z < lenght; z++)
            {
                var point = new Vector3(x - with / 2, 0, z - lenght / 2) + transform.position;
                gradePoints.Add(point);
                if (SetObstacles(point))
                {
                    pointsState.Add(PointType.Obstacle);
                }
                else
                {
                    pointsState.Add(PointType.Normal);
                }
                goal.Add(SetGoal(point));
                crossroads.Add(false);
            }
        }
    }
    
    
    
    public static Vector3 GetPoint(Vector3 hitPoint)
    {
        var minDist = Mathf.Infinity;
        var point = Vector3.zero;
        var full = false;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(hitPoint,gradePoints[i]);
            if (dist < minDist)
            {
                point = gradePoints[i];
                minDist = dist;
                full = pointsState[i]  == PointType.Obstacle;
            }
        }
        if (full) return Vector3.back;
        return point;
    }
    public static bool IsObstacle(Vector3 point)
    {
        for (int i = 0; i < gradePoints.Count; i++)
        {
            if (point == gradePoints[i]) return pointsState[i] == PointType.Obstacle;
        }
        return false;
    }
    public static bool IsGoal(Vector3 point)
    {
        var minDist = Mathf.Infinity;
        var full = false;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (dist < minDist)
            {
                minDist = dist;
                full = goal[i];
            }
        }
        return full;
    }
    public static bool IsFull(Vector3 point)
    {
        if (point == Vector3.back) return false;
        if (GetClosestPointIndex(point) > pointsState.Count) return false;
        return pointsState[GetClosestPointIndex(point)] == PointType.Full;
    }
    public static void PointFull(Vector3 point)
    {
        if (GetClosestPointIndex(point) > pointsState.Count) return;
        pointsState[GetClosestPointIndex(point)] = PointType.Full;
    }
    public static void RemovePoint(Vector3 point)
    {
        if (GetClosestPointIndex(point) > pointsState.Count) return;
        pointsState[GetClosestPointIndex(point)] = PointType.Normal;
    }

    public static void SetCrossroads(Vector3 point, bool active= true)
    {
        crossroads[GetClosestPointIndex(point)] = active;
    }
    
    public static Vector3 GetClosestRoadPoint(Vector3 point)
    {
        var minDist = 1f;
        var closestPoint = Vector3.back;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (pointsState[i] == PointType.Full && dist < minDist)
            {
                closestPoint = gradePoints[i];
                minDist = dist;
            }
        } 
        return closestPoint;
    }
    public static Vector3 GetClosestPoint(Vector3 point)
    {
        var minDist = 0.5f;
        var closestPoint = Vector3.back;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (dist < minDist)
            {
                closestPoint = gradePoints[i];
                minDist = dist;
            }
        } 
        return closestPoint;
    }

    public static bool IsCrossroads(Vector3 point)
    {
        var index = GetClosestPointIndex(point);
        if (index > crossroads.Count) return false;
        return crossroads[index];
    }



    private bool SetObstacles(Vector3 point)
    {
        Collider[] results = new Collider[5];
        var size = Physics.OverlapSphereNonAlloc(point, 0.3f, results, obstacleLayer);
        if (size > 0) return true;
        return false;
    }
    private bool SetGoal(Vector3 point)
    {
        Collider[] results = new Collider[5];
        var size = Physics.OverlapSphereNonAlloc(point, 0.3f, results, goalLayer);
        if (size > 0) return true;
        return false;
    }
    private static int GetClosestPointIndex(Vector3 point)
    {
        var minDist = 1f;
        var index = 100000;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (dist < minDist)
            {
                index = i;
                minDist = dist;
            }
        }
        return index;
    }
    
    
    
    

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        SetGradePoint();
        for (int i = 0; i < gradePoints.Count; i++)
        {
            switch (pointsState[i])
            {
                case PointType.Normal:
                    
                    Gizmos.color = Color.green;
                    break;
                case PointType.Obstacle:
                    Gizmos.color = Color.red;
                    break;
                case PointType.Full:
                    Gizmos.color = Color.yellow;
                    break;
            }
            if(goal[i]) Gizmos.color = Color.blue;
            if(crossroads[i]) Gizmos.color = Color.white;
            Gizmos.DrawSphere(gradePoints[i],0.3f);
        }
    }

    private void OnApplicationQuit()
    {
        SetGradePoint();
    }
}
