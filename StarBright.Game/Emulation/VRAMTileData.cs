using System;
using Microsoft.Xna.Framework.Graphics;

namespace StarBright
{
    public class VRAMTileData
    {
        GraphicsDevice dev;
        public byte[] tileData;

        public VRAMTileData(GraphicsDevice _dev)
        {
            tileData = new byte[6144];
            dev = _dev;
        }

        public Texture2D convertToTexture()
        {
            Texture2D tex = new Texture2D(dev, 6144, 1, false, SurfaceFormat.Alpha8);
            tex.SetData<byte>(tileData);
            return tex;
        }
    }
}