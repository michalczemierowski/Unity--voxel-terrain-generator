using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Chunks
{
    public class ChunkDissapearingAnimation : MonoBehaviour
    {
        public float speed = 32;
        public float distance = 32;

        private void Update()
        {
            // start moving chunk down
            transform.Translate(Vector3.down * speed * Time.deltaTime);
            
            if (transform.position.y <= -distance)
            {
                // disable mesh renderers
                foreach (var mr in GetComponentsInChildren<MeshRenderer>())
                {
                    mr.enabled = false;
                }

                // disable object
                gameObject.SetActive(false);
                this.enabled = false;
            }
        }
    }
}

