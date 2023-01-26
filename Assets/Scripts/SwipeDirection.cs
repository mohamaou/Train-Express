using UnityEngine;

public enum Direction
{
    Forward, Back, Right, Left, Null
}
public class SwipeDirection
{
    
    private static Vector3 firstMousPoint, direction;
    
    public static Direction MovementDirection()
    {
        if (Input.GetMouseButtonDown(0)) firstMousPoint = Input.mousePosition;
        var dist = Vector3.Distance(Input.mousePosition, firstMousPoint);
        if (dist > Screen.height / 8f)
        {
            var x = (firstMousPoint.x - Input.mousePosition.x)/4;
            var y = (firstMousPoint.y - Input.mousePosition.y)/4;
            firstMousPoint = Input.mousePosition + new Vector3(x,y);
        }
        var dir = (firstMousPoint - Input.mousePosition).normalized;
        if (Input.GetMouseButtonUp(0))
        {
            var x = -dir.x;
            var y = -dir.y;
            if (y > 0.5f)
            {
                return Direction.Forward;
            }
            if (y < -0.5)
            {
                return Direction.Back;
            }

            if (x > 0.5f)
            {
                return Direction.Right;
            }

            if (x < -0.5f)
            {
                return Direction.Left;
            }
        }
        return  Direction.Null;
    }
}
