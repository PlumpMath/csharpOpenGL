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

        /// <summary>
        /// Function to draw the model
        /// </summary>
        private void drawModel(OpenGL gl) 
        {
            if(l_vboId != null)
            {
                gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);
                // itering over each list of points
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
            gl.ClearColor(0.10f, 0.10f, 0.10f, 1.0f);
            //  Load the identity matrix.
            gl.LoadIdentity();
            // using a camera to move into the scene
            //gl.LookAt(v_position.X, v_position.Y, v_position.Z, v_position.X + v_lookat.X, v_position.Y + v_lookat.Y, v_position.Z + v_lookat.Z, 0.0f, 1.0f, 0.0f);
            gl.LookAt(cameraPos.X, cameraPos.Y, cameraPos.Z, (cameraPos + cameraFront).X, (cameraPos + cameraFront).Y, (cameraPos + cameraFront).Z, cameraUp.X, cameraUp.Y, cameraUp.Z);
            
            //drawing the model
            gl.PushMatrix();
                gl.Rotate(-90, 1, 0, 0);
                drawModel(gl);
            gl.PopMatrix();
        }

        /// <summary>
        /// Open a File to be loaded
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        private void loadFile(string fileName)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            string line;
            if (l_vertex == null)
                l_vertex = new List<SharpGL.SceneGraph.Vertex>();
            if (l_color == null)
                l_color = new List<SharpGL.SceneGraph.Vertex>();
            // read each line
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
                //ignoring the words[3]
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

        /// <summary>
        /// Allow change the color of the List lcolor according the height of the lvertex
        /// </summary>
        /// <param name="lvertex"> List of vertex to read the height according the min Vertex</param>
        /// <param name="lcolor"> List of color to change according its height</param>
        private void changeColorScale(ref List<SharpGL.SceneGraph.Vertex> lvertex, ref List<SharpGL.SceneGraph.Vertex> lcolor)
        {
            //top color
            SharpGL.SceneGraph.GLColor red = new SharpGL.SceneGraph.GLColor(0.9f, 0.05f, 0.05f, 1);
            //bottom color
            SharpGL.SceneGraph.GLColor green = new SharpGL.SceneGraph.GLColor(0.05f, 0.95f, 0.05f, 1);
            float diff = maxVertex.Z - minVertex.Z;

            SharpGL.SceneGraph.Vertex colorTemp = new SharpGL.SceneGraph.Vertex();
            for (int k = 0; k < lcolor.Count; k++)
            {
                float t = (l_vertex[k].Z - minVertex.Z) / diff;
                //interpolated value
                colorTemp.Set( (red.R * (1.0f - t)) + (green.R * t),
                                (red.G * (1.0f - t)) + (green.G * t),
                                (red.B * (1.0f - t)) + (green.B * t));
                lcolor[k] = colorTemp;
            }
        }

        /// <summary>
        /// Open a File Dialog to load a new file
        /// </summary>
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
                changeColorScale(ref l_vertex, ref l_color);
                //create the GPU buffers
                createInitialBuffers();
            }
        }

        /// <summary>
        /// Just a debugging function to demostrate how to add new points over the existing list
        /// </summary>
        private void testFunctiontoAddCopy() 
        {
            //to debug/test
            Random r = new Random();
            List<SharpGL.SceneGraph.Vertex> newVertex = new List<SharpGL.SceneGraph.Vertex>(l_vertex);
            List<SharpGL.SceneGraph.Vertex> newColor = new List<SharpGL.SceneGraph.Vertex>(l_color);
            for (int k = 0; k < newVertex.Count; k++)
            {
                newVertex[k] = new SharpGL.SceneGraph.Vertex(newVertex[k].X + (float)(r.NextDouble() * 50.0), newVertex[k].Y + (float)(r.NextDouble() * 50.0), newVertex[k].Z);
                newColor[k] = new SharpGL.SceneGraph.Vertex(1, 1, 1);
            }
            // lists must exist
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

        /// <summary>
        /// Function invoked when points and colors must be added to the render.
        /// The size of lvertex and lcolor MUST BE THE SAME. Otherwise, there will be black points (no visible)
        /// </summary>
        /// <param name="lvertex"> List of vertexes to be added</param>
        /// <param name="lcolor"> List of colors to be added</param>
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
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //  Create a perspective transformation.
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);
            lastX = Width / 2.0f;
            lastY = Height / 2.0f;
            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        /// <summary>
        /// Clean initial data 
        /// </summary>
        private void reset()
        {
            OpenGL gl = openGLControl.OpenGL;
            //delete ids
            if (l_vboId != null) 
            {
                foreach (var element in l_vboId)
                    gl.DeleteBuffers(2, element);
            }
            if(l_vboId != null) l_vboId.Clear();
            //delete sizes of list of points
            if(l_sizes != null) l_sizes.Clear();
            //clear list of vertex and color
            if(l_color != null) l_color.Clear();
            if(l_vertex != null) l_vertex.Clear();
            //reset to original values the camera
            //angle = 0.0f;
            //lx = 0.0f; lz = -1.0f;
            //x = 0.0f; y = 0.0f; z = +1.0f;

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
            protected const float SPEED_MOVE = 0.015f;   //this value is the amount of movement in the camera, its fixed
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
            //mouse variables
            private Point posStart;     //first click
            private bool bisLeftDrag = false;
            private bool bisRightDrag = false;
            //camera variables
            protected SharpGL.SceneGraph.Vertex cameraPos = new SharpGL.SceneGraph.Vertex(0,0,0);
            protected SharpGL.SceneGraph.Vertex cameraUp = new SharpGL.SceneGraph.Vertex(0,1,0);
            protected SharpGL.SceneGraph.Vertex cameraFront = new SharpGL.SceneGraph.Vertex(0, 0, -1);
            protected float yaw = -90.0f;	// Yaw is initialized to -90.0 degrees since a yaw of 0.0 results in a direction vector pointing to the right
            protected float pitch = 0.0f;
            protected float lastX;
            protected float lastY;
            protected bool firstMouse = true; //to be set the 1st time on click
        #endregion


        #region mouseControls
            /// <summary>
            /// Simple cross product between vectors
            /// <param name="a">first vector</param>
            /// <param name="b">second vector</param>
            /// <returns> a resulting vector of a x b</returns>
            /// </summary>
            SharpGL.SceneGraph.Vertex cross(SharpGL.SceneGraph.Vertex a, SharpGL.SceneGraph.Vertex b) 
            {
                return new SharpGL.SceneGraph.Vertex(a.Y*b.Y - a.Z*b.Y,a.Z*b.X - a.X*b.Z, a.X*b.Y - a.Y*b.X);
            }

            /// <summary>
            /// Function to calculate the radian value given a degree
            /// </summary>
            /// <param name="angle"> in degree (0,360)</param>
            /// <returns> a radian value (0, 2PI)</returns>
            private float DegreeToRadian(double angle)
            {
                return (float)(angle * (Math.PI / 180.0));
            }

            /// <summary>
            /// When a key is pressed
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void openGLControl_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.O) 
                {
                    openInputDataDialog();  //open the dialog
                }
                else if (e.KeyCode == Keys.T) 
                {
                    testFunctiontoAddCopy();    //function to test. ITS A DEBUGGING FUNCTION, can be delete it
                }
                Invalidate();   //to invoke the draw function
            }
            
            /// <summary>
            /// When a mouse click is pressed (right or left)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void openGLControl_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    bisLeftDrag = true;
                    posStart = new Point(e.X, e.Y);
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right) 
                {
                    bisRightDrag = true;
                    posStart = new Point(e.X, e.Y);
                }
            }
            
            /// <summary>
            /// When the mouse wheel is touched/invoked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void openGLControl_MouseWheel(object sender, MouseEventArgs e)
            {
                if (e.Delta > 0)    //mouse wheel is from back to front
                    cameraPos += cameraFront * SPEED_MOVE;
                else 
                    cameraPos -= cameraFront * SPEED_MOVE;
            }

            /// <summary>
            /// When the mouse is moving over the screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void openGLControl_MouseMove(object sender, MouseEventArgs e)
            {
                if (bisLeftDrag)    //left button clicked
                {
                    if (firstMouse)
                    {
                        lastX = e.X;
                        lastY = e.Y;
                        firstMouse = false;
                    }
                    float xoffset = e.X - lastX;
                    float yoffset = lastY - e.Y; // Reversed since y-coordinates go from bottom to left
                    lastX = e.X;
                    lastY = e.Y;
                    xoffset *= SPEED_MOVE;  //sensivity
                    yoffset *= SPEED_MOVE;  //sensivity
                    yaw += xoffset;
                    pitch += yoffset;
                    // Make sure that when pitch is out of bounds, screen doesn't get flipped
                    if (pitch > 89.0f)
                        pitch = 89.0f;
                    if (pitch < -89.0f)
                        pitch = -89.0f;
                    SharpGL.SceneGraph.Vertex front = new SharpGL.SceneGraph.Vertex();
                    front.X = (float)(Math.Cos(DegreeToRadian(yaw)) * Math.Cos(DegreeToRadian(pitch)));
                    front.Y = (float)Math.Sin(DegreeToRadian(pitch));
                    front.Z = (float)(Math.Sin(DegreeToRadian(yaw)) * Math.Cos(DegreeToRadian(pitch)));
                    front.Normalize();
                    cameraFront = front;
                }
                else if (bisRightDrag) //right button
                {
                    if (firstMouse)
                    {
                        lastX = e.X;
                        lastY = e.Y;
                        firstMouse = false;
                    }
                    float xoffset = e.X - lastX;
                    float yoffset = lastY - e.Y; // Reversed since y-coordinates go from bottom to left
                    lastX = e.X;
                    lastY = e.Y;
                    if (xoffset > 0)    //to the right
                    {
                        SharpGL.SceneGraph.Vertex v = cross(cameraFront, cameraUp);
                        v.Normalize();
                        cameraPos += v * SPEED_MOVE;
                    }
                    else if (xoffset < 0)   //to the left
                    {
                        SharpGL.SceneGraph.Vertex v = cross(cameraFront, cameraUp);
                        v.Normalize();
                        cameraPos -= v * SPEED_MOVE;
                    }
                    if (yoffset > 0)    //to go up
                    {
                        cameraPos.Y += SPEED_MOVE / 6.0f; //to adjust the sensivity
                    }
                    else if (yoffset < 0)   //to go down
                    {
                        cameraPos.Y -= SPEED_MOVE / 6.0f; //to adjust the sensivity
                    }
                }
            }
            
            /// <summary>
            /// When the mouse is released from the click
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void openGLControl_MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    bisLeftDrag = false;
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    bisRightDrag = false;
                }
                firstMouse = true;
            }
         #endregion
    }
}
