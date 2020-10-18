using UnityEngine;
using VoxelTG.DebugUtils;
using VoxelTG.Terrain;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovement : MonoBehaviour
    {
        public bool fly;

        private float defaultSpeed;
        public float movementSpeed = 6.0f;
        public float acceleration = 10f;
        public float decleration = 20f;
        public float jumpSpeed = 8.0f;
        public float jumpHeight = 3f;
        public float gravity = -9.81f;
        public float waterSpeedMultipler = 0.5f;

        private float _speed;
        private float speed{
            get => _speed;
            set{
                _speed = value;
                playerController.cameraAnimator.SetFloat("movementSpeed", value);
            }
        }

        public Transform groundCheck;
        public Vector3 groundDistance = new Vector3(0.4f, 0.4f, 0.4f);
        public LayerMask groundMask;

        [Space(20)]
        [SerializeField] private GameObject waterImage;

        private Rigidbody m_Rigidbody;
        private BoxCollider m_BoxCollider;

        private bool isInWater;
        private bool _isWalking;
        public bool isWalking
        {
            get => _isWalking;
            set
            {
                if (value != _isWalking)
                {
                    playerController.cameraAnimator.SetBool("isWalking", value);
                    _isWalking = value;
                }
            }
        }

        private bool _isGrounded;
        private bool isGrounded
        {
            get => _isGrounded;
            set
            {
                if(value != _isGrounded)
                {
                    playerController.cameraAnimator.SetBool("isGrounded", value);
                    _isGrounded = value;
                }
            }
        }
        private bool jump;

        private Vector2 moveVector;

        private Chunk currentChunk;
        private PlayerController playerController;

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

            if (!currentChunk ||
                x - currentChunk.chunkPos.x > WorldSettings.chunkWidth ||
                x - currentChunk.chunkPos.x < 1 ||
                z - currentChunk.chunkPos.y > WorldSettings.chunkWidth ||
                z - currentChunk.chunkPos.y < 1)
            {
                currentChunk = World.GetChunk(x, z);
            }

            int bix = x - currentChunk.chunkPos.x;
            int biz = z - currentChunk.chunkPos.y;

            string debugText = string.Empty;
            debugText += $"[ x:{x}, y:{y}, z:{z} ]";

            for (int _y = 0; _y < 4; _y++)
            {
                nearbyBlocks[_y] = currentChunk.GetBlock(bix, y - _y + 1, biz);
                //debugText += nearbyBlocks[_y].ToString() + "\n";
            }

            DebugConsole.SetPositionText(debugText);

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
                movementSpeed = defaultSpeed * waterSpeedMultipler;
            else
                movementSpeed = defaultSpeed;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_BoxCollider = GetComponent<BoxCollider>();
            playerController = GetComponent<PlayerController>();

            defaultSpeed = movementSpeed;

            currentPosition = new Vector3Int(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.CeilToInt(transform.position.z));
        }

        void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            if (fly)
                HandleFlying();
            else
            {
                HandleWalking();
                float y = m_Rigidbody.velocity.y + (gravity * Time.fixedDeltaTime);
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, y, m_Rigidbody.velocity.z);
            }


        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                SwitchFlying();

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal == 0 && vertical == 0)
            {
                isWalking = false;
            }
            else
                isWalking = true;

            moveVector = new Vector2(horizontal, vertical).normalized;

            float acc = moveVector.magnitude > 0 ? acceleration : decleration;
            speed = Mathf.MoveTowards(speed, moveVector.magnitude * movementSpeed, acc * Time.deltaTime);
            moveVector *= speed;

            isGrounded = Physics.CheckBox(groundCheck.position, groundDistance, Quaternion.identity, groundMask);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                jump = true;
        }

        private void HandleWalking()
        {
            Vector3 npos = (transform.forward * moveVector.y) + (transform.right * moveVector.x);

            m_Rigidbody.velocity = new Vector3(npos.x, m_Rigidbody.velocity.y, npos.z);
            if (isInWater)
            {
                if (Input.GetKey(KeyCode.Space))
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

        private void HandleFlying()
        {
            Vector3 npos = (transform.position + (transform.forward * moveVector.y * Time.deltaTime + transform.right * moveVector.x * Time.deltaTime));
            if (Input.GetKey(KeyCode.Space))
                npos += Vector3.up * movementSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.LeftShift))
                npos += Vector3.down * movementSpeed * Time.deltaTime;

            transform.position = npos;
        }

        private void SwitchFlying()
        {
            if (fly)
            {
                m_Rigidbody.isKinematic = false;
                m_BoxCollider.enabled = true;
            }
            else
            {
                m_Rigidbody.isKinematic = true;
                m_BoxCollider.enabled = false;
            }
            fly = !fly;
        }
    }
}
