using Prism.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Terrain
{
    public static Vector3Int GetBlocPos(Vector3 pos)
    {
        Vector3Int blocPos = new Vector3Int(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
        );

        return blocPos;
    }

    public static Vector3Int GetBlocPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 pos = new Vector3(
            MoveWithinBlock(hit.point.x, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z, hit.normal.z, adjacent)
        );

        return GetBlocPos(pos);
    }

    public static bool SetBloc(RaycastHit hit, byte bloc, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlocPos(hit, adjacent);
        chunk.World.SetBloc(new Vector3(pos.x, pos.y, pos.z), bloc);

        return true;
    }

    public static byte GetBloc(RaycastHit hit, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return 255;

        Vector3Int pos = GetBlocPos(hit, adjacent);

        byte bloc = chunk.World.GetBloc(new Vector3(pos.x, pos.y, pos.z));

        return bloc;
    }

    static float MoveWithinBlock(float pos, float norm, bool adjacent = false)
    {
        if (pos - (int)pos == 0.5f || pos - (int)pos == -0.5f)
        {
            if (adjacent)
            {
                pos += (norm / 2);
            }
            else
            {
                pos -= (norm / 2);
            }
        }

        return (float)pos;
    }
}
