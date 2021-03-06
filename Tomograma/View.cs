﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging; 
namespace Tomograma
{
    class View
    {
        Bitmap textureImage;
        int VBOtexture;
        public int min = 0, width = 2000;
        int line;
         
        public void SetupView(int width, int height)
        {
            
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            if (width > height)
                line = height;
            else
                line = width;
            GL.Viewport(0, 0, line, line);
            SetupLightning();
        }

        public int clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public Color TransferFunction(short value)
        {
            int max = min + width;
            int newVal = clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }

        public void DrawQuads(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);


            for(int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
                for(int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    short value;
                    value = Bin.array[x_coord + y_coord * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord + 1);

                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord + 1);

                    value = Bin.array[x_coord + 1 + y_coord * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            GL.End();
        }

        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);


            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            ErrorCode Er = GL.GetError();
            string str = Er.ToString();
        }


        public void generateTextureImage(int layerNumber)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for(int i=0; i< Bin.X; i++)
                for(int j=0; j<Bin.Y; j++)
                {
                    int PixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[PixelNumber]));
                }
        }


        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);


            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }

        public void SetupLightning()
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light1);
            GL.Enable(EnableCap.Light2);

            //Vector4 lightPosition1 = new Vector4(1.5f, -1.5f, 10.0f, 1.0f);
            //Vector4 lightPosition2 = new Vector4(0.75f, -0.75f, 10.0f, 1.0f);

            Vector4 lightPosition1 = new Vector4(1.5f, -1.5f, 1, 1);
            Vector4 lightPosition2 = new Vector4(0.75f, -0.75f, 1, 1);

            GL.Light(LightName.Light1, LightParameter.Position, lightPosition1);
            GL.Light(LightName.Light2, LightParameter.Position, lightPosition2);

            Vector4 lightDirection1 = new Vector4(0.75f, -0.75f, 0.8f, 1);
            GL.Light(LightName.Light1, LightParameter.SpotDirection, lightDirection1);

            Vector4 lightDirection2 = new Vector4(0.75f, -0.75f, 0.8f, 1);
            GL.Light(LightName.Light2, LightParameter.SpotDirection, lightDirection2);

            GL.Light(LightName.Light1, LightParameter.Diffuse, OpenTK.Graphics.Color4.Yellow);
            GL.Light(LightName.Light2, LightParameter.Diffuse, OpenTK.Graphics.Color4.Red);
        }

    }
}
