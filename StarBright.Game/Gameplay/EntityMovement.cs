using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StarBright
{
    public class EntityMovement
    {
        int type;
        float timer;
        float duration; // 
        public Vector2 velocity;

        public byte[] romData;
        public EntityMovement(byte[] data)
        {
            romData = data;
            type = data[0];
            timer = 0f;
            duration = 0f;

            switch (type)
            {
                case 0:
                case 1:
                    
                    break;
            }
        }

        void Update(GameTime gameTime)
        {

            switch (type)
            {
                case 0:
                    break;

                case 1:
                    timer = (float)(timer + gameTime.ElapsedGameTime.TotalSeconds);
                    break;
            }
        }
    }
}