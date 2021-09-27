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
        [SerializeField] private float speed = 6f;
        public float Speed
        {
            get => speed;
            set
            {
                speed = value;
                playerController.CameraAnimator.SetFloat("movementSpeed", value);
            }
        }

        private float maxMovementSpeed;
        public float MaxMovementSpeed
        {
            get
            {
                float result = maxMovementSpeed;

                // apply multiplers
                if (PlayerController.InventorySystem.IsOverloaded)
                    result *= overloadedSpeedMultipler;
                if (IsInWater)
                    result *= waterSpeedMultipler;

                return result;
            }
        }

        [Tooltip("Speed multipler when player is overloaded (too many items in inventory)")]
        [SerializeField] private float overloadedSpeedMultipler = 0.75f;
        [Tooltip("Speed multipler when player is swimming")]
        [SerializeField] private float waterSpeedMultipler = 0.5f;

        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float gravity = -19.62f;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector3 groundDistance = new Vector3(0.4f, 0.4f, 0.4f);
        [SerializeField] private LayerMask groundLayer;

        public bool IsInWater { get; private set; }
        private bool isWalking;
        public bool IsWalking
        {
            get => isWalking;
            private set
            {
                if (value != isWalking)
                {
                    playerController.CameraAnimator.SetBool("isWalking", value);
                    isWalking = value;
                }
            }
        }

        private bool isGrounded;
        public bool IsGrounded
        {
            get => isGrounded;
            private set
            {
                if (value != isGrounded)
                {
                    playerController.CameraAnimator.SetBool("isGrounded", value);
                    isGrounded = value;
                }
            }
        }
        public bool IsJumping { get; private set; }
        public bool IsFlyingModeActive { get; private set; }
        private Vector2 moveVector;

        private Chunk currentChunk;
        private PlayerController playerController;
        private Rigidbody m_Rigidbody;
        private BoxCollider m_BoxCollider;

        // above head; head; legs; below legs
        private BlockType[] nearbyBlocks = new BlockType[4];

        private Vector3Int _currentPosition;
        public Vector3Int CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            private set
            {
                if (value != _currentPosition)
                {
                    _currentPosition = value;
                    OnPositionChange();
                }
            }
        }

        /// <summary>
        /// Called whenever player's rounded position changes (Vector3Int currentPosition)
        /// </summary>
        private void OnPositionChange()
        {
            int x = _currentPosition.x;
            int y = _currentPosition.y;
            int z = _currentPosition.z;

            if (!currentChunk ||
                x - currentChunk.ChunkPosition.x > WorldSettings.ChunkSizeXZ ||
                x - currentChunk.ChunkPosition.x < 1 ||
                z - currentChunk.ChunkPosition.y > WorldSettings.ChunkSizeXZ ||
                z - currentChunk.ChunkPosition.y < 1)
            {
                currentChunk = World.GetChunk(x, z);
            }

            if(currentChunk == null)
                return;

            int bix = x - currentChunk.ChunkPosition.x;
            int biz = z - currentChunk.ChunkPosition.y;

            string positionString = $"[ x:{x}, y:{y}, z:{z} ]";

            for (int _y = 0; _y < 4; _y++)
            {
                nearbyBlocks[_y] = currentChunk.GetBlock(bix, y - _y + 1, biz);
                //debugText += nearbyBlocks[_y].ToString() + "\n";
            }

            DebugManager.SetPositionText(positionString);
            DebugManager.UpdateBiomeInfoText();

            CheckIfInWater();
        }

        /// <summary>
        /// Check if player is in water
        /// </summary>
        private void CheckIfInWater()
        {
            bool headInWater = nearbyBlocks[1] == BlockType.WATER;
            UIManager.InWaterOverlay.SetActive(headInWater);

            IsInWater = nearbyBlocks[1] == BlockType.WATER || nearbyBlocks[2] == BlockType.WATER;
        }

        private void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_BoxCollider = GetComponent<BoxCollider>();
            playerController = GetComponent<PlayerController>();

            maxMovementSpeed = speed;

            CurrentPosition = new Vector3Int(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.CeilToInt(transform.position.z));
        }

        private void Update()
        {
            if (!UIManager.IsUIModeActive)
                HandleInput();
            else
            {
                IsWalking = false;
                moveVector = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (IsFlyingModeActive)
                HandleFlying();
            else
            {
                HandleWalking();
                float y = m_Rigidbody.velocity.y + (gravity * Time.fixedDeltaTime);
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, y, m_Rigidbody.velocity.z);
            }

            // ceil x, round y, ceil z
            CurrentPosition = new Vector3Int(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.CeilToInt(transform.position.z));
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            moveVector = new Vector2(horizontal, vertical).normalized;

            // check if player is walking
            IsWalking = moveVector.sqrMagnitude != 0;

            // handle acceleration/deceleration
            bool isAccelerating = moveVector.sqrMagnitude > 0;
            float targetSpeed = isAccelerating ? MaxMovementSpeed : 0;
            float accelerationSpeed = Time.deltaTime * (targetSpeed > Speed ? acceleration : deceleration);

            Speed = Mathf.MoveTowards(Speed, targetSpeed, accelerationSpeed);
            moveVector *= Speed;
        }

        /// <summary>
        /// Handle default movement
        /// </summary>
        private void HandleWalking()
        {
            // check if player is grounded
            IsGrounded = Physics.CheckBox(groundCheck.position, groundDistance, Quaternion.identity, groundLayer);

            Vector3 npos = (transform.forward * moveVector.y) + (transform.right * moveVector.x);
            m_Rigidbody.velocity = new Vector3(npos.x, m_Rigidbody.velocity.y, npos.z);

            // FIXME: rework swimming
            if (IsInWater)
            {
                if (Input.GetKey(KeyCode.Space))
                    m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 1, m_Rigidbody.velocity.z);//  .MovePosition(transform.position + (transform.up * speed * Time.deltaTime));
            }
            else if (IsJumping)
            {
                m_Rigidbody.AddForce(new Vector3(0, Mathf.Sqrt(-2 * gravity * jumpHeight), 0), ForceMode.VelocityChange);
                IsJumping = false;
            }
        }

        /// <summary>
        /// Handle movement when flying is enabled
        /// </summary>
        private void HandleFlying()
        {
            Vector3 npos = (transform.position + (transform.forward * moveVector.y * Time.deltaTime + transform.right * moveVector.x * Time.deltaTime));
            if (Input.GetKey(KeyCode.Space))
                npos += Vector3.up * maxMovementSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.LeftShift))
                npos += Vector3.down * maxMovementSpeed * Time.deltaTime;

            transform.position = npos;
        }

        public void ToggleFlying()
        {
            IsFlyingModeActive = !IsFlyingModeActive;

            m_Rigidbody.isKinematic = IsFlyingModeActive;
            m_BoxCollider.enabled = !IsFlyingModeActive;
        }

        /// <summary>
        /// Try to jump (possible only when player is grounded)
        /// </summary>
        public void TryToJump()
        {
            if (IsGrounded)
                IsJumping = true;
        }
    }
}
