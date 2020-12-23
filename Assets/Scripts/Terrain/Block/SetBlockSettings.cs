/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    /// <summary>
    /// Struct containing settings for replacing blocks
    /// </summary>
    public struct SetBlockSettings
    {
        /// <summary>
        /// Should destroy event be called
        /// </summary>
        public bool callDestroyEvent;
        /// <summary>
        /// Should place event be called
        /// </summary>
        public bool callPlaceEvent;
        /// <summary>
        /// Should block drop be dropped
        /// </summary>
        public bool dropItemPickup;
        /// <summary>
        /// Velocity of dropped item (ignored if dropItemPickup == false)
        /// </summary>
        public float droppedItemVelocity;
        /// <summary>
        /// Should dropped item be rotated same as player camera (ignored if dropItemPickup == false)
        /// </summary>
        public bool rotateDroppedItem;

        public SetBlockSettings(bool callDestroyEvent, bool callPlaceEvent, bool dropItemPickup, float droppedItemVelocity = 0, bool rotateDroppedItem = false)
        {
            this.callDestroyEvent = callDestroyEvent;
            this.callPlaceEvent = callPlaceEvent;
            this.dropItemPickup = dropItemPickup;
            this.droppedItemVelocity = droppedItemVelocity;
            this.rotateDroppedItem = rotateDroppedItem;
        }

        /// <summary>
        /// Dont call any events and dont drop item
        /// </summary>
        public static readonly SetBlockSettings VANISH = new SetBlockSettings(false, false, false);
        /// <summary>
        /// Call only destroy event
        /// </summary>
        public static readonly SetBlockSettings DESTROY = new SetBlockSettings(true, false, false);
        /// <summary>
        /// Call only place event
        /// </summary>
        public static readonly SetBlockSettings PLACE = new SetBlockSettings(false, true, false);
        /// <summary>
        /// Call destroy event and drop item
        /// </summary>
        public static readonly SetBlockSettings MINE = new SetBlockSettings(true, false, true);
    }
}
