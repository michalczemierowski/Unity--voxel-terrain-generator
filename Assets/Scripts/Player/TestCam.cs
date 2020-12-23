/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
public class TestCam : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.W))
            speed += 15 * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            speed -= 15 * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 30 * Time.deltaTime, 0);
        else if (Input.GetKey(KeyCode.A))
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y - 30 * Time.deltaTime, 0);

        if (Input.GetKey(KeyCode.LeftShift))
            transform.Translate(Vector3.down * Time.deltaTime * 5);
        else if (Input.GetKey(KeyCode.Space))
            transform.Translate(Vector3.up * Time.deltaTime * 5);
    }
}

