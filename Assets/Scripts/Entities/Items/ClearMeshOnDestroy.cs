using UnityEngine;

/*
 * Micha³ Czemierowski
 * https://github.com/michalczemierowski
*/
[RequireComponent(typeof(MeshFilter))]
public class ClearMeshOnDestroy : MonoBehaviour
{
    public MeshFilter TargetMeshFilter { get; set; }

    private void OnDestroy()
    {
        if (TargetMeshFilter == null)
            return;

        TargetMeshFilter.mesh?.Clear();
    }
}
