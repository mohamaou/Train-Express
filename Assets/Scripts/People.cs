using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class People : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject[] characters;
    
    private Transform targetWagon;
    private Vector3 position;
    private bool inTrain;
    private float jumpDistance;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Transform startParent;


    private void Start()
    {
        startParent = transform.parent;
        startPosition = transform.position;
        startRotation = transform.rotation;
        Invoke(nameof(StartAnimation),Random.Range(0,2f));
    }

    private void StartAnimation()
    {
        anim.SetBool("Jump", true);
    }
    
    private void Update()
    {
        if (inTrain)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation,Quaternion.identity, 6*Time.deltaTime);
            transform.localPosition =
                Vector3.Lerp(transform.localPosition, new Vector3(0, 0.2f, 0), 6 * Time.deltaTime);
            return;
        }
        Jumping();
    }
    
    public void RandomizedCharacters()
    {
        var random = Random.Range(0, characters.Length);
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == random);
        }
    }

    public void GoToTrain(Transform target)
    {
        targetWagon = target;
    }
    
    public bool IsInTrain()
    {
        return inTrain;
    }
    
    private void Jumping()
    {
        
        if (targetWagon == null ) return;
        position = Vector3.Lerp(position, targetWagon.position, 6 * Time.deltaTime);
        if (jumpDistance == 0f)
        {
            position = transform.position;
            jumpDistance = Vector3.Distance(position, targetWagon.position);
        }
        var dist = Vector3.Distance(position, targetWagon.position);
        var offset = dist / jumpDistance;
        var y = Mathf.Sin(offset * Mathf.PI) * jumpHeight;
        transform.position = position + Vector3.up * y;
        if (dist < 0.3f)
        {
            inTrain = true;
            anim.SetBool("Jump", false);
            transform.SetParent(targetWagon);
            GameManager.Instance.CheckIfTakeAllPeople();
        }
    }

    public void Back()
    {
        targetWagon = null;
        inTrain = false;
        Invoke(nameof(StartAnimation),Random.Range(0,2f));
        transform.tag = "People";
        transform.position = startPosition;
        transform.SetParent(startParent);
        transform.rotation = startRotation;
    }
}
