using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bloc 3 Textures", menuName = "Blocs/3 Textures Bloc")]
public class BlocGrass : Bloc
{
    public BlocGrass(BlocGrass bloc) : base(bloc)
    {
        UpFaceTextureTilePosition = bloc.UpFaceTextureTilePosition;
        DownFaceTextureTilePosition = bloc.DownFaceTextureTilePosition;
        TextureTilePosition = bloc.TextureTilePosition;
    }

    public Vector2Int UpFaceTextureTilePosition;
    public Vector2Int DownFaceTextureTilePosition;

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        switch (direction)
        {
            case Direction.U:
                //tile.x = 2;
                //tile.y = 0;
                tile.x = UpFaceTextureTilePosition.x;
                tile.y = UpFaceTextureTilePosition.y;
                return tile;
            case Direction.D:
                //tile.x = 1;
                //tile.y = 0;
                tile.x = DownFaceTextureTilePosition.x;
                tile.y = DownFaceTextureTilePosition.y;
                return tile;
            default:
                tile.x = TextureTilePosition.x;
                tile.y = TextureTilePosition.y;
                //tile.x = 3;
                //tile.y = 0;
                return tile;
        }
    }
}
