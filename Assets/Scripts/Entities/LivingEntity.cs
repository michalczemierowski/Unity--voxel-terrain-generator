using System.Collections.Generic;
using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities
{
    /// <summary>
    /// Base class for living entities
    /// </summary>
    public class LivingEntity : Entity
    {
        [Header("Settings")]
        public float Speed;
        public Transform Target;

        /// <summary>
        /// last position of follow target
        /// </summary>
        private Vector3Int lastTargetPosition;
        /// <summary>
        /// entity destination
        /// </summary>
        private Vector3 targetMoveSpot;
        private float pathNodeTravelTime;

        /// <summary>
        /// Queue containing path
        /// </summary>
        private Queue<Vector3> pathQueue = new Queue<Vector3>();

        protected override void OnSpawn()
        {
            base.OnSpawn();

            targetMoveSpot = transform.position;
        }

        /// <summary>
        /// Handle finding target logics
        /// </summary>
        protected virtual void FindTarget() { }

        /// <summary>
        /// Handle movement when there's no target
        /// </summary>
        protected virtual void HandleMovement() { }

        /// <summary>
        /// Handle movement when following target
        /// </summary>
        protected virtual void HandleMovementTarget()
        {
            // get target pos
            Vector3Int targetPosition = Utils.WorldToBlockPosition(Target.position - Vector3.up);
            targetPosition.y = World.GetTopBlockPosition(new Vector2Int(targetPosition.x, targetPosition.z)).y + 1;

            if (targetPosition != lastTargetPosition)
            {
                lastTargetPosition = targetPosition;

                Vector3Int entityPosition = Utils.WorldToBlockPosition(transform.position);

                // try to find path
                Vector3[] path = PathFinding.FindPath(entityPosition, targetPosition);
                if (path == null || path.Length == 0)
                    return;

                pathQueue = new Queue<Vector3>();
                targetMoveSpot = transform.position;

                // enqueue path nodes
                for (int i = 0; i < path.Length; i++)
                {
                    pathQueue.Enqueue(path[i]);
                }
            }

            // walk to target if path is not null
            if (pathQueue != null)
            {
                if (Vector3.Distance(transform.position, targetMoveSpot) > 0.02f)
                {
                    float x = Mathf.MoveTowards(transform.position.x, targetMoveSpot.x, Speed * Time.fixedDeltaTime);
                    float y = Mathf.MoveTowards(transform.position.y, targetMoveSpot.y, Speed * Time.fixedDeltaTime * 2);
                    float z = Mathf.MoveTowards(transform.position.z, targetMoveSpot.z, Speed * Time.fixedDeltaTime);
                    transform.position = new Vector3(x, y, z);

                    pathNodeTravelTime += Time.fixedDeltaTime;
                }
                else if (pathQueue.Count > 0)
                {
                    targetMoveSpot = pathQueue.Dequeue();
                    pathNodeTravelTime = 0;
                }

                // TODO: remove this
                // if (pathNodeTravelTime > 2)
                // {
                //     Debug.Log("STUCK");
                //     transform.position = targetMoveSpot;
                // }
            }
        }

        private void FixedUpdate()
        {
            if (Target)
                HandleMovementTarget();
            else
                HandleMovement();
        }
    }
}