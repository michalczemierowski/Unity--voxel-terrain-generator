using UnityEngine;
using VoxelTG.Player;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities
{
    public class EntityManager : MonoBehaviour
    {
        [SerializeField] private GameObject testEntity;
        public void SpawnEntity(EntityType entityType, Vector3 worldPosition)
        {
            Entity entity = Instantiate(testEntity, worldPosition, Quaternion.identity).GetComponent<Entity>();
            if (entity is LivingEntity livingEntity)
            {
                livingEntity.Target = PlayerController.Instance.transform;
            }
        }
    }
}