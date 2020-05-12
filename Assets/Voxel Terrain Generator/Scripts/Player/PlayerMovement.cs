using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float defaultSpeed;
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float jumpHeight = 3f;
    public float gravity = -9.81f;
    public float waterSpeedMultipler = 0.5f;

    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;

    [Space(20)]
    [SerializeField] private GameObject waterImage;

    private Rigidbody m_Rigidbody;
    private BoxCollider m_BoxCollider;
    private Camera m_Camera;

    private bool isInWater;
    private bool isGrounded;
    private bool jump;

    private Vector2 moveVector;

    private TerrainChunk currentChunk;

    // above head; head; legs; below legs
    private BlockType[] nearbyBlocks = new BlockType[4];

    private Vector3Int _currentPosition;
    private Vector3Int currentPosition
    {
        get
        {
            return _currentPosition;
        }
        set
        {
            if (value != _currentPosition)
            {
                _currentPosition = value;
                OnPositionChange();
            }
        }
    }

    private void OnPositionChange()
    {
        int x = _currentPosition.x;
        int y = _currentPosition.y;
        int z = _currentPosition.z;

        if (!currentChunk || x - currentChunk.chunkPos.x > 16 || x - currentChunk.chunkPos.x < 1 || z - currentChunk.chunkPos.z > 16 || z - currentChunk.chunkPos.z < 1)
        {
            currentChunk = TerrainGenerator.GetChunk(x, z);
        }

        int bix = x - currentChunk.chunkPos.x;
        int biz = z - currentChunk.chunkPos.z;

        string debugText = string.Empty;
        debugText += $"x:{x}, y:{y}, z:{z}\n";

        for (int _y = 0; _y < 4; _y++)
        {
            nearbyBlocks[_y] = currentChunk.GetBlock(bix, y - _y + 1, biz);
            debugText += nearbyBlocks[_y].ToString() + "\n";
        }

        DebugUI.Instance?.SetDebugText(debugText);

        CheckWater();

    }

    private void CheckWater()
    {
        if (nearbyBlocks[1] == BlockType.WATER)
        {
            waterImage.SetActive(true);
        }
        else
        {
            waterImage.SetActive(false);
        }

        isInWater = nearbyBlocks[1] == BlockType.WATER || nearbyBlocks[2] == BlockType.WATER;

        if (isInWater)
            speed = defaultSpeed * waterSpeedMultipler;
        else
            speed = defaultSpeed;
    }

    private void Awake()
    {
        TerrainGenerator.player = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_BoxCollider = GetComponent<BoxCollider>();
        m_Camera = Camera.main;

        defaultSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveVector = new Vector2(horizontal * speed, vertical * speed);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        Vector3 npos = (transform.forward * moveVector.y) + (transform.right * moveVector.x);
        m_Rigidbody.velocity = new Vector3(npos.x, m_Rigidbody.velocity.y, npos.z);
        if (isInWater)
        {
            if(Input.GetKey(KeyCode.Space))
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 1, m_Rigidbody.velocity.z);//  .MovePosition(transform.position + (transform.up * speed * Time.deltaTime));
        }
        else if (jump)
        {
            m_Rigidbody.AddForce(new Vector3(0, Mathf.Sqrt(-2 * gravity * jumpHeight), 0), ForceMode.VelocityChange);
            jump = false;
        }

        // ceil x, round y, ceil z
        currentPosition = new Vector3Int(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.CeilToInt(transform.position.z));
    }
}
