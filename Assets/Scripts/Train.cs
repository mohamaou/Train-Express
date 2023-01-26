using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;




public class Mark
{
    private List<Vector3> localPosition = new List<Vector3>();
    private List<Quaternion> localRotation = new List<Quaternion>();
    private Transform train;
    private int resolution = 2;
    public List<Vector3> GetPosition => localPosition;
    

    public Mark(Transform train)
    { 
        this.train = train;
    }
    public void SetMark(Vector3 position, Quaternion rotation)
    {
        if (localPosition.Count > 1) 
        {
            if(localPosition[0] == position || Vector3.Distance(localPosition[0],position) < 1f / resolution) return; 
        } 
        var positionList = new List<Vector3>();
        var rotationList = new List<Quaternion>();
        positionList.AddRange(localPosition);
        rotationList.AddRange(localRotation);
        
        localPosition.Clear();
        localRotation.Clear();
        
        localPosition.Add(position);
        localRotation.Add(rotation);
        
        
        localPosition.AddRange(positionList);
        localRotation.AddRange(rotationList);
    }

    public void Clear()
    {
        localPosition.Clear();
        localRotation.Clear();
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        float offset = Vector3.Distance(train.position,localPosition[0]);

        for (int i = 1; i < localPosition.Count; i++)
        {
            offset += Vector3.Distance(localPosition[i - 1], localPosition[i]);
            if (offset >= distance)
            {
                var t = offset - distance;
                return Vector3.Lerp(localPosition[i], localPosition[i - 1], t * resolution);
            }
        }
        return localPosition[localPosition.Count - 1];
    }
    
    public Quaternion GetRotationAtDistance(float distance)
    {
        var offset = Vector3.Distance(train.position,localPosition[0]);

        for (int i = 1; i < localPosition.Count; i++)
        {
            offset += Vector3.Distance(localPosition[i - 1], localPosition[i]);
            if (offset >= distance)
            {
                var t = offset - distance;
                return Quaternion.Lerp(localRotation[i],localRotation[i-1],t * resolution);
            }
        }
        return localRotation[localPosition.Count -1];
    }
    
    public float PathDistance()
    {
        var distance = Vector3.Distance(train.position,localPosition[0]);

        for (int i = 1; i < localPosition.Count; i++)
        {
            distance += Vector3.Distance(localPosition[i - 1], localPosition[i]);
        }
        return distance;
    }

    public static string Combine(string dataPath, string sourceManifestPath)
    {
        throw new NotImplementedException();
    }
}


[Serializable]
public class Arrow
{
    [SerializeField] private Transform parent;
    [SerializeField] private Transform forward, back, right, left;
    
    public void ShowArrow(bool forwardDirection = true, bool backDirection  = true, bool rightDirection = true, bool leftDirection = true)
    {
        if(forwardDirection) forward.DOScale(2.0354f, 0.3f);
        if(backDirection) back.DOScale(2.0354f, 0.3f);
        if(rightDirection) right.DOScale(2.0354f, 0.3f);
        if(leftDirection) left.DOScale(2.0354f, 0.3f);
        parent.rotation = Quaternion.identity;
    }

    public void Hide()
    {
        forward.DOScale(0, 0.3f);
        right.DOScale(0, 0.3f);
        left.DOScale(0, 0.3f);
        back.DOScale(0, 0.3f);
    }
}

public class Train : MonoBehaviour
{ 
    public static Train Instance {get; protected set;}
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject wagon;
    [SerializeField] private LayerMask wagonLayer;
    [SerializeField] private GameObject explosion;
    [SerializeField] private Arrow arrow;
    
    private Vector3 starPoint;
    private Quaternion startRotation;
    private List<Transform> wagons = new List<Transform>();
    private Vector3 activePoint;
    private bool moveInRailroad, moveFinal , setDirection, inGoal;
    private Direction direction, startDirection;
    private Mark path;


    private void Awake()
    {
        Instance = this;
        path = new Mark(transform);
        arrow.Hide();
        explosion.SetActive(false);
    }

    private void Start()
    {
       starPoint =  transform.position = GameManager.Instance.currentLevel.startPoint.position;
       startRotation = transform.rotation = GameManager.Instance.currentLevel.startPoint.rotation;
    }

    public void MoveTrain()
    {
        moveInRailroad = true;
        activePoint = GetTargetPoint();
        startDirection = GetTrainDirection();
    }
    
    private void Update()
    {
        MovementInRailroad();
        MoveFinal();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("People"))
        {
            AddPeople(collision.transform.GetComponent<People>());
        }
    }

    
    #region Movement
    private void MovementInRailroad()
    {
        if (!moveInRailroad) return;
        WagonsMovement();
        if (setDirection)
        {
            GetDirection();
            return;
        }
        if (Vector3.Distance(transform.position, activePoint) < 0.1f)
        {
            activePoint = GetTargetPoint();
            if (IsCrossroads())
            {
                setDirection = true;
                return;
            }
        }
        CheckIfCrashes();
        transform.position = Vector3.MoveTowards(transform.position, activePoint, speed * Time.deltaTime);
        var dir = activePoint - transform.position;
        if (dir == Vector3.zero) dir = transform.forward;
        var rotation  = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation,rotation,speed * 2.5f * Time.deltaTime);
        path.SetMark(transform.position, transform.rotation);
        
    }
    private void MoveFinal()
    {
        if (!moveFinal) return;
        if (inGoal)
        {
            var direction = transform.position + GameManager.Instance.currentLevel.goal.forward;
            transform.position = Vector3.MoveTowards(transform.position,direction , speed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation,GameManager.Instance.currentLevel.goal.rotation,8*Time.deltaTime);
            path.SetMark(transform.position, transform.rotation);
        }
        else
        {
            var target = GameManager.Instance.currentLevel.goal.position - GameManager.Instance.currentLevel.goal.forward;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation,GameManager.Instance.currentLevel.goal.rotation,8*Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.1f) inGoal = true;
        }
       
        WagonsMovement();
    }
    private void WagonsMovement()
    {
        for (int i = 0; i < wagons.Count; i++)
        {
            var index = wagons.Count -  i - 1;
            float offset = index * 1.1f + 1.3f;
            wagons[i].position = Vector3.Lerp(wagons[i].position, path.GetPointAtDistance(offset), speed * 2 * Time.deltaTime);
            wagons[i].rotation = Quaternion.Lerp(wagons[i].rotation,path.GetRotationAtDistance(offset), speed * 2.5f * Time.deltaTime);
        }
    }
    private Vector3 GetTargetPoint()
    {
        var forward = Grade.GetClosestRoadPoint(transform.position + Vector3.forward);
        var right = Grade.GetClosestRoadPoint(transform.position +  Vector3.right);
        var left = Grade.GetClosestRoadPoint(transform.position +  Vector3.left);
        var back = Grade.GetClosestRoadPoint(transform.position +  Vector3.back);

        switch (direction)
        {
            case Direction.Forward:
                if (forward.z >= 0)
                {
                    direction = Direction.Forward; 
                    return forward;
                }
                if (left.z >= 0)
                {
                    direction = Direction.Left;
                    return left;
                }
                if (right.z >= 0)
                {
                    direction = Direction.Right;
                    return right;
                }
                break;
            case Direction.Left:
                if (left.z >= 0)
                {
                    direction = Direction.Left;
                    return left;
                }
                if (forward.z >= 0)
                {
                    direction = Direction.Forward; 
                    return forward;
                }
                if (back.z >= 0)
                {
                    direction = Direction.Back;
                    return back;
                }
                break;
            case Direction.Right : 
                if (right.z >= 0)
                {
                    direction = Direction.Right;
                    return right;
                }
                if (forward.z >= 0)
                {
                    direction = Direction.Forward; 
                    return forward;
                }
                if (back.z >= 0)
                {
                    direction = Direction.Back;
                    return back;
                }
                break;
            case Direction.Back : 
                if (back.z >= 0)
                {
                    direction = Direction.Back;
                    return back;
                }
                if (right.z >= 0)
                {
                    direction = Direction.Right;
                    return right;
                }
                if (left.z >= 0)
                {
                    direction = Direction.Left;
                    return left;
                }
                break;
        }
        Done();
        return transform.position;
    }
    
    #endregion
    
    #region Crossroads
    private bool IsCrossroads()
    {
        return Grade.IsCrossroads(Grade.GetClosestPoint(transform.position));
    }
    private void GetDirection()
    {
        direction = SwipeDirection.MovementDirection();
        if (Input.GetMouseButtonUp(0) && IsActiveDirection(direction))
        {
            setDirection = false;
            arrow.Hide();
        }
        else if(setDirection && !moveFinal)
        {
            var forward = IsActiveDirection(Direction.Forward, true);
            var right = IsActiveDirection(Direction.Right, true); 
            var left = IsActiveDirection(Direction.Left, true);
            var back = IsActiveDirection(Direction.Back, true);
            arrow.ShowArrow(forward, back, right, left);
        }
    }
    private bool IsActiveDirection(Direction worldDirection , bool isArrow = false)
    {
        var localDirection = ConvertToLocal(worldDirection);
        var position = transform.position;
        var forwardPoint = Grade.GetClosestPoint(position + transform.forward);
        var rightPoint = Grade.GetClosestPoint(position +  transform.right);
        var leftPoint = Grade.GetClosestPoint(position -  transform.right);

        var goalPoint = GameManager.Instance.currentLevel.goal.position;
        var rightGoal =  Vector3.Distance(position + transform.right, goalPoint) < 0.2f;
        var leftGoal = Vector3.Distance(position - transform.right, goalPoint) < 0.2f;
        var forwardGoal = Vector3.Distance(position + transform.forward, goalPoint) < 0.2f;
      


        var forward = Grade.IsFull(forwardPoint) && localDirection == Direction.Forward;
        var left = Grade.IsFull(leftPoint) && localDirection == Direction.Left;
        var right = Grade.IsFull(rightPoint) && localDirection == Direction.Right;
        var goal = localDirection == Direction.Left && leftGoal 
                   || localDirection == Direction.Right && rightGoal
                   || localDirection == Direction.Forward && forwardGoal;
     
        
        if (goal && !isArrow)
        {
            var peoples = FindObjectsOfType<People>();
            var win = true;
            for (int i = 0; i < peoples.Length; i++)
            {
                if (!peoples[i].IsInTrain()) win = false;
            }
            if (win)
            {
                activePoint = GetTargetPoint();
                Done(true);
            }
            else
            {
                return false;
            }
        }
        if (forward || left || right)
        {
            if(!isArrow)activePoint = GetTargetPoint();
        }
        return forward || left || right;
    }

    private Direction ConvertToLocal(Direction worldDirection)
    {
        switch (GetTrainDirection())
        {
            case Direction.Back:
                switch (worldDirection)
                {
                    case Direction.Forward:
                        return Direction.Back;
                    case Direction.Back:
                        return Direction.Forward;
                    case Direction.Left:
                        return Direction.Right;
                    case Direction.Right:
                        return Direction.Left;
                }
                break;
            case Direction.Right:
                switch (worldDirection)
                {
                    case Direction.Forward:
                        return Direction.Left;
                    case Direction.Back:
                        return Direction.Right;
                    case Direction.Left:
                        return Direction.Back;
                    case Direction.Right:
                        return Direction.Forward;
                }
                break;
            case Direction.Left:
                switch (worldDirection)
                {
                    case Direction.Forward:
                        return Direction.Right;
                    case Direction.Back:
                        return Direction.Left;
                    case Direction.Left:
                        return Direction.Forward;
                    case Direction.Right:
                        return Direction.Back;
                }
                break;
        }
        return worldDirection;
    }
    private Direction GetTrainDirection()
    {
        var localDirection = Direction.Forward;
        
        var x = transform.forward.x;
        var z = transform.forward.z;
        
        if (z > 0.5f)
        {
            localDirection = Direction.Forward;
        }
        if (z < -0.5)
        {
            localDirection = Direction.Back;
        }
        if (x > 0.5f)
        {
            localDirection = Direction.Right;
        }
        if (x < -0.5f)
        {
            localDirection = Direction.Left;
        }

        return localDirection;
    }
    

    #endregion

    #region Clash

    private void CheckIfCrashes()
    {
        if (GameManager.State != GameState.Play) return;
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up/2, transform.forward);
        if (Physics.Raycast(ray, out hit, 1, wagonLayer))
        {
            Clash();
        }
    }

    private void Clash()
    {
       explosion.SetActive(true);
       Done(false);
       CameraShake.Shake();
    }
    

    #endregion
    
    #region Event

    public void Back()
    {
        arrow.Hide();
        moveInRailroad = false;
        setDirection = false;
        activePoint = Vector3.zero;
        direction = startDirection;
        transform.position = starPoint;
        transform.rotation = startRotation;
        explosion.SetActive(false);
        path.Clear();
        var peoples = FindObjectsOfType<People>();
        for (int i = 0; i < peoples.Length; i++)
        {
            peoples[i].Back();
        }
        for (int i = 0; i < wagons.Count; i++)
        {
            Destroy(wagons[i].gameObject);
        }
        wagons.Clear();
    }
    private void AddPeople(People person)
    {
        person.tag = "Untagged";
        var newWagon = Instantiate(wagon, transform.position, Quaternion.identity).transform;
        wagons.Add(newWagon);
        person.GoToTrain(newWagon);
    }
    private void Done(bool inGoal = false)
    {
        arrow.Hide();
        moveInRailroad = false;
        if(Grade.IsGoal(transform.position) || inGoal)
        {
            var peoples = FindObjectsOfType<People>();
            var win = true;
            for (int i = 0; i < peoples.Length; i++)
            {
                if (!peoples[i].IsInTrain()) win = false;
            }
            moveFinal = win;
            if (win)
            {
                UiManager.Instance.HidePlayPanel();
                GameManager.Instance.Invoke(nameof(GameManager.Instance.Win),1);
            }
            else
            {
                UiManager.Instance.Wrong();
            }
        }
        else
        {
            UiManager.Instance.Wrong();
        }
    }

    #endregion
}
