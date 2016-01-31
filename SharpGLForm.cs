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

            //  Load the identity matrix.
            gl.LoadIdentity();

            //  Rotate around the Y axis.
            
            gl.Translate(0.0f, 0.0f, -1.0f);
            gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);
            gl.Scale(0.08, 0.08, 0.08);
            gl.Translate(-center[0], -center[1], -center[2]);
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            if (v_vertex != null) 
            {
                IntPtr ptr1 = GCHandle.Alloc(v_vertex, GCHandleType.Pinned).AddrOfPinnedObject();
                gl.VertexPointer(3, OpenGL.GL_FLOAT, 0, ptr1);
                int size = v_vertex.Length / 3;
                gl.DrawArrays(OpenGL.GL_POINTS, 0, size);
            }
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);

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
            if(line != null)
            {
                //Debug.WriteLine(line);
                if (line.CompareTo("COFF") == 0) 
                {
                    line = file.ReadLine();
                    if (line != null)
                    {
                        string[] words = line.Split(' ');
                        int nvertex = Convert.ToInt32(words[0]);
                        int npolygons = Convert.ToInt32(words[1]);
                        v_vertex = new float[nvertex * 3];
                        int index = 0;
                        float[] maxV = new float[3];
                        maxV[0] = maxV[1] = maxV[2] = float.MinValue;
                        float[] minV = new float[3];
                        minV[0] = minV[1] = minV[2] = float.MaxValue;
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
                        scale = Math.Max(Math.Max(maxV[0] - minV[0], maxV[1] - minV[1]), maxV[2] - minV[2]);
                    }
                }
            }
            file.Close();
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

            //  Set the clear color.
            gl.ClearColor(0, 0, 0, 0);

            //load a file
            loadFile("heart.off");
            
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
            gl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        /// <summary>
        /// The current rotation.
        /// </summary>
        private float rotation = 0.0f;
        private float [] v_vertex;
        private float [] center = new float[3];
        private float scale;
    }
}
