using System;
using System.IO;
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
        public List<Mtl> mtls;
        public short smooth;
        public int vertcount, texcoordcount, normcount, totaltris;
        public string mtllib;

        public struct Mtl
        {
            public short illum;
            public string mtlname,filename;
            public Vector3 diffuse, ambient, specular, specExp;
            public float specCoeff;
        }

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
            this.mtls = getMtls(this.mtllib);
        }

        public List<Mtl> getMtls(string mtlname)
        {
            //string path = @"Z:\code\VSprojects\dddEngine\testmeshes\link_sb64\";
            string path = @".\";
            path = path + mtlname;
            List<Mtl> result = new List<Mtl>();
            
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                Mtl single;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] chunks = s.Split(' ');
                    if (chunks[0] == "newmtl")
                    {
                        single.mtlname = chunks[1];
                        s=sr.ReadLine();
                        chunks = s.Split(' ');
                        single.illum = Int16.Parse(chunks[1]);
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.diffuse = new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]));
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.ambient = new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]));
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.specular = new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]));
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.specExp = new Vector3(Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]));
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.specCoeff = Convert.ToSingle(chunks[1]);
                        s = sr.ReadLine();
                        chunks = s.Split(' ');
                        single.filename = chunks[1];

                        result.Add(single);


                    }
                }
            }
            return result;
        }

        public ObjFile(ObjData data)
        {
            this.mtllib = data.mtllib;
            this.verts = data.verts;
            this.norms = data.norms;
            this.texcoords = data.texcoords;
            this.facegroups = data.facegroups;
            this.vertcount = data.vertcount;
            this.texcoordcount = data.texcoordcount;
            this.normcount = data.normcount;
            this.totaltris = data.totaltris;
            this.smooth = data.smooth;
            this.mtls = getMtls(data.mtllib);
        }

        public ObjFile(string str):this(parseObj(str)){ /*uhhh?*/}

        private static ObjData parseObj(string location)
        {
            ObjData result = new ObjData();
            using (StreamReader sr = File.OpenText(location))
            {
                List<Vector3> vertices = new List<Vector3>();
                List<float[]> texcoords = new List<float[]>();
                List<Vector3> norms = new List<Vector3>();
                int vertcount, texcoordcount, normcount, totaltris;
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
                result = new ObjData(mtllib, vertices, norms, texcoords, facegs, vertcount, texcoordcount, normcount, totaltris, smooth);
            }
            return result;
        }

        public struct ObjData
        {
            public string mtllib;
            public List<Vector3> verts, norms;
            public List<float[]> texcoords;
            public List<FaceGroup> facegroups;
            public int vertcount, texcoordcount, normcount, totaltris;
            public short smooth;

            public ObjData(string mtllib, List<Vector3> verts, List<Vector3> norms, List<float[]> texcoords,
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
        }
        
        //objfile helper function
        private static int[] faceString(string s1, string s2, string s3)
        {
            int[] result = new int[9];
            string[] s1str = s1.Split('/');
            string[] s2str = s2.Split('/');
            string[] s3str = s3.Split('/');
            for (int i = 0; i < 3; i++)
            {
                result[0 + i] = Convert.ToInt32(s1str[i]);
                result[3 + i] = Convert.ToInt32(s2str[i]);
                result[6 + i] = Convert.ToInt32(s3str[i]);
            }
            return result;
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