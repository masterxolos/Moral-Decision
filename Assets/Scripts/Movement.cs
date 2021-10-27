using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using DG.Tweening;
using Tabtale.TTPlugins;
using TMPro;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public PlayerState playerState;

    [SerializeField] private GameObject youChooseCanvas;

    [SerializeField] private Transform camTransform;

    public float rotSpeed = 30f;

    [SerializeField] private int point = 50;
    
    public Slider slider;
    public Image fill;

    [SerializeField] private TextMeshProUGUI textBox;
    private void Awake()
    {

        // Initialize CLIK

        TTPCore.Setup();

        // Your code here

    }
    public enum PlayerState
    {
        Stop,
        Move
    }
    //Touch Settings
    [SerializeField] bool isTouching;
    float touchPosX;
    Vector3 direction;
    [SerializeField, ReadOnly] private float movementSpeed;
    [SerializeField, ReadOnly] private float controlSpeed = 20;

    [SerializeField] private Transform carTransform;
  
    void Start()
    {
        playerState = PlayerState.Move;
        FillSlider();
    }

    void Update()
    {
        GetInput();
    }

    private void OnMouseUp()
    {
        carTransform.DORotate(new Vector3(0, 0, 0), 0.2f);
    }

    private void FixedUpdate()
    {
        if (playerState == PlayerState.Move)
        {
            transform.position += Vector3.forward * movementSpeed * Time.fixedDeltaTime;
            

        }
        if (isTouching)
        {
            touchPosX += Input.GetAxis("Mouse X") * controlSpeed * Time.fixedDeltaTime;
            OnMouseDrag();

        }

        transform.position = new Vector3(touchPosX, transform.position.y, transform.position.z);
        
    }

    void GetInput()
    {
        if (Input.GetMouseButton(0))
        {
            isTouching = true;
        }
        else
        {
            isTouching = false;
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("slowDown"))
        {
            movementSpeed /= 1.7f;
            //camTransform.DOLocalMove(new Vector3(0f, -2.71f, -1.64f), 1);
            //camTransform.DOLocalRotate(new Vector3(12.2f, 0, 0), 1);
        }
        if (other.gameObject.CompareTag("speedUp"))
        {
            movementSpeed *= 1.7f;
            //camTransform.DOLocalMove(new Vector3(0.2f, -2.2f, -68f), 1);
            //camTransform.DOLocalRotate(new Vector3(22.42f, 0, 0), 1);
        }

        if (other.gameObject.CompareTag("knife"))
        {
            point -= 5;
            FillSlider();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("flower"))
        {
            point += 5;
            FillSlider();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("CanvasAc"))
        {
            youChooseCanvas.SetActive(true);
            playerState = PlayerState.Stop;
        }
        // pointi + yada - yapıp fillslider();
    }

   
   

    void OnMouseDrag()
    {
        
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

        transform.Rotate(Vector3.up, +rotX);
        //transform.Rotate(Vector3.right, +rotY);
    }
    
    public void FillSlider()
    {
        Debug.Log(point);
        slider.value = point;
        fill.fillAmount = slider.value;
        if (point >= 100)
        {
            textBox.text = "Angel";
        }
        else if (point >= 90)
        {
            textBox.text = "Hearted";
        }
        else if (point >= 80)
        {
            textBox.text = "Good";
        }
        else if (point >= 70)
        {
            textBox.text = "Beneficent";
        }
        else if (point >= 60)
        {
            textBox.text = "Fair";
        }
        else if (point <= 50)
        {
            textBox.text = "Natural";
        }
       
        
        
        
        
        
    }
}