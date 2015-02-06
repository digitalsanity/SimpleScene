﻿
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using SimpleScene;
using System.Collections;


namespace Example2DTileGame
{
    public class SSLineObject : SSObject
    {
        #region Variables
        Vector3 mapPosition;
        float squareWidth = 4;

        Vector3[,] mapArray = new Vector3[10, 10]; // W x D map (X & Z)

        bool isGenerating = true;
        int x = 0;

        float vHeight = 0;
        float currentHeight;

        ArrayList vectorList = new ArrayList();
        
        // Default values of square - should never actually set anything to anything
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(0, 0, 0);
        Vector3 p2 = new Vector3(0, 0, 0);
        Vector3 p3 = new Vector3(0, 0, 0);
        Vector3 middle = new Vector3(0, 0, 0);
        #endregion
        
        /// <summary>
        /// draw a 'wire - frame' of the map
        /// </summary>
        public void drawWireFrame()
        {
            GL.Begin(PrimitiveType.Lines);
            foreach (Vector3 v in vectorList)
            {
                // Draw each point added
                GL.Vertex3(v);
            }
            GL.End();
        }

        /// <summary>
        /// Render line object
        /// </summary>
        public override void Render(ref SSRenderConfig renderConfig)
        {
                
                base.Render(ref renderConfig);

                //!important!
                // mode setup
                SSShaderProgram.DeactivateAll(); // disable GLSL
                GL.Disable(EnableCap.Texture2D);                
                GL.Disable(EnableCap.Lighting);
                //!important!                
                
                GL.Color3(1f, 0.1f, 0.8f);
                drawWireFrame(); // Draw it               
        }

        /// <summary>
        /// Adds points into array-list
        /// </summary>
        public void addToArray(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 Middle)
        {
            vectorList.Add(p0); vectorList.Add(p1);
            vectorList.Add(p0); vectorList.Add(p2);
            vectorList.Add(p2); vectorList.Add(p3);
            vectorList.Add(p3); vectorList.Add(p1);
            vectorList.Add(p0); vectorList.Add(Middle);
            vectorList.Add(p1); vectorList.Add(Middle);
            vectorList.Add(p2); vectorList.Add(Middle);
            vectorList.Add(p3); vectorList.Add(Middle);
        }

        public SSLineObject (Vector3 mapPos) : base()
        {
            Random rand = new Random();
            Console.WriteLine("Set points");
            for (int i = 0; i < mapArray.GetLength(0); i++)
            {
                for (int j = 0; j < mapArray.GetLength(1); j++)
                {
                    float Middle = squareWidth / 2; // Middle point of the square
                    float squareCX = i * squareWidth;
                    float squareCY = j * squareWidth;

                    p0 = new Vector3(squareCX, vHeight, squareCY);
                    p1 = new Vector3(squareCX + squareWidth, vHeight, squareCY);
                    p2 = new Vector3(squareCX, vHeight, squareCY + squareWidth);
                    p3 = new Vector3(squareCX + squareWidth, vHeight, squareCY + squareWidth);

                    currentHeight = rand.Next(0, 10);
                    // Determines height
                    middle = new Vector3(squareCX + Middle, currentHeight, squareCY + Middle);

                    

                    addToArray(p0, p1, p2, p3, middle);

                }

            }
            
        }


    }

}
