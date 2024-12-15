using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : NetworkBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float curSpeedX, curSpeedY;
    public bool isUnderWater = false;
    public bool isSittingMission = false;
    public GameObject Character;
    [SerializeField] Canvas canvas;
    private Animator animator;

    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    public string vertical = "Vertical";
    public string horizontal = "Horizontal";
    string mousey = "Mouse Y";
    string mousex = "Mouse X";
    private string isDrinking = "isDrinking";
    private string isFloating = "isFloating";
    private string isLaying = "isLaying";
    private string isRapping = "isRapping";
    private string isSitting = "isSitting";
    private string isTalking1 = "isTalking1";
    private string isTalking2 = "isTalking2";
    private string isThreading = "isThreading";
    private string isWalking = "isWalking";
    private string metalObject = "MetalObject";
    public List<Transform> targetPoints;
    private Dictionary<GameObject, Transform> targetPointsMap = new Dictionary<GameObject, Transform>();

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canvas.gameObject.SetActive(false);

        animator = Character.GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!canvas.gameObject.active && !isPresent())
        {
            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            // Press Left Shift to run
            bool isRunning = false;//Input.GetKey(KeyCode.LeftShift);
            curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis(vertical) : 0;
            curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis(horizontal) : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            moveDirection.y = movementDirectionY;

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);

            // Player and Camera rotation
            if (canMove)
            {
                rotationX += -Input.GetAxis(mousey) * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis(mousex) * lookSpeed, 0);
            }
        }
        animation();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(true);
                gameObject.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
                gameObject.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
                // Lock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                canvas.gameObject.SetActive(false);
                gameObject.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
                gameObject.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
                // Lock cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void animation()
    {
        if (!isPresent())
        {
            if (curSpeedX != 0 || curSpeedY != 0)
            {
                animator.SetBool(isWalking, true);
            }
            else
            {
                animator.SetBool(isWalking, false);
            }
        }
        else
        {
            if (transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(vertical) != 0 || transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(horizontal) != 0)
            {
                animator.SetBool(isWalking, true);
            }
            else
            {
                animator.SetBool(isWalking, false);
            }
        }
        

        //if (curSpeedX == 0 && curSpeedY == 0)
        //{
        //    if (Input.GetKeyDown(KeyCode.Mouse0))
        //    {
        //        animator.SetBool(isDrinking, true);
        //    }
        //    else
        //    {
        //        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //            animator.SetBool(isDrinking, false);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        animator.SetBool(isRapping, true);
        //    }
        //    else
        //    {
        //        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //            animator.SetBool(isRapping, false);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        animator.SetBool(isSitting, true);
        //    }
        //    else
        //    {
        //        if (curSpeedX != 0 || curSpeedY != 0)
        //            animator.SetBool(isSitting, false);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha3))
        //    {
        //        animator.SetBool(isTalking1, true);
        //    }
        //    else
        //    {
        //        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //            animator.SetBool(isTalking1, false);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha4))
        //    {
        //        animator.SetBool(isTalking2, true);
        //    }
        //    else
        //    {
        //        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //            animator.SetBool(isTalking2, false);
        //    }
        //}
        //else
        //{
        //    animator.SetBool(isDrinking, false);
        //    animator.SetBool(isRapping, false);
        //    animator.SetBool(isSitting, false);
        //    animator.SetBool(isTalking1, false);
        //    animator.SetBool(isTalking2, false);
        //}
    }

    public void StartAttractingObjects(float magnetRange, float attractionSpeed)
    {
        StartAttractingObjectsServerRpc(magnetRange, attractionSpeed);
    }

    [ServerRpc (RequireOwnership = false)]
    private void StartAttractingObjectsServerRpc(float magnetRange, float attractionSpeed)
    {
        StartCoroutine(AttractObjects(magnetRange, attractionSpeed));
    }

    private IEnumerator AttractObjects(float magnetRange, float attractionSpeed)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, magnetRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(metalObject))
            {
                if (!targetPointsMap.ContainsKey(hitCollider.gameObject))
                {
                    // Choose a target point and store it
                    Transform randomTargetPoint = targetPoints[UnityEngine.Random.Range(0, targetPoints.Count)];
                    targetPointsMap[hitCollider.gameObject] = randomTargetPoint;
                }

                Transform targetPoint = targetPointsMap[hitCollider.gameObject];

                if (Vector3.Distance(hitCollider.transform.position, transform.position) < magnetRange)
                {
                    hitCollider.transform.position = Vector3.MoveTowards(hitCollider.transform.position, targetPoint.position, attractionSpeed * Time.deltaTime);
                    hitCollider.GetComponent<Rigidbody>().useGravity = false;
                    //hitCollider.GetComponent<BoxCollider>().isTrigger = true;
                    if (!hitCollider.GetComponent<AudioSource>().isPlaying)
                        hitCollider.GetComponent<AudioSource>().Play();
                }
                else
                {
                    hitCollider.GetComponent<Rigidbody>().useGravity = true;
                    //hitCollider.GetComponent<BoxCollider>().isTrigger = false;
                    if (hitCollider.GetComponent<AudioSource>().isPlaying)
                        hitCollider.GetComponent<AudioSource>().Pause();
                }

                if (hitCollider.transform.position == targetPoint.position)
                {
                    hitCollider.transform.SetParent(transform);
                    hitCollider.GetComponent<Rigidbody>().isKinematic = true;
                    if (hitCollider.GetComponent<AudioSource>().isPlaying)
                        hitCollider.GetComponent<AudioSource>().Stop();
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    public static bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }
}