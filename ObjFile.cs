using System;
using System.Collections.Generic;
using OpenTK;

namespace dddEngine
{
    public class ObjFile
    {
        //declarations for OBJ file facets
        public List<Vector3> verts, norms;
        public List<float[]> texcoords;
        public List<FaceGroup> facegroups;
        public short smooth;
        public int vertcount, texcoordcount, normcount, totaltris;
        public string mtllib;

        public ObjFile(string mtllib, List<Vector3> verts, List<Vector3> norms, List<float[]> texcoords,
                            List<FaceGroup> facegroups, int vertcount, int texcoordcount, int normcount, int totaltris,
                            short smooth)
        {
            this.mtllib = mtllib;
            this.verts = verts;
            this.norms = norms;
            this.texcoords = texcoords;
            this.facegroups = facegroups;
            this.vertcount = vertcount;
            this.texcoordcount = texcoordcount;
            this.normcount = normcount;
            this.totaltris = totaltris;
            this.smooth = smooth;
        }

        public struct FaceGroup
        {
            public string group, usemtl; 
            public int tris;
            public List<int[]> faces, ftextcoord, fnorms;

            public FaceGroup(string group, string usemtl, int tris, List<int[]> faces,
                                List<int[]> ftc, List<int[]> fnorms)
            {
                this.group = group;
                this.usemtl = usemtl;
                this.tris = tris;
                this.faces = faces;
                this.ftextcoord = ftc;
                this.fnorms = fnorms;
            }
        };
    }
}