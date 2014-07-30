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
    //A Component is: a group of faces with common material properties/texture
    //A Component has: vertex, normal, and texture indices, name, texture


    public class Component
    {
        public List<ulong[]> vertexElements,normalElements,textureElements;
        public uint[] ves;
        public string name;
        public Bitmap texture;
        public uint vertexHandle;
        //other facets of a model i guess, we'll get there in time.

        public Component(List<ulong[]> vertexElements, List<ulong[]> normalElements, List<ulong[]> textureElements, string name, Bitmap texture)
        {
            this.vertexElements = vertexElements;
            this.normalElements = normalElements;
            this.textureElements = textureElements;
            this.name = name;
            this.ves = new uint[vertexElements.Count * 3];
            for (int i = 0; i < vertexElements.Count; i++)
            {
                ves[0 + i*3] = (uint)vertexElements[i][0];
                ves[1 + i*3] = (uint)vertexElements[i][1];
                ves[2 + i * 3] = (uint)vertexElements[i][2];

            }
        }
    }
}