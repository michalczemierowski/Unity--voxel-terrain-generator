/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private void Awake()
    {
        int temp = VoxelTG.Terrain.WorldSettings.chunkHeight;
        Debug.Log($"{temp} {VoxelTG.Terrain.WorldSettings.chunkWidth}");
    }

    private void Start()
    {
        SceneManager.LoadScene(1);
    }
}
