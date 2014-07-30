using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace dddEngine
{
    public class Entity
    {
        public string name;
        public Vector3Array vertexes;
        public Vector2Array teevees;
        public List<float[]> vertices, normals, texturecoordinates;
        public List<Component> components;
        public uint vertexHandle;
        public uint[] componentElementHandles;

        public struct nonnullVecs
        {
            public List<float[]> vals;
        }
        public struct nonnullLongs
        {
            public List<ulong[]> elements;
        }

        public Entity(ObjFile obj)
        {
            //set general entity stuff
            this.name = obj.name;
            this.vertices = obj.vertexList;
            this.normals = obj.normalList;
            this.texturecoordinates = obj.textureCoordinates;
            this.vertexes = new Vector3Array(obj.vertexList);


            //for each face group, make components
            this.components = new List<Component>();
            Component comp;
            for (int i = 0; i < obj.vertexElements.Count; i++)
            {
                comp = new Component(obj.vertexElements[i], obj.normalElements[i],obj.textureElements[i],
                    obj.groupnames[i], null);
                this.components.Add(comp);
            }
            this.componentElementHandles = new uint[this.components.Count];
        }

        public void bindVertexHandle()
        {
            int size = 0;
            GL.GenBuffers(1, out this.vertexHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexHandle);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.vertexes.points.Length * Vector3.SizeInBytes), this.vertexes.points,
            BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
        }

        public void bindElementHandles()
        {
            for (int i = 0; i < components.Count; i++)
            {
                int size = 0;
                GL.GenBuffers(1, out this.componentElementHandles[i]);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.componentElementHandles[i]);

                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(this.components[i].ves.Length * sizeof(uint)), this.components[i].ves,
                BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            }
        }

        public void load()
        {
            bindVertexHandle();
            bindElementHandles();
        }

        public void draw()
        {
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);

            GL.EnableClientState(ArrayCap.VertexArray);
            float[] color = new float[3];
            GL.Color3(0f,1f,0f);

            for(int i = 0;i<this.components.Count;i++){
                drawComponent(i);
            }


        }

        public void drawComponent(int i){
            //draw solid
            GL.Color3(0f, .7f, 0f);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.componentElementHandles[i]);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr)(0));
            GL.DrawElements(PrimitiveType.Triangles, this.components[i].ves.Length, DrawElementsType.UnsignedInt, new IntPtr(0));

            //draw wireframe
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.LineWidth(2f);
            GL.Color3(1f, 1f, .8f);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.componentElementHandles[i]);
            

            GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr)(0));
            GL.DrawElements(PrimitiveType.Triangles, this.components[i].ves.Length, DrawElementsType.UnsignedInt, new IntPtr(0));

        }

    }

}
