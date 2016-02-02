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
using System.Collections;
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

        private void drawModel(OpenGL gl) 
        {
            if(l_vboId != null)
            {
                gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);
                for(int k = 0; k < l_vboId.Count; k++)
                {
                    gl.PushMatrix();
                        //transformations
                        gl.Scale(1.0f / f_scale, 1.0f / f_scale, 1.0f / f_scale);
                        gl.Translate(-v_center.X, -v_center.Y, -v_center.Z);
                        //vertexes
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[k][0]);
                        gl.VertexPointer(3, OpenGL.GL_FLOAT, 0, BUFFER_OFFSET_ZERO);
                        //color
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[k][1]);
                        gl.ColorPointer(3, OpenGL.GL_FLOAT, 0, BUFFER_OFFSET_ZERO);
                        //draw l_sizes[k] points
                        gl.DrawArrays(OpenGL.GL_POINTS, 0, l_sizes[k]);
                    gl.PopMatrix();
                }
                gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
            }
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
            // using a camera to move into the scene
            gl.LookAt(x, y, z, x + lx, 0.0f, z + lz, 0.0f, 1.0f, 0.0f);
            //drawing the model
            gl.Rotate(-90, 1, 0, 0);
            gl.PushMatrix();
                drawModel(gl);
            gl.PopMatrix();
        }

        /// <summary>
        /// Open a File to be loaded
        /// </summary>
        /// /// <param name="fileName">The name of the file to open.</param>
        private void loadFile(string fileName)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            string line;
            if (l_vertex == null)
                l_vertex = new List<SharpGL.SceneGraph.Vertex>();
            if (l_color == null)
                l_color = new List<SharpGL.SceneGraph.Vertex>();
            while ((line = file.ReadLine()) != null) 
            {
                string[] words = line.Split(',');
                SharpGL.SceneGraph.Vertex vertex = new SharpGL.SceneGraph.Vertex();
                SharpGL.SceneGraph.Vertex color = new SharpGL.SceneGraph.Vertex();
                vertex.Set( float.Parse(words[0], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture));
                l_vertex.Add(vertex);
                if (vertex.X < minVertex.X)
                    minVertex.X = vertex.X;
                if (vertex.Y < minVertex.Y)
                    minVertex.Y = vertex.Y;
                if (vertex.Z < minVertex.Z)
                    minVertex.Z = vertex.Z;
                if (vertex.X > maxVertex.X)
                    maxVertex.X = vertex.X;
                if (vertex.Y > maxVertex.Y)
                    maxVertex.Y = vertex.Y;
                if (vertex.Z > maxVertex.Z)
                    maxVertex.Z = vertex.Z;
                color.Set(  float.Parse(words[4], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[5], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[6], System.Globalization.CultureInfo.InvariantCulture));
                color /= 255.0f;
                l_color.Add(color);
            }
            v_center = (maxVertex + minVertex) / 2.0f;
            SharpGL.SceneGraph.Vertex distance = (maxVertex - minVertex);
            f_scale = Math.Max(Math.Max(distance.X, distance.Y), distance.Z);
            file.Close();
        }

        private void changeColorScale()
        {
            SharpGL.SceneGraph.GLColor red = new SharpGL.SceneGraph.GLColor(0.9f, 0.05f, 0.05f, 1);
            SharpGL.SceneGraph.GLColor green = new SharpGL.SceneGraph.GLColor(0.05f, 0.95f, 0.05f, 1);
            float diff = maxVertex.Y - minVertex.Y;
            SharpGL.SceneGraph.Vertex colorTemp = new SharpGL.SceneGraph.Vertex();
            for (int k = 0; k < l_vertex.Count; k++)
            {
                float t = (l_vertex[k].Y - minVertex.Y) / diff;
                colorTemp.Set( (red.R * (1.0f - t)) + (green.R * t),
                                (red.G * (1.0f - t)) + (green.G * t),
                                (red.B * (1.0f - t)) + (green.B * t));
                l_color[k] = colorTemp;
            }
        }

        public void openInputDataDialog() 
        {
            //load a file
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (.txt)|*.txt";
            DialogResult userClickedOK = ofd.ShowDialog(openGLControl);
            if (userClickedOK == System.Windows.Forms.DialogResult.OK)
            {
                //check if application has elements loaded
                if (l_vboId.Count > 0)
                    reset();
                //from selected file
                loadFile(ofd.FileName);
                //to create a gradient color - comment to get original colors from file
                changeColorScale();
                //create the GPU buffers
                createInitialBuffers();
            }
        }

        private void testFunctiontoAddCopy() 
        {
            //to debug/test
            Random r = new Random();
            List<SharpGL.SceneGraph.Vertex> newVertex = new List<SharpGL.SceneGraph.Vertex>(l_vertex);
            List<SharpGL.SceneGraph.Vertex> newColor = new List<SharpGL.SceneGraph.Vertex>(l_color);
            for (int k = 0; k < newVertex.Count; k++)
            {
                //element = new SharpGL.SceneGraph.Vertex(element.X, element.Y + 20, element.Z);
                newVertex[k] = new SharpGL.SceneGraph.Vertex(newVertex[k].X + (float)(r.NextDouble() * 50.0), newVertex[k].Y + (float)(r.NextDouble() * 50.0), newVertex[k].Z);
                //newVertex[k] = new SharpGL.SceneGraph.Vertex(newVertex[k].X, -44722.71f, ewVertex[k].Z);
                newColor[k] = new SharpGL.SceneGraph.Vertex(1, 1, 1);
            }
            addPointsAndColor(newVertex, newColor);
            newVertex.Clear();
            newColor.Clear();
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            
            //to change smoothness of points
            //gl.Enable(OpenGL.GL_POINT_SMOOTH);

            //  Set the point size
            gl.PointSize(1);

            openInputDataDialog();
        }

        private void addPointsAndColor(List<SharpGL.SceneGraph.Vertex> lvertex, List<SharpGL.SceneGraph.Vertex> lcolor)
        {
            OpenGL gl = openGLControl.OpenGL;
            //append new points to the existing list (if exists)
            if (l_vertex == null)
                l_vertex = new List<SharpGL.SceneGraph.Vertex>();
            l_vertex.AddRange(lvertex);
            //append new colors to the existing list (if exists)
            if(l_color == null)
                l_color = new List<SharpGL.SceneGraph.Vertex>();
            l_color.AddRange(lcolor);
            //set new size of the new list (just to render, if it exists)
            if (l_sizes == null)
                l_sizes = new List<int>();
            l_sizes.Add(lvertex.Count);

            uint[] ids = new uint[2];
            int pos = l_vboId.Count;
            //add ids to the existing list
            if (l_vboId == null)
                l_vboId = new List<uint[]>();
            l_vboId.Add(ids);

            gl.GenBuffers(2, l_vboId[pos]);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[pos][0]);
            IntPtr ptr1 = GCHandle.Alloc(lvertex.ToArray().ToArray(), GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * lvertex.Count * 3, ptr1, OpenGL.GL_STATIC_DRAW);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[pos][1]);
            IntPtr ptr2 = GCHandle.Alloc(lcolor.ToArray().ToArray(), GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * lcolor.Count * 3, ptr2, OpenGL.GL_DYNAMIC_DRAW);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
        }

        /// <summary>
        /// Creation of vertex and color buffer from a List<Vertex>. This function reset all values once a new file is loaded
        /// </summary>
        private void createInitialBuffers() 
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            //create 
            if (l_sizes == null)
                l_sizes = new List<int>();  //create list to store points of each buffer's list
            l_sizes.Add(l_vertex.Count);

            uint [] ids = new uint[2];
            l_vboId.Add(ids);
            //create buffers
            gl.GenBuffers(2, l_vboId[0]);
            //vertex buffer
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[0][0]);
            IntPtr ptr1 = GCHandle.Alloc(l_vertex.ToArray().ToArray(), GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * l_vertex.Count * 3, ptr1, OpenGL.GL_STATIC_DRAW);
            //color buffer
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, l_vboId[0][1]);
            IntPtr ptr2 = GCHandle.Alloc(l_color.ToArray().ToArray(), GCHandleType.Pinned).AddrOfPinnedObject();
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(float) * l_color.Count * 3, ptr2, OpenGL.GL_DYNAMIC_DRAW);
            //unbind buffers
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

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void reset()
        {
            OpenGL gl = openGLControl.OpenGL;
            //delete ids
            if (l_vboId != null) 
            {
                foreach (var element in l_vboId)
                    gl.DeleteBuffers(2, element);
            }
            l_vboId.Clear();
            //delete sizes of list of points
            l_sizes.Clear();
            //clear list of vertex and color
            l_color.Clear();
            l_vertex.Clear();
            //reset to original values
            angle = 0.0f;
            lx = 0.0f; lz = -1.0f;
            x = 0.0f; y = 0.0f; z = +1.0f;
            fraction = 0.02f;
            maxVertex = new SharpGL.SceneGraph.Vertex(float.MinValue, float.MinValue, float.MinValue);
            minVertex = new SharpGL.SceneGraph.Vertex(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        /// <summary>
        /// Invoked when the form is closed
        /// </summary>
        private void SharpGLForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            reset();
        }

        #region variables
            private IntPtr BUFFER_OFFSET_ZERO = GCHandle.Alloc(null, GCHandleType.Pinned).AddrOfPinnedObject(); //const value
            //mouse variables
            protected float angle = 0;
            protected float lx = 0.0f, lz = -1.0f;
            protected float x = 0.0f, y = 0.0f, z = +1.0f;
            protected float fraction = 0.02f;   //this value is the amount of movement in the camera
            //data structures
            private SharpGL.SceneGraph.Vertex maxVertex = new SharpGL.SceneGraph.Vertex(float.MinValue, float.MinValue, float.MinValue);    //maximum value once loaded
            private SharpGL.SceneGraph.Vertex minVertex = new SharpGL.SceneGraph.Vertex(float.MaxValue, float.MaxValue, float.MaxValue);
            private List<SharpGL.SceneGraph.Vertex> l_vertex = null;    //store ALL vertexes added
            private List<SharpGL.SceneGraph.Vertex> l_color = null;     //store ALL colors added
            private List<int> l_sizes = null;   //store size of each buffer of points (a list of points)
            private List<uint[]> l_vboId = new List<uint[]>();  //list of index in buffers
            //public values
            public SharpGL.SceneGraph.Vertex v_center;
            public float f_scale;
        #endregion

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
            else if (e.KeyCode == Keys.O) 
            {
                openInputDataDialog();
            }
            else if (e.KeyCode == Keys.T) 
            {
                testFunctiontoAddCopy();
            }
            Invalidate();
        }
    }
}
