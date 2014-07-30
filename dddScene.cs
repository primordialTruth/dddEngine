using System;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace dddEngine
{

public class dddScene
{
    //what makes a scene?
    // Scene->entities->component
    //         --first entity = grid?
    //      ->lights
    //   entity: single vertex group, multiple components
    //   component: element array(ref entity) with unique texture
 
    //declarations
    public List<Entity> entities;
    public List<Vector3[]> cameras;//camera is a point and a target
    public int eyeCameraIndex; //this is stupid, do it implicitly at 0
    public Light[] lights;
    public Entity grid;
    public uint[] gridHandles;
    
    //variables
    public Vector3 up = new Vector3(0f,1f,0f);//y axis is up

    //grid shouldnt be entity or component, too much wasted space
    public float gridWidth = 10f;
    public float gridHeight = 10f;
    public uint gridDivisions = 100;

    public Vector3Array gridVertices;
    public uint[] gridElements;

    public dddScene() { 
        //returns grid of given size centered at origin
        this.gridVertices = makeGrid(gridWidth, gridHeight, gridDivisions);
        this.gridElements = makeElements(this.gridDivisions);

        //default values
        this.entities = new List<Entity>();
        this.cameras = new List<Vector3[]>();
        cameras.Add(new Vector3[2] { new Vector3(0f, .5f, 1f), new Vector3(0f, 0f, 0f) });
        loadGrid();
    }

    public struct Light { Vector3 direction; uint color; float intensity;};
    

    //killllmeeeee
    public struct thisisdumb { public uint e;};
    //killllmeeeee

    //==THE HELP====== - helper functions
    
    //drawGrid - make a draw call to draw the grid at the origin.
    public void drawGrid()
    {

        //wireframe
        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
        GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
        GL.PointSize(1.5f);
        GL.LineWidth(1f);

        //1- vertexarray client state enabled 2- bind buffer handles 3-set up data ptrs
        //4- call DrawElement
        //GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.TextureCoordArray);
        

        GL.BindBuffer(BufferTarget.ArrayBuffer, this.gridHandles[0]);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.gridHandles[1]);

        GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr)(0));
        //GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, new IntPtr(0));
        //GL.TexCoordPointer(2, TexCoordPointerType.Float, BlittableValueType.StrideOf(texturecoordinates), (IntPtr)(0));

        GL.DrawElements(PrimitiveType.Lines, this.gridElements.Length, DrawElementsType.UnsignedInt, new IntPtr(0));

    }

    //draws scene from objects loaded in GPU
    public void draw()
    {
        drawGrid();
        for (int i = 0; i < this.entities.Count; i++)
        {
            this.entities[i].draw();
        }
    }

    //loads graphics objects onto GPU
    public void load()
    {
        loadGrid();
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].load();
        }
    }

    public void loadGrid()
    {
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        //GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.VertexArray);
        GL.LineWidth(2);

        //get handles
        int size = 0;
        this.gridHandles = new uint[2];
        GL.GenBuffers(1, out this.gridHandles[0]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.gridHandles[0]);

        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.gridVertices.points.Length * Vector3.SizeInBytes), this.gridVertices.points,
            BufferUsageHint.StaticDraw);
        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

        thisisdumb[] isntit = new thisisdumb[size/(sizeof(uint)/sizeof(byte))];
        GL.GetBufferSubData(BufferTarget.ArrayBuffer, new IntPtr(0), new IntPtr(size), isntit);
        float[] newarray = new float[isntit.Length];
        for (int i = 0; i < newarray.Length; i++)
        {
            byte[] val = new byte[4];
            val = BitConverter.GetBytes(isntit[i].e);
            newarray[i] = BitConverter.ToSingle(val,0);
        }

            //check
            if (this.gridVertices.points.Length * Vector3.SizeInBytes != size)
                throw new ApplicationException("Ya blew it, vertex data is boned");

        GL.GenBuffers(1, out this.gridHandles[1]);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.gridHandles[1]);

        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(this.gridElements.Length * sizeof(uint)), this.gridElements,
            BufferUsageHint.StaticDraw);
        GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
        //check elems
        if (this.gridElements.Length * sizeof(uint) != size)
            throw new ApplicationException("jeez, elem data fudged up good");

        //loaded, now draw -- later
    }


    //makeGrid - make a list of points to be rendered into a grid of quads
    public Vector3Array makeGrid(float w, float h, uint div)
    {
        //makes vertices for a grid -- if div = 2, verts look like:
        //   0  1  2                     +--+--+ 
        //   7     3  to be made into -> |--|--| 
        //   6  5  4                     +--+--+
        //              elements would be: (0,2,2,4,4,6,6,0,1,5,7,3)
        //Vector3Array holder;
        float[][] holder = new float[4*div][];
        //top and bot
        for (int i = 0; i < div + 1; i++)
        {
            //top
            //holder = new Vector3Array();
            holder[i] = new float[3]{-w/2.0f + w*i/div, 0f, -h/2.0f};
            //result[i] = holder;
            //bottom
            //holder = new VertexStruct();
            holder[2*div+i] = new float[3]{w / 2.0f - w * i / div, 0f, h / 2.0f};
            //result[2*div+i]=holder;
        }
        //sides
        for (int i = 0; i < div - 1;i++){
            float height = -h / 2.0f + (i + 1) * h / div;
            //right
            //holder = new VertexStruct();
            holder[div + i + 1] = new float[3]{w/2.0f,0f,height};
            //result[div + i + 1] = holder;
            //left
            //holder = new VertexStruct();
            holder[4 * div - i - 1] = new float[3]{-w / 2.0f, 0f, height};
            //result[4 * div - i - 1] = holder;
        }
        Vector3Array result = new Vector3Array(holder);
        return result;
    }

    //makeElements - make a list of index positions in reference to grid elements 
    public uint[] makeElements(uint divs)
    {
        uint elementsize = 2*2*(divs+1);//number of lines*2 points per line

        uint[]result = new uint[elementsize];
        //exterior:
        //--top
        result[0] = 0;
        result[1] = divs;
        //--right
        result[2] = divs;
        result[3] = 2*divs;
        //--bottom
        result[4] = 2*divs;
        result[5] = 3*divs;
        //--left
        result[6] = 3*divs;
        result[7] = 0;

        //interior
        for (uint i = 0; i < divs-1; i++)
        {
            //vertical
            result[8+2*i] = i+1;
            result[8+2*i+1] = 3*divs-1-i;
            //horizontal
            result[8+(divs-1)*2+2*i]=4*divs-1-i;
            result[8 + (divs - 1) * 2 + 2 * i + 1] = divs + 1 + i;

        }
        return result;
    }


}//end class dddScene
}//end namespace