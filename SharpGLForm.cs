using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGL;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace PointRendering
{
    /// <summary>
    /// The main form class.
    /// </summary>
    public partial class SharpGLForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharpGLForm"/> class.
        /// </summary>
        public SharpGLForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RenderEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs e)
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            
            //gl.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
            gl.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);
            //  Load the identity matrix.
            gl.LoadIdentity();

            gl.LookAt(x, y, z, x + lx, 0.0f, z + lz, 0.0f, 1.0f, 0.0f);
            //  Rotate around the Y axis.
            gl.PushMatrix();
//            gl.Translate(0.0f, 0.0f, -1.25f);
//            gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);
            gl.Scale(1.0f / scale, 1.0f / scale, 1.0f / scale);
            gl.Translate(-center[0], -center[1], -center[2]);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBOId[0]);
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.VertexPointer(3, OpenGL.GL_FLOAT, 0, BUFFER_OFFSET_ZERO);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBOId[1]);
            gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);
            gl.ColorPointer(3, OpenGL.GL_FLOAT, 0, BUFFER_OFFSET_ZERO);
            
            gl.DrawArrays(OpenGL.GL_POINTS, 0, n_vertex);

            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
            gl.PopMatrix();
            
            //  Nudge the rotation.
            rotation += 10.0f;
        }

        /// <summary>
        /// Open a File to be loaded
        /// </summary>
        /// /// <param name="fileName">The name of the file to open.</param>
        private void loadFile(string fileName) 
        {
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            string line = file.ReadLine();
            float[] maxV = new float[3];
            maxV[0] = maxV[1] = maxV[2] = float.MinValue;
            float[] minV = new float[3];
            minV[0] = minV[1] = minV[2] = float.MaxValue;

            if(line != null)
            {
                //Debug.WriteLine(line);
                if (line.CompareTo("OFF") == 0) 
                {
                    line = file.ReadLine();
                    if (line != null)
                    {
                        string[] words = line.Split(' ');
                        int nvertex = Convert.ToInt32(words[0]);
                        int npolygons = Convert.ToInt32(words[1]);
                        v_vertex = new float[nvertex * 3];
                        v_color = new float[nvertex * 3];
                        int index = 0;
                       
                        for (int k = 0; k < nvertex; k++, index += 3)
                        {
                            line = file.ReadLine();
                            string[] values = line.Split(' ');
                            v_vertex[index + 0] = (float)Convert.ToDouble(values[0]);
                            v_vertex[index + 1] = (float)Convert.ToDouble(values[1]);
                            v_vertex[index + 2] = (float)Convert.ToDouble(values[2]);

                            if (v_vertex[index + 0] > maxV[0])
                                maxV[0] = v_vertex[index + 0];
                            if (v_vertex[index + 0] < minV[0])
                                minV[0] = v_vertex[index + 0];

                            if (v_vertex[index + 1] > maxV[1])
                                maxV[1] = v_vertex[index + 1];
                            if (v_vertex[index + 1] < minV[1])
                                minV[1] = v_vertex[index + 1];

                            if (v_vertex[index + 2] > maxV[2])
                                maxV[2] = v_vertex[index + 2];
                            if (v_vertex[index + 2] < minV[2])
                                minV[2] = v_vertex[index + 2];
                        }
                        center[0] = (maxV[0] + minV[0]) / 2.0f;
                        center[1] = (maxV[1] + minV[1]) / 2.0f;
                        center[2] = (maxV[2] + minV[2]) / 2.0f;
                        scale = Math.Max(Math.Max(Math.Abs(maxV[0] - minV[0]), Math.Abs(maxV[1] - minV[1])), Math.Abs(maxV[2] - minV[2]));
                    }
                }
            }
            file.Close();
            //just to assign some colors, min = red, max = blue
            float t;
            float[] red = new float[3];
            red[0] = 0.6f; red[1] = 0.4f; red[2] = 0.25f;
            float[] blue = new float[3];
            blue[0] = 0.05f; blue[1] = 0.1f;  blue[2] = 0.9f;
            for (int k = 0; k < v_color.Length; k += 3) 
            {
                t = (v_vertex[k + 1] - minV[1]) / (maxV[1] - minV[1]);
                v_color[k + 0] = (1.0f - t) * blue[0] + t * red[0];
                v_color[k + 1] = (1.0f - t) * blue[1] + t * red[1];
                v_color[k + 2] = (1.0f - t) * blue[2] + t * red[2];
            }
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            //  TODO: Initialise OpenGL here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //gl.Enable(OpenGL.GL_POINT_SMOOTH);

            //  Set the point size
            gl.PointSize(1);

            //load a file
            //loadFile("heart.off");
            //loadFile("happy.off");
            loadFile("mountains.off");

            n_vertex = v_vertex.Length / 3;

            //initialize buffers
            VBOId = new uint[2];
            gl.GenBuffers(2, VBOId);
            //vertex buffer
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBOId[0]);
            IntPtr ptr1 = GCHandle.Alloc(v_vertex, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * v_vertex.Length, ptr1, OpenGL.GL_STATIC_DRAW);
            //color buffer
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBOId[1]);
            IntPtr ptr2 = GCHandle.Alloc(v_color, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * v_color.Length, ptr2, OpenGL.GL_DYNAMIC_DRAW);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, EventArgs e)
        {
            //  TODO: Set the projection matrix here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //  Create a perspective transformation.
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            //gl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void SharpGLForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            gl.DeleteBuffers(2, VBOId);
        }

        /// <summary>
        /// The current rotation.
        /// </summary>
        private float rotation = 0.0f;
        private float [] v_vertex;
        private float[] v_color;
        private float [] center = new float[3];
        private float scale;
        private uint [] VBOId;
        private IntPtr BUFFER_OFFSET_ZERO = GCHandle.Alloc(null, GCHandleType.Pinned).AddrOfPinnedObject();
        private int n_vertex;
        //mouse
        float angle;
        float lx = 0.0f, lz = -1.0f;
        float x = 0.0f, y = 0.0f, z = +1.0f;
        float fraction = 0.02f;

        private void SharpGLForm_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void SharpGLForm_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void SharpGLForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void openGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                x += lx * fraction;
                z += lz * fraction;
            }
            else if (e.KeyCode == Keys.S)
            {
                x -= lx * fraction;
                z -= lz * fraction;
            }
            if (e.KeyCode == Keys.A)
            {
                angle -= 0.01f;
                lx = (float)Math.Sin(angle);
                lz = (float)-Math.Cos(angle);
            }
            else if (e.KeyCode == Keys.D)
            {
                angle += 0.01f;
                lx = (float)Math.Sin(angle);
                lz = (float)-Math.Cos(angle);
            }
            else if (e.KeyCode == Keys.Q) 
            {
                y += fraction;
            }
            else if (e.KeyCode == Keys.E)
            {
                y -= fraction;
            }
            Invalidate();
        }
    }
}
