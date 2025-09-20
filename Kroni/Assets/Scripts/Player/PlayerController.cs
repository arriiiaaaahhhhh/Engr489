using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //reference the transform
    Transform t;

    public static bool isSwimming;
    public static bool inWater;

    //if inwater and not swimming, then activate float on top of water code. 
    //if not in water, activate walk code
    //if swimming, activate swimming code

    [Header("Water")]
    public LayerMask waterMask;
    public string waterTriggerTag = "Water";   // <-- tag your WaterCollider with "Water"
    bool isTeleporting = false;                // suppress trigger flips during teleport

    Rigidbody rb;

    [Header("Player Rotation")]
    public float sensitivity = 1;

    //clamp variables
    public float rotationMin;
    public float rotationMax;

    //mouse input
    float rotationX;
    float rotationY;

    [Header("Player Movement")]
    public float speed = 1;
    float moveX;
    float moveY;
    float moveZ;

    // Inventory
    InventorySystem iSystem;

    // Respawn
    [Header("Respawn")]
    public Transform customSpawn;
    Vector3 _spawnPos;
    Quaternion _spawnRot;


    // Setup
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        t = transform;
        Cursor.lockState = CursorLockMode.Locked;

        inWater = false;
        rb.useGravity = true;

        iSystem = InventorySystem.Instance;

        // Store spawn point
        _spawnPos = (customSpawn != null) ? customSpawn.position : t.position;
        _spawnRot = (customSpawn != null) ? customSpawn.rotation : t.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIBaseClass.menuOpen)
        {
            LookAround();
        }

        // For debugging
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void FixedUpdate()
    {
        if (!UIBaseClass.menuOpen)
        {
            SwimmingOrFloating();
            Move();
        }
    }

    // Only react to the Water trigger
    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;
        if (((1 << other.gameObject.layer) & waterMask) != 0)  // layer-based
        {
            inWater = true;
            rb.useGravity = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isTeleporting) return;
        if (((1 << other.gameObject.layer) & waterMask) != 0)  // layer-based
        {
            inWater = false;
            rb.useGravity = true;
        }
    }

    void SwimmingOrFloating()
    {
        bool swimCheck = false;

        if (inWater)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(t.position.x, t.position.y + 0.5f, t.position.z), Vector3.down, out hit, Mathf.Infinity, waterMask))
            {
                if (hit.distance < 0.65)
                {
                    swimCheck = true;
                }
            }
            else
            {
                swimCheck = true;
            }
        }
        isSwimming = swimCheck;
        UnityEngine.Debug.Log(isSwimming);
    }

    void LookAround()
    {
        //get the mouse input
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;

        //clamp the values of y
        rotationY = Mathf.Clamp(rotationY, rotationMin, rotationMax);

        //setting the rotation value every update
        t.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);
    }

    void Move()
    {
        //get the movement input
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Forward");
        moveY = Input.GetAxis("Vertical");

        //check if the player is standing still
        if (inWater) //If in water, velocity = 0
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            if (moveX == 0 && moveZ == 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        // Land movement
        if (!inWater)
        {
            t.Translate(new Quaternion(0, t.rotation.y, 0, t.rotation.w) * new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed, Space.World);
        }
        else
        {
            // Water movement
            if (!isSwimming)
            {
                // Ensure player can't float above surface
                moveY = Mathf.Min(moveY, 0);

                // convert the local direction vector into a worldspace vector. 
                Vector3 clampedDirection = transform.TransformDirection(new Vector3(moveX, moveY, moveZ));
                // clamp the values
                clampedDirection = new Vector3(clampedDirection.x, Mathf.Min(clampedDirection.y, 0), clampedDirection.z);

                t.Translate(clampedDirection * Time.deltaTime * speed, Space.World);
            }
            else
            {
                // Underwater movement
                t.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed);
                t.Translate(new Vector3(0, moveY, 0) * Time.deltaTime * speed, Space.World);
            }
        }
    }

    // Respawn
    public void TeleportToSpawn()
    {
        UnityEngine.Debug.Log("[Respawn] TeleportToSpawn called.");

        isTeleporting = true;  // suppress water trigger flips this frame

        // stop motion
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // move to spawn pos
        t.SetPositionAndRotation(_spawnPos, _spawnRot);

        // ensure player's on land
        inWater = false;
        isSwimming = false;
        rb.useGravity = true;

        // small lift to avoid clipping
        t.position += Vector3.up * 0.25f;

        UnityEngine.Debug.Log($"[Respawn] New pos: {t.position}");

        // release the guard shortly after teleport so triggers resume normally
        StartCoroutine(ClearTeleportingFlag());
    }

    IEnumerator ClearTeleportingFlag()
    {
        // wait to stop re-trigger water bug
        yield return new WaitForFixedUpdate();
        isTeleporting = false;
    }
}
