//MS
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
//others
using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
//mine


/* Notes: Change VBO structure; bind one vertex array, bind multiple element arrays with corresponding texture arrays.
 * 
 */

namespace dddEngine
{
    
    public class Test : GameWindow
    {

        //perspective declarations
        Vector3 eye = new Vector3(0f, .2f, 1.3f);
        Vector3 target = new Vector3(0f, .2f, 0f);
        Vector3 up = new Vector3(0f, 1f, 0f);
        //parametric variables for camera circling
        float r = 0f;
        float a1 = 0f;
        float a2 = 0f;

        // GLSL Objects
        int VertexShaderObject, FragmentShaderObject, ProgramObject,TextureObject;

        //note: should only have one vboID, several eboIDs with their numelems
        struct VertexBufferObj { public int vboID, eboID, numElems;}

        // this is from a tutorial and patently stupid, Vector3 will suffice
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

        VertexPositionColor[] linkVerts;
        uint[] elementsLink;
        float[] texturecoordinates;

        public Test() : base(800, 500) { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.r = 1.5f;

            this.a1 = (float)(Math.PI / 2f);
            this.a2 = (float)(Math.PI / 2f);
            eye = sphere2cart(r, a1, a2) + this.target;
            //ObjFile goods = (new ObjFile(@"Z:\code\VSprojects\dddEngine\testmeshes\link_sb64\Link.obj"));
            /* TEST */
            ObjFile goods = (new ObjFile("Link.obj"));
            /*
            VertexBufferObj[] testVBOs = new VertexBufferObj[goods.facegroups.Count * 2];
            for (int i = 0; i < testVBOs.Length; i++)
            {
                //uint[] uintface = new uint[];
                testVBOs[i] = LoadVBO(goods.verts.ToArray, (goods.facegroups[i].faces));
            }*/
                //ObjFile goods = (new ObjFile("RB-OptimusBoss.obj"));
                texturecoordinates = new float[goods.texcoordcount * 2];
            for (int i = 0; i < goods.texcoordcount; i++)
            {
                texturecoordinates[2*i] = goods.texcoords[i][0];
                texturecoordinates[2 * i+1] = goods.texcoords[i][1];
            }

                //add all vertices to VBO, then elements obj by obj
                linkVerts = new VertexPositionColor[goods.vertcount];

            for (int i = 0; i < goods.vertcount; i++)
            {
                
                linkVerts[i]=new VertexPositionColor(goods.verts[i][0], goods.verts[i][1], goods.verts[i][2],Color.White);
            }

            elementsLink = new uint[goods.totaltris * 3];
            int irregularCount = 0;
            for(int i =0; i < goods.facegroups.Count;i++)
            {
                for (int j = 0; j < goods.facegroups[i].tris; j++)
                {
                    elementsLink[irregularCount*3 +0] = (uint)(goods.facegroups[i].faces[j][0] - 1);
                    elementsLink[irregularCount*3 + 1] = (uint)(goods.facegroups[i].faces[j][1] - 1);
                    elementsLink[irregularCount*3 +2] = (uint)(goods.facegroups[i].faces[j][2] - 1);
                    irregularCount++;
                }
            }

            //version check
            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(1, 5);
            if (version < target)
            {
                throw new NotSupportedException(String.Format(
                    "OpenGL {0} is required (you only have {1}).", target, version));
            }
            /*
            //texture lyfe bruh
            GL.Enable(EnableCap.Texture2D);
            int[] texIds = new int[goods.facegroups.Count];
            GL.GenTextures(goods.facegroups.Count,texIds);
            //GL.BindTextures()

            //bind textures in a loop, because i'm inefficient like that
            string texPath = @"Z:\code\VSprojects\dddEngine\testmeshes\link_sb64\";
            Bitmap bmpTemp;

            string path = @"Z:\code\VSprojects\dddEngine\testmeshes\link_sb64\";
            for(int i = 0; i < texIds.Length;i++){

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
               // GL.TexEnv()

                texPath = path + goods.mtls[i].filename;
                bmpTemp = new Bitmap(texPath);
                GL.BindTexture(TextureTarget.Texture2D, i + 1);
                BitmapData data = bmpTemp.LockBits(new System.Drawing.Rectangle(0, 0, bmpTemp.Width, bmpTemp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                
                bmpTemp.UnlockBits(data);
            }
             * 
            string vs=path + "shader.vert";
            string fs=path+"shader.frag";
            */
            GL.ClearColor(Color.BlueViolet);
            GL.Enable(EnableCap.DepthTest);

            

            //CreateShaders(vs, fs, out VertexShaderObject, out FragmentShaderObject, out ProgramObject);

            vbo[0] = LoadVBO(linkVerts, elementsLink);
            vbo[1] = LoadVBO(linkVerts, elementsLink);
        }//end ONLOAD

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();
            if (Keyboard[OpenTK.Input.Key.Left])
                a2 -= 0.2f;
            if (Keyboard[OpenTK.Input.Key.Right])
                a2 += 0.2f;
            if (Keyboard[OpenTK.Input.Key.Up])
                a1 += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Down])
                a1 -=0.1f;
            if (Keyboard[OpenTK.Input.Key.S])
            {
                if (r > 1.3f)
                    r -= 0.1f;
            }
            if (Keyboard[OpenTK.Input.Key.W])
                r += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Space]){
                r = 1.5f;
                a1= (float)(Math.PI/2f);
                a2 = (float)(Math.PI / 2f);
            }
            eye = sphere2cart(r, a1, a2) + target;
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(ProgramObject);

            Matrix4 lookat = Matrix4.LookAt(eye, target, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            //GL.Disable(EnableCap.StencilTest);
            //GL.Disable(EnableCap.Lighting);

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

        VertexBufferObj LoadVBO(VertexPositionColor[] vertices, uint[] elements)
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

            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * BlittableValueType.StrideOf(elements)), elements,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            //check elems
            if (elements.Length * sizeof(uint) != size)
               throw new ApplicationException("jeez, elem data fudged up good");
            handle.numElems = elements.Length;
            GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(BlittableValueType.StrideOf(texturecoordinates)), texturecoordinates, BufferUsageHint.StaticDraw);

            return handle;
        }

        VertexBufferObj LoadVBO(Vector3[] vertices, uint[] elements)
        {
            VertexBufferObj handle = new VertexBufferObj();
            int size;

            //1-gen handles 2-bind handle, upload data, check 3-repeat for elems
            GL.GenBuffers(1, out handle.vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * BlittableValueType.StrideOf(vertices)), vertices,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            //check
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Ya blew it, vertex data is boned");

            GL.GenBuffers(1, out handle.eboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.eboID);

            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * BlittableValueType.StrideOf(elements)), elements,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            //check elems
            if (elements.Length * sizeof(uint) != size)
                throw new ApplicationException("jeez, elem data fudged up good");
            handle.numElems = elements.Length;
            //GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(BlittableValueType.StrideOf(texturecoordinates)), texturecoordinates, BufferUsageHint.StaticDraw);

            return handle;
        }

        void draw(VertexBufferObj handle)
        {
            //wireframe
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            GL.PointSize(5f);

            //1- vertexarray client state enabled 2- bind buffer handles 3-set up data ptrs
            //4- call DrawElement
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.eboID);

            GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(linkVerts), (IntPtr)(0));
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(linkVerts), new IntPtr(12));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, BlittableValueType.StrideOf(texturecoordinates), (IntPtr)(0));

            GL.DrawElements(PrimitiveType.Triangles, handle.numElems, DrawElementsType.UnsignedInt, new IntPtr(0));

            GL.ColorPointer(4,ColorPointerType.UnsignedByte,BlittableValueType.StrideOf(linkVerts),(IntPtr)(0));
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.LineWidth(2);
            GL.DrawElements(PrimitiveType.Triangles,handle.numElems, DrawElementsType.UnsignedInt, new IntPtr(0));
        }

        public Vector3 sphere2cart(float r, float a1, float a2)
        {
            float x=r*(float)(Math.Sin(a1)*Math.Cos(a2));
            float z=r*(float)(Math.Sin(a1)*Math.Sin(a2));
            float y=r*(float)(Math.Cos(a1));
            Vector3 result = new Vector3(x,y,z);
            return result;
        }

        #region CreateShaders -- stolen from opentk
        
        void CreateShaders(string vs, string fs,
            out int vertexObject, out int fragmentObject,
            out int program)
        {
            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
            GL.UseProgram(program);
        }
        
        #endregion

        
        [STAThread]
        public static void Main(string[] args){
            using (Test example = new Test())
            {
                example.Run(30.0, 0.0);
            }
        }//end main
    }//end class
}//end namespace