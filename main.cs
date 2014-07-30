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
    //global structs
    public struct ByteStruct { public uint e;};

    //global vars

    public class Test : GameWindow
    {

        Random rn = new Random();
        dddScene scene;

        //perspective declarations
        Vector3 eye = new Vector3(0f, .2f, 1.3f);
        Vector3 target = new Vector3(0f, .2f, 0f);
        Vector3 up = new Vector3(0f, 1f, 0f);
        //variables for camera circling
        float r = 0f;
        float a1 = 0f;
        float a2 = 0f;

        //fps counter stuffs
        float fps=60f;

        // GLSL Objects
        int VertexShaderObject, FragmentShaderObject, ProgramObject,TextureObject;

        //note: should only have one vboID, several eboIDs with their numelems
        struct VertexBufferObj { public int vboID, eboID, numElems;}
        

        VertexBufferObj[] vbo = new VertexBufferObj[2];

        public Test() : base(800, 600) {
            this.scene = new dddScene();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.r = 1.5f;

            this.a1 = (float)(Math.PI / 2f);
            this.a2 = (float)(Math.PI / 2f);
            this.scene.cameras[0][0] = sphere2cart(r, a1, a2) + this.scene.cameras[0][1];

            ObjFile goods = (new ObjFile("Link.obj"));
            this.scene = new dddScene();
            this.scene.entities.Add(new Entity(goods));

            //version check
            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(1, 5);
            if (version < target)
            {
                throw new NotSupportedException(String.Format(
                    "OpenGL {0} is required (you only have {1}).", target, version));
            }

            GL.ClearColor(Color.BlueViolet);
            GL.Enable(EnableCap.DepthTest);

            //CreateShaders(vs, fs, out VertexShaderObject, out FragmentShaderObject, out ProgramObject);

            scene.load();

            //vbo[0] = LoadVBO(goods.verts, elementsLink);
            //vbo[1] = LoadVBO(linkVerts, elementsLink);
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
            this.scene.cameras[0][0] = sphere2cart(r, a1, a2) + this.scene.cameras[0][1];
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(ProgramObject);

            Matrix4 lookat = Matrix4.LookAt(scene.cameras[0][0], scene.cameras[0][1], scene.up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            //GL.Disable(EnableCap.StencilTest);
            //GL.Disable(EnableCap.Lighting);

            //draw(vbo[0]);
            scene.draw();


            SwapBuffers();

            fps = ((float)(1 / e.Time) + fps) / 2;
            base.Title = fps.ToString();
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
        /*
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
        }*/
        /*
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
        }*/

        void draw(VertexBufferObj handle)
        {
            this.scene.draw();
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
                example.Run(60.0, 0.0);
            }
        }//end main
    }//end class
}//end namespace