using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace dddEngine
{
    public class Vector3Array
    {
        public Vector3[] points;

        public Vector3Array(List<float[]> into){
            this.points = new Vector3[into.Count];
            for (int i = 0; i < into.Count; i++)
            {
                this.points[i].X = into[i][0];
                this.points[i].Y = into[i][1];
                this.points[i].Z = into[i][2];
            }
        }

        public Vector3Array(float[][] into)
        {
            this.points = new Vector3[into.Length];
            for (int i = 0; i < into.Length; i++)
            {
                this.points[i].X = into[i][0];
                this.points[i].Y = into[i][1];
                this.points[i].Z = into[i][2];
            }
        }
        
    }

    public class Vector2Array
    {
        public Vector2[] points;

            public Vector2Array(List<float[]> into){
            this.points = new Vector2[into.Count];
            for (int i = 0; i < into.Count; i++)
            {
                this.points[i].X = into[i][0];
                this.points[i].Y = into[i][1];
            }
        }
    }
}
