using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace dddEngine
{
    class Debug
    {
        public Debug() { }

        public Object checkBuffer(string arg, int bytesize){
            Object result;
            ByteStruct[] data = new ByteStruct[bytesize];
            switch (arg)
            {
                case "v3":
                    GL.GetBufferSubData(BufferTarget.ArrayBuffer, new IntPtr(0), new IntPtr(bytesize), data);
                    float[] newarray = new float[data.Length];
                    for (int i = 0; i < newarray.Length; i++)
                    {
                        byte[] val = new byte[4];
                        val = BitConverter.GetBytes(data[i].e);
                        newarray[i] = BitConverter.ToSingle(val,0);
                    }
                    result = newarray;
                    break;
                default:
                    result = IntPtr.Zero;
                    break;
            }

            return result;
        }
    }
}
