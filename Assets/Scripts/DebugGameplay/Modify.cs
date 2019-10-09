using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Modify : MonoBehaviour
{
    Vector2 rot;

    private void Update()
    {
        var world = GameObject.FindObjectOfType<World>();

        if (world == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100))
            {
                Terrain.SetBloc(hit, world.BlocsDefinition[0].id);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (world == null)
                return;

            for (int x = -2; x < 2; x++)
                for (int y = -1; y < 1; y++)
                    for (int z = -1; z < 1; z++)
                    {
                        world.CreateChunk(x * 16, y * 16, z * 16);
                    }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100))
            {
                if (world == null)
                    return;

                for (int x = -2; x < 2; x++)
                    for (int y = -1; y < 1; y++)
                        for (int z = -1; z < 1; z++)
                        {
                            world.Destroy(x * 16, y * 16, z * 16);
                        }
            }
        }

        rot = new Vector2(
            rot.x + Input.GetAxis("Mouse X") * 3,
            rot.y + Input.GetAxis("Mouse Y") * 3);

        transform.localRotation = Quaternion.AngleAxis(rot.x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rot.y, Vector3.left);

        transform.position += transform.forward * 3 * Input.GetAxis("Vertical");
        transform.position += transform.right * 3 * Input.GetAxis("Horizontal");
    }
}