﻿using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SimpleScene
{
    public class SSInstancedParticleRenderer : SSObject
    {
        private static readonly SSVertex_PosTex1[] c_billboardVertices = {
            // CCW quad; no indexing
            new SSVertex_PosTex1(-.5f, -.5f, 0f, 0f, 0f),
            new SSVertex_PosTex1(+.5f, -.5f, 0f, 1f, 0f),
            new SSVertex_PosTex1(+.5f, +.5f, 0f, 1f, 1f),

            new SSVertex_PosTex1(-.5f, -.5f, 0f, 0f, 0f),
            new SSVertex_PosTex1(+.5f, +.5f, 0f, 1f, 1f),
            new SSVertex_PosTex1(-.5f, +.5f, 0f, 0f, 1f),
        };
        protected static readonly SSVertexBuffer<SSVertex_PosTex1> s_billboardVbo;

        protected SSParticleSystem m_ps;
        protected SSIndexBuffer m_ibo;
        protected SSAttributeBuffer<SSAttributePos> m_posBuffer;
        protected SSAttributeBuffer<SSAttributeColor> m_colorBuffer;
        protected SSTexture m_texture;

        static SSInstancedParticleRenderer()
        {
            s_billboardVbo = new SSVertexBuffer<SSVertex_PosTex1> (c_billboardVertices);
        }

        public SSInstancedParticleRenderer (SSParticleSystem ps, SSTexture texture)
        {
            m_ps = ps;
            m_texture = texture;
            m_posBuffer 
                = new SSAttributeBuffer<SSAttributePos> (BufferUsageHint.StreamDraw);
            m_colorBuffer
                = new SSAttributeBuffer<SSAttributeColor> (BufferUsageHint.StreamDraw);

            // test
            this.boundingSphere = new SSObjectSphere (10f);
        }

        public override void Render (ref SSRenderConfig renderConfig)
        {
            int numActive = m_ps.ActiveBlockLength;
            if (numActive <= 0) return;

            base.Render(ref renderConfig);

            // update buffers
            m_posBuffer.UpdateBufferData(m_ps.Positions);
            m_colorBuffer.UpdateBufferData(m_ps.Colors);

            // override matrix setup to get rid of any rotation in view
            // http://stackoverflow.com/questions/5467007/inverting-rotation-in-3d-to-make-an-object-always-face-the-camera/5487981#5487981

            Matrix4 modelView = this.worldMat * renderConfig.invCameraViewMat;
            modelView = OpenTKHelper.BillboardMatrix(ref modelView);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            GL.Enable (EnableCap.AlphaTest);
            GL.Enable (EnableCap.Blend);
            GL.AlphaFunc(AlphaFunction.Greater, 0.2f);
            GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);

            #if true
            // draw using the main shader
            // TODO: debug with bump mapped lighting mode
            renderConfig.MainShader.Activate();
            renderConfig.MainShader.UniAmbTexEnabled = true;
            renderConfig.MainShader.UniDiffTexEnabled = false;
            renderConfig.MainShader.UniSpecTexEnabled = false;
            renderConfig.MainShader.UniBumpTexEnabled = false;
            GL.ActiveTexture(TextureUnit.Texture2);
            #else
            // TODO: Try drawing without shader
            SSShaderProgram.DeactivateAll();
            GL.ActiveTexture(TextureUnit.Texture0);
            #endif
            if (m_texture != null) {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, m_texture.TextureID);
            } else {
                GL.Disable(EnableCap.Texture2D);
            }

            //s_billboardVbo.DrawArrays(PrimitiveType.Triangles);
            //return;

            // prepare attribute arrays for draw

            int posInstancesPerValue = m_ps.Positions.Length < numActive ? numActive : 1;
            int posAttrLoc = renderConfig.MainShader.AttrInstancePos;
            m_posBuffer.PrepareAttribute(posInstancesPerValue, posAttrLoc);

            int colorInstancesPerValue = m_ps.Colors.Length < numActive ? numActive : 1;
            int colorAttrLoc = renderConfig.MainShader.AttrInstanceColor;
            #if false
            m_colorBuffer.PrepareAttribute(colorInstancesPerValue, colorAttrLoc);
            #endif

            // do the draw
            renderConfig.MainShader.UniInstanceDrawEnabled = true;
            s_billboardVbo.DrawInstanced(PrimitiveType.Triangles, numActive);
            renderConfig.MainShader.UniInstanceDrawEnabled = false;

            // undo attribute state
            m_posBuffer.DisableAttribute(colorAttrLoc);
            m_colorBuffer.DisableAttribute(posAttrLoc);

            //this.boundingSphere.Render(ref renderConfig);
        }
    }
}

