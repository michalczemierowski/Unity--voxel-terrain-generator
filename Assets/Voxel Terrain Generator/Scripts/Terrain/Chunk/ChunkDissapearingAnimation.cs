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
            transform.Translate(Vector3.down * speed * Time.deltaTime);

            if (transform.position.y <= -distance)
            {
                foreach (var mr in GetComponentsInChildren<MeshRenderer>())
                {
                    mr.enabled = false;
                }

                gameObject.SetActive(false);
                this.enabled = false;
            }
        }
    }
}

