using System;
using UnityEngine;

public class Railroad : MonoBehaviour
{
    private enum RailroadType
    {
        Normal, Start, Final
    }
    [SerializeField] private RailroadType type;
    [SerializeField] private GameObject one, two, three, four;
    private bool  left, right, back, forward ;



    private void Update()
   {
       if (type == RailroadType.Final || type == RailroadType.Start || GameManager.State != GameState.Draw && GameManager.State != GameState.Play) return;
       GetNearPoints(); 
       SetDirection();
   }
   
   


   private void GetNearPoints()
   {
       var goal = GameManager.Instance.currentLevel.goal.position;
       var start = GameManager.Instance.currentLevel.startPoint.position;
       left = Grade.IsFull(transform.position + Vector3.left) || transform.position == goal || transform.position == start;
       right =  Grade.IsFull(transform.position + Vector3.right) || transform.position + Vector3.right == goal || transform.position + Vector3.right == start;
       forward =  Grade.IsFull(transform.position + Vector3.forward) || transform.position + Vector3.forward == goal || transform.position + Vector3.forward == start;
       back = Grade.IsFull(transform.position + Vector3.back) || transform.position + Vector3.back == goal || transform.position + Vector3.back == start;
   }

   private void SetDirection()
   {
       one.SetActive(false);
       two.SetActive(false);
       three.SetActive(false);
       four.SetActive(false);
       if (back && forward && right && left)
       {
           four.SetActive(true); 
           Grade.SetCrossroads(transform.position);
       }
       else if (forward && right && left || back && right && left || back && forward && left || back && forward && right)
       {
           three.SetActive(true);
           Grade.SetCrossroads(transform.position);
           if (forward && right && left) transform.eulerAngles = new Vector3(0, 270, 0);
           if (back && right && left) transform.eulerAngles = new Vector3(0, 90, 0);
           if (back && forward && left) transform.eulerAngles = new Vector3(0, 180, 0);
           if (back && forward && right)transform.eulerAngles = new Vector3(0, 0, 0);
       }
       else if(back && forward || left && right)
       {
           one.SetActive(true);
           Grade.SetCrossroads(transform.position, false);
           if (back && forward) transform.eulerAngles = new Vector3(0, 0, 0);
           if (left && right) transform.eulerAngles = new Vector3(0, 90, 0);
       } 
       else if (back && left || forward && right || left && forward || right && back)
       {
           two.SetActive(true);
           Grade.SetCrossroads(transform.position, false);
           if (back && left) transform.eulerAngles = new Vector3(0, -90, 0);
           if (back && right) transform.eulerAngles = new Vector3(0, 180, 0);
           if (forward && right) transform.eulerAngles = new Vector3(0, 90, 0);
           if (forward && left) transform.eulerAngles = new Vector3(0, 0, 0); }
       else
       {
           if (back || forward) transform.eulerAngles = new Vector3(0, 0, 0);
           if (left || right) transform.eulerAngles = new Vector3(0, 90, 0);
           one.SetActive(true);
       }
   }
}
