using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Chunks
{
    public class ChunkAnimation : MonoBehaviour
    {
        public float distance;
        public float speed = 32;

        private void OnEnable()
        {
            // offset chunk position
            transform.position = new Vector3(transform.position.x, -distance, transform.position.z);
            
            // enable mesh renderers
            foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            {
                mr.enabled = true;
            }
        }

        private void Update()
        {
            // move chunk to its default position
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            
            if (transform.position.y >= 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                this.enabled = false;
            }
        }
    }
}
