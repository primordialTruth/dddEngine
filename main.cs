//MS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
//others
using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
//mine


namespace dddEngine
{
    
    public class Test : GameWindow
    {
        float angle;
        struct VertexBufferObj { public int vboID, eboID, numElems;}
        struct VertexPositionColor { public Vector3 position; public uint color;
            public VertexPositionColor(float x, float y, float z, Color c)
            {
                this.position = new Vector3(x,y,z);
                this.color = toRgba(c);
            }
            static uint toRgba(Color color)
            {
                return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
            }
        };

        VertexBufferObj[] vbo = new VertexBufferObj[2];

#region hardcoded declarations
        VertexPositionColor[] CubeVerts = new VertexPositionColor[]{
            new VertexPositionColor(-1.0f, -1.0f, 1.0f, Color.DarkBlue),
            new VertexPositionColor(1.0f, -1.0f, 1.0f, Color.DarkRed),
            new VertexPositionColor(1.0f,  1.0f, 1.0f, Color.DarkGreen),
            new VertexPositionColor(-1.0f,  1.0f, 1.0f, Color.DarkGray),
            new VertexPositionColor(-1.0f, -1.0f, -1.0f, Color.DarkOrange),
            new VertexPositionColor(1.0f, -1.0f, -1.0f, Color.DarkSeaGreen),
            new VertexPositionColor(1.0f,  1.0f, -1.0f, Color.DarkViolet),
            new VertexPositionColor(-1.0f,  1.0f, -1.0f, Color.DarkTurquoise)
        };

        readonly short[] CubeElements = new short[]{
            0, 1, 2, 2, 3, 0, // front face 2 tris
            3, 2, 6, 6, 7, 3, // top face
            7, 6, 5, 5, 4, 7, // back face
            4, 0, 3, 3, 7, 4, // left face
            0, 1, 5, 5, 4, 0, // bottom face
            1, 5, 6, 6, 2, 1, // right face

        };
#endregion

        public Test() : base(800, 600) { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //version check
            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(1, 5);
            if (version < target)
            {
                throw new NotSupportedException(String.Format(
                    "OpenGL {0} is required (you only have {1}).", target, version));
            }

            GL.ClearColor(Color.FromArgb(25, 25, 25));
            GL.Enable(EnableCap.DepthTest);

            vbo[0] = LoadVBO(CubeVerts, CubeElements);
            vbo[1] = LoadVBO(CubeVerts, CubeElements);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();
            if (Keyboard[OpenTK.Input.Key.Left])
                angle += 1;
            if (Keyboard[OpenTK.Input.Key.Right])
                angle -= 1;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 lookat = Matrix4.LookAt(0, 5, 5, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            GL.Rotate(angle, 0.0f, 1.0f, 0.0f);

            draw(vbo[0]);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }

        VertexBufferObj LoadVBO<TVertex>(TVertex[] vertices, short[] elements) where TVertex : struct
        {
            VertexBufferObj handle = new VertexBufferObj();
            int size;
            
            //1-gen handles 2-bind handle, upload data, check 3-repeat for elems
            GL.GenBuffers(1, out handle.vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length*BlittableValueType.StrideOf(vertices)),vertices,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            //check
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Ya blew it, vertex data is boned");

            GL.GenBuffers(1, out handle.eboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.eboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(short)), elements,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            //check elems
            if (elements.Length * sizeof(short) != size)
                throw new ApplicationException("jeez, elem data fudged up good");
            handle.numElems = elements.Length;

            return handle;

        }

        public static ObjFile loadOBJ(string location)
        {
            ObjFile result;
            using (StreamReader sr = File.OpenText(location))
            {
                List<Vector3> vertices = new List<Vector3>();
                List<float[]> texcoords = new List<float[]>();
                List<Vector3> norms = new List<Vector3>();
                int vertcount, texcoordcount, normcount, totaltris ;
                List<string> groupnames = new List<string>();
                List<string> usemtlnames = new List<string>();
                short smooth = new short();
                List<int> tricount = new List<int>();
                List<List<int[]>> faceholder = new List<List<int[]>>();
                string mtllib = "";

                string s = "";

                //gather vertices
                vertcount = 0 - 1;//placeholder so VS shuts the fuck up
                while ((s = sr.ReadLine()) != null)
                {
                    string[] chunks = s.Split(' ');
                    if (chunks[0].Equals("v"))
                    {
                        Vector3 temp = new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]));
                        vertices.Add(temp);
                    }
                    else if (chunks[0].Equals("mtllib"))
                    {
                        mtllib = chunks[1];
                    }
                    else if (chunks.Length > 2 && chunks[2].Equals("vertices"))
                    {
                        vertcount = Convert.ToInt32(chunks[1]);
                        break;
                    }
                }

                //gather texture coords
                texcoordcount = 0 - 1;//placeholder so VS shuts the fuck up
                while ((s = sr.ReadLine()) != null)
                {
                    string[] chunks = s.Split(' ');
                    if (chunks[0].Equals("vt"))
                    {
                        texcoords.Add(new float[] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]) });
                    }
                    else if (chunks.Length > 3 && chunks[2].Equals("texture") && chunks[3].Equals("coordinates"))
                    {
                        texcoordcount = Convert.ToInt32(chunks[1]);
                        break;
                    }
                }

                //gather normal vectors
                normcount = 0 - 1;//placeholder so VS shuts the fuck up
                while ((s = sr.ReadLine()) != null)
                {
                    string[] chunks = s.Split(' ');
                    if (chunks[0].Equals("vn"))
                    {
                        norms.Add(new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3])));
                    }
                    else if (chunks.Length > 2 && chunks[2].Equals("normals"))
                    {
                        normcount = Convert.ToInt32(chunks[1]);
                        break;
                    }
                }

                //gather face groups
                totaltris = 0 - 1;//placeholder
                while ((s = sr.ReadLine()) != null)
                {

                    string[] chunks = s.Split(' ');
                    if (chunks[0].Equals("g"))
                    {
                        List<int[]> faces = new List<int[]>();
                        List<int[]> fts = new List<int[]>();
                        List<int[]> fns = new List<int[]>();
                        List<int[]> faceinfolist = new List<int[]>();
       
                        groupnames.Add(chunks[1]);
                        //while in this face group
                        while ((s = sr.ReadLine()) != null)
                        {
                            chunks = s.Split(' ');
                            if (chunks[0].Equals("f"))
                            {
                                int[] faceinfo = faceString(chunks[1], chunks[2], chunks[3]);
                                faceinfolist.Add(faceinfo);
                                continue;
                            }
                            else if (chunks[0].Equals("usemtl"))
                            {
                                usemtlnames.Add(chunks[1]);
                                continue;
                            }
                            else if (chunks[0].Equals("s"))
                            {
                                smooth = Convert.ToInt16(chunks[1]);
                                continue;
                            }
                            else if (chunks[2].Equals("triangles") && chunks[3].Equals("in"))
                            {
                                tricount.Add(Convert.ToInt32(chunks[1]));


                                //any other end of group activities
                                for (int i = 0; i < faceinfolist.Count; i++)
                                {
                                    faces.Add(new int[3] { faceinfolist[i][0], faceinfolist[i][3], faceinfolist[i][6] });
                                    fts.Add(new int[3] { faceinfolist[i][1], faceinfolist[i][4], faceinfolist[i][7] });
                                    fns.Add(new int[3] { faceinfolist[i][2], faceinfolist[i][5], faceinfolist[i][8] });
                                }
                                faceholder.Add(faces);
                                faceholder.Add(fts);
                                faceholder.Add(fns);
                                break;
                            }
                        }
                        //if mtl count and group count not same, add filler mtl name
                        if (groupnames.Count != usemtlnames.Count)
                        {
                            usemtlnames.Add("NO_MTL_SPECIFIED_ERROR");
                        }
                    }
                    if (chunks.Length > 3 && chunks[2].Equals("triangles") && chunks[3].Equals("total"))
                    {
                        totaltris = Convert.ToInt32(chunks[1]);
                        break;
                    }
                }


                //create face group list
                List<ObjFile.FaceGroup> facegs = new List<ObjFile.FaceGroup>();
                for (int i = 0; i < groupnames.Count; i++)
                {
                    facegs.Add(new ObjFile.FaceGroup(groupnames[i], usemtlnames[i], tricount[i], faceholder[0 + i * 3], faceholder[1 + i * 3], faceholder[2 + i * 3]));
                }
                //throw it all in an object
                result = new ObjFile(mtllib, vertices, norms, texcoords, facegs, vertcount, texcoordcount, normcount, totaltris, smooth);
            }
            return result;
        }

        private static int[] faceString(string s1, string s2,string s3)
        {
            int[] result = new int[9];
            string[] s1str= s1.Split('/');
            string[] s2str= s2.Split('/');
            string[] s3str= s3.Split('/');
            for (int i = 0; i < 3; i++)
            {
                result[0+i] = Convert.ToInt32(s1str[i]);
                result[3+i] = Convert.ToInt32(s2str[i]);
                result[6 + i] = Convert.ToInt32(s3str[i]);
            }
            return result;
        }

        void draw(VertexBufferObj handle)
        {
            //1- vertexarray client state enabled 2- bind buffer handles 3-set up data ptrs
            //4- call DrawElement
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.eboID);

            GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(CubeVerts), new IntPtr(0));
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVerts), new IntPtr(12));

            GL.DrawElements(PrimitiveType.Triangles, handle.numElems, DrawElementsType.UnsignedShort, IntPtr.Zero);

        }


        [STAThread]
        public static void Main(string[] args){
            //Utilities.SetWindowTitle(example);
            ObjFile goods = (loadOBJ(@"Z:\code\VSprojects\OpenTK_testground\testmeshes\link_sb64\Link.obj"));
            using (Test example = new Test())
            {
                example.Run(30.0, 0.0);
            }
        }//end main

    }

}