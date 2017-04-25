#region Using Statments
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Project3
{
    class Cube
    {
        #region Fields
        static VertexPositionNormalTexture[] vertices;

        //Calculate the position of the vertices on the top face
        static Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, -1.0f);
        static Vector3 topLeftBack =  new Vector3(-1.0f, 1.0f, 1.0f);
        static Vector3 topRightFront =  new Vector3(1.0f, 1.0f, -1.0f);
        static Vector3 topRightBack =  new Vector3(1.0f, 1.0f, 1.0f);

        //Calculate the postion of the vertices on the bottom face
        static Vector3 btmLeftFront =  new Vector3(-1.0f, -1.0f, -1.0f);
        static Vector3 btmLeftBack =  new Vector3(-1.0f, -1.0f, 1.0f);
        static Vector3 btmRightFront =  new Vector3(1.0f, -1.0f, -1.0f);
        static Vector3 btmRightBack =  new Vector3(1.0f, -1.0f, 1.0f);

        //Normal vector for each face
        static Vector3 normalFront = new Vector3(0.0f, 0.0f, 1.0f);
        static Vector3 normalBack = new Vector3(0.0f, 0.0f, -1.0f);
        static Vector3 normalTop = new Vector3(0.0f, 1.0f, 0.0f);
        static Vector3 normalBottom = new Vector3(0.0f, -1.0f, 0.0f);
        static Vector3 normalLeft = new Vector3(-1.0f, 0.0f, 0.0f);
        static Vector3 normalRight = new Vector3(1.0f, 0.0f, 0.0f);

        //UV texture coordinates
        static Vector2 textureTopLeft = new Vector2(1.0f , 0.0f );
        static Vector2 textureTopRight = new Vector2(0.0f , 0.0f );
        static Vector2 textureBottomLeft = new Vector2(1.0f , 1.0f );
        static Vector2 textureBottomRight = new Vector2(0.0f , 1.0f );

        const ushort NUM_VERTICES = 36;
        #endregion

        #region Initialize 
        /// <summary>
        /// Constructor for sky cube
        /// </summary>
        public Cube()
        {
            vertices = new VertexPositionNormalTexture[NUM_VERTICES];

            //Add the vertices for the front face
            vertices[0] = new VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft);
            vertices[1] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);
            vertices[3] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);

            //Add the vertices for the back face
            vertices[6] = new VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight);
            vertices[7] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[8] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[9] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[10] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[11] = new VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft);

            // Add the vertices for the TOP face.
            vertices[12] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft);
            vertices[15] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[16] = new VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight);
            vertices[17] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);

            // Add the vertices for the BOTTOM face. 
            vertices[18] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[19] = new VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft);
            vertices[20] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[21] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[22] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[23] = new VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight);

            // Add the vertices for the LEFT face.
            vertices[24] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);
            vertices[25] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[26] = new VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight);
            vertices[27] = new VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft);
            vertices[28] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[29] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);

            // Add the vertices for the RIGHT face. 
            vertices[30] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[31] = new VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft);
            vertices[32] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
            vertices[33] = new VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight);
            vertices[34] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[35] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
        }

        #endregion

        /// <summary>
        /// Sets up the vertex buffer with the needed information and creates the cube
        /// </summary>
        /// <param name="device"> this is the graphics device</param>
        public void Render(GraphicsDevice device)
        {

            VertexBuffer buffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), NUM_VERTICES, BufferUsage.WriteOnly);

            buffer.SetData<VertexPositionNormalTexture>(vertices);

            device.SetVertexBuffer(buffer);

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
        }



        /// <summary>
        /// creates string for cube object
        /// </summary>
        /// <returns>the string representation of the cube object</returns>
        public override string ToString()
        {
            string message = "Vertices: \n\n";

            for (int i = 0; i < vertices.Length; i++)
                message += string.Format("Vertex[{0}]: {1}\n", i, vertices[i].ToString());

            message += "\n\n";

            return message;

        }
    }
}
