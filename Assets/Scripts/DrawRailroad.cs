using System.Collections.Generic;
using UnityEngine;

public class DrawRailroad : MonoBehaviour
{
    public static DrawRailroad Instance {get; protected set;}
    [SerializeField] private Railroad railroad;
    [SerializeField] private GameObject activeRailroad;
    [SerializeField] private LayerMask groundLayer;
    
    private Camera cam;
    private Vector3  lastPoint;
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Railroad> railroads = new List<Railroad>();
    private bool draw = true;
    


    private void Start()
    {
        Instance = this;
        lastPoint = GameManager.Instance.currentLevel.startPoint.position;
    }
    
    
    private void Update()
    {
        Active();
        if (GameManager.State != GameState.Draw) return;
        if (Input.GetMouseButton(0)) DrawRoad();
    }
    private void DrawRoad()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pathPoints.Count != 0)
            {
                draw = false;
                for (int i = 0; i < pathPoints.Count; i++) 
                { 
                    var dis = Vector3.Distance(HitPoint(), pathPoints[i]);
                    if (dis <= 1f)
                    { 
                        lastPoint = pathPoints[i];
                        draw = true;
                    }
                }
            }
            else
            {
                draw = false;
                var dis = Vector3.Distance(HitPoint(), lastPoint);
                if (dis <= 1.1f)
                {
                    draw = true;
                }
            }
        }
        if (!draw) return;
        if (lastPoint != HitPoint())
        {
            AddNewPoint(HitPoint());
        }
        activeRailroad.transform.position = lastPoint;
    }

    private void Active()
    {
        if (GameManager.State != GameState.Draw)
        {
            activeRailroad.SetActive(false);
            return;
        }
        if (Input.GetMouseButtonDown(0)) activeRailroad.SetActive(true);
        if (Input.GetMouseButtonUp(0)) activeRailroad.SetActive(false);
    }
    
    
    private void AddNewPoint(Vector3 point)
    {
        for (int i = 0; i < pathPoints.Count-1; i++)
        {
            if (pathPoints[i] == point)
            { 
                if (i == pathPoints.Count - 2) RemovePoint(i +1);
                return;
            }
        }
        if(pathPoints.Count > 0)
        {
            var dist = Vector3.Distance(lastPoint, point);
            if (dist > 1)
            {
                if(dist < 3)AddAutoPoints(lastPoint,point);
                return;
            }
        }
        SetRoad(point);
    }

    
    
    private void AddAutoPoints(Vector3 startPoint, Vector3 endPoint)
    {
        var myPoint = startPoint;
        for (int i = 0; i < 200; i++)
        {
            if (myPoint == endPoint) return;
            if(myPoint.x < endPoint.x) myPoint += Vector3.right;
            else if(myPoint.x > endPoint.x) myPoint += Vector3.left;
            else if(myPoint.z < endPoint.z) myPoint += Vector3.forward;
            else if(myPoint.z > endPoint.z) myPoint += Vector3.back;
            if (Grade.IsObstacle(myPoint)) return;
            SetRoad(myPoint);
        }
    }

    
    
    private void SetRoad(Vector3 point)
    {
        if (Grade.IsObstacle(point) || Grade.IsFull(point)) return;
        pathPoints.Add(point); 
        Grade.PointFull(point);
        GenerateRoad();
        for (int i = 0; i < pathPoints.Count; i++)
        {
            railroads[i].transform.position = pathPoints[i];
        }
        lastPoint = HitPoint();
    }
    private void GenerateRoad()
    {
        var r = Instantiate(railroad);
        r.transform.name = "railroad " + railroads.Count;
        r.transform.SetParent(transform);
        railroads.Add(r);
    }
    private void RemovePoint(int index)
    {
        Grade.RemovePoint(pathPoints[index]);
        pathPoints.Remove(pathPoints[index]);
        var railroad = railroads[index];
        railroads.Remove(railroads[index]);
        Destroy(railroad.gameObject);
    }
    private Vector3 HitPoint()
    {
        if(cam == null) cam = Camera.main;
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            return Grade.GetPoint(hit.point);
        }
        return Vector3.zero;
    }
    public void ClearRoad()
    {
        Grade.Instance.SetGradePoint();
        for (int i = 0; i < railroads.Count; i++)
        {
            Destroy(railroads[i].gameObject);
        }
        railroads.Clear();
        pathPoints.Clear();
        Start();
    }
}
