using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    Transform cam;
    World world;

    public Transform hb;
    public Transform pb;

    public Text BlockText;
    public byte selectedBlockIndex = 1;

    public bool isGrounded;
    public bool isSprinting;

    public float walkSpeed = 3f;
    public float gravity = -9.8f;

    public float sprintSpeed = 6f;
    public float jumpForce = 5f;

    public float playerWidth = 0.15f;

    float horizontal;
    float vertical;
    float mouseHorizontal;
    float mouseVertical;
    Vector3 velocity;
    float verticalMomentum = 0;
    bool jumpRequest;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    float rotY;
    float RotSpeed = 2f;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        Cursor.lockState = CursorLockMode.Locked;
        BlockText.text = "1 - " + world.blockdata[1].Name;
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (jumpRequest)
        {
            jump();
        }

        rotY += Input.GetAxis("Mouse Y") * RotSpeed;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        transform.Rotate(Vector3.up * (mouseHorizontal * 2));
        cam.rotation = Quaternion.Euler(-rotY, transform.eulerAngles.y, 0f);

        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        getPlayerInput();
        placeCursorBlock();
    }

    void jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    void CalculateVelocity()
    {
        // Gravity
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        // Sprinting
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // VerticalMomentum
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if(velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    void placeCursorBlock()
    {
        float step = checkIncrement;
        Vector3 LastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckRender(pos))
            {
                hb.position = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
                pb.position = new Vector3((int)LastPos.x, (int)LastPos.y, (int)LastPos.z);
                hb.gameObject.SetActive(true);
                pb.gameObject.SetActive(true);

                return;
            }
            LastPos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
            step += checkIncrement;
        }
        hb.gameObject.SetActive(false);
        pb.gameObject.SetActive(false);
    }

    void getPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll != 0)
        {
            if (scroll > 0)
                selectedBlockIndex--;
            else
                selectedBlockIndex++;
            if(selectedBlockIndex > world.blockdata.Length - 1)
            {
                selectedBlockIndex = 1;
            }
            if (selectedBlockIndex < 1)
            {
                selectedBlockIndex = (byte)(world.blockdata.Length - 1);
            }
            BlockText.text = selectedBlockIndex + " - " + world.blockdata[selectedBlockIndex].Name;
        }
        if (hb.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(1))
                world.ChunkFromVector3(hb.position).EditVoxel(hb.position, 0);
            if (Input.GetMouseButtonDown(0))
                world.ChunkFromVector3(pb.position).EditVoxel(pb.position, selectedBlockIndex);
        }
    }

    private float checkDownSpeed(float downspeed)
    {
        if (
            world.CheckSolid(transform.position + new Vector3(-playerWidth, downspeed, -playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(playerWidth, downspeed, -playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(playerWidth, downspeed, playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(-playerWidth, downspeed, playerWidth))
            )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downspeed + gravity / 1000;
        }
    }
    private float checkUpSpeed(float upspeed)
    {
        if (
            world.CheckSolid(transform.position + new Vector3(-playerWidth, 2f + upspeed, -playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(playerWidth, 2f + upspeed, -playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(playerWidth, 2f + upspeed, playerWidth)) ||
            world.CheckSolid(transform.position + new Vector3(-playerWidth, 2f + upspeed, playerWidth))
            )
        {
            return 0;
        }
        else
        {
            return upspeed;
        }
    }
    public bool front
    {
        get
        {
            if (
                world.CheckSolid(transform.position + new Vector3(0, 0, playerWidth)) ||
                world.CheckSolid(transform.position + new Vector3(0, 1, playerWidth))
                )
                return true;
            else
                return false;
        }
    }
    public bool back
    {
        get
        {
            if (
                world.CheckSolid(transform.position + new Vector3(0, 0, -playerWidth)) ||
                world.CheckSolid(transform.position + new Vector3(0, 1, playerWidth))
                )
                return true;
            else
                return false;
        }
    }
    public bool left
    {
        get
        {
            if (
                world.CheckSolid(transform.position + new Vector3(-playerWidth, 0, 0)) ||
                world.CheckSolid(transform.position + new Vector3(-playerWidth, 1, 0))
                )
                return true;
            else
                return false;
        }
    }
    public bool right
    {
        get
        {
            if (
                world.CheckSolid(transform.position + new Vector3(playerWidth, 0, 0)) ||
                world.CheckSolid(transform.position + new Vector3(playerWidth, 1, 0))
                )
                return true;
            else
                return false;
        }
    }
}
