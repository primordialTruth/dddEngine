using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace dddEngine
{

    public class ObjFile
    {
        /*
        //declarations for OBJ file facets
        public List<float[]> verts, norms;
        public List<float[]> texcoords;
        public List<FaceGroup> facegroups;
        public List<Mtl> mtls;
        public short smooth;
        public long vertcount, texcoordcount, normcount, totaltris;
        public string mtllib;

        public struct Mtl
        {
            public short illum;
            public string mtlname,filename;
            public Vector3 diffuse, ambient, specular, specExp;
            public float specCoeff;
        }

        public struct FaceGroup
        {
            public string group, usemtl;
            public long tris;
            public List<ulong[]> faces, ftextcoord, fnorms;

            public FaceGroup(string group, string usemtl, long tris, List<ulong[]> faces,
                                List<ulong[]> ftc, List<ulong[]> fnorms)
            {
                this.group = group;
                this.usemtl = usemtl;
                this.tris = tris;
                this.faces = faces;
                this.ftextcoord = ftc;
                this.fnorms = fnorms;
            }
        };

        public ObjFile(string mtllib, List<float[]> verts, List<float[]> norms, List<float[]> texcoords,
                            List<FaceGroup> facegroups, long vertcount, long texcoordcount, long normcount, long totaltris,
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

            string path = @".\testmeshes\link_sb64\";
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

        public ObjFile(string str):this(parseObj(str)){}

        private static ObjData parseObj(string location)
        {
            ObjData result = new ObjData();
            using (StreamReader sr = File.OpenText(location))
            {
                List<float[]> vertices = new List<float[]>();
                List<float[]> texcoords = new List<float[]>();
                List<float[]> norms = new List<float[]>();
                long vertcount, texcoordcount, normcount, totaltris;
                List<string> groupnames = new List<string>();
                List<string> usemtlnames = new List<string>();
                short smooth = new short();
                List<long> tricount = new List<long>();
                List<List<ulong[]>> faceholder = new List<List<ulong[]>>();
                string mtllib = "";

                string s = "";

                //gather vertices
                vertcount = 0 - 1;//placeholder so VS shuts the fuck up
                while ((s = sr.ReadLine()) != null)
                {
                    string[] chunks = s.Split(' ');
                    if (chunks[0].Equals("v"))
                    {
                        float[] temp = new float[3]{Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3])};
                        vertices.Add(temp);
                    }
                    else if (chunks[0].Equals("mtllib"))
                    {
                        mtllib = chunks[1];
                    }
                    else if (chunks.Length > 2 && chunks[2].Equals("vertices"))
                    {
                        vertcount = Convert.ToInt64(chunks[1]);
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
                        texcoords.Add(new float[2]{ Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]) });
                    }
                    else if (chunks.Length > 3 && chunks[2].Equals("texture") && chunks[3].Equals("coordinates"))
                    {
                        texcoordcount = Convert.ToInt64(chunks[1]);
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
                        norms.Add(new float[3]{Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3])});
                    }
                    else if (chunks.Length > 2 && chunks[2].Equals("normals"))
                    {
                        normcount = Convert.ToInt64(chunks[1]);
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
                        List<ulong[]> faces = new List<ulong[]>();
                        List<ulong[]> fts = new List<ulong[]>();
                        List<ulong[]> fns = new List<ulong[]>();
                        List<ulong[]> faceinfolist = new List<ulong[]>();

                        groupnames.Add(chunks[1]);
                        //while in this face group
                        while ((s = sr.ReadLine()) != null)
                        {
                            chunks = s.Split(' ');
                            if (chunks[0].Equals("f"))
                            {
                                ulong[] faceinfo = faceString(chunks[1], chunks[2], chunks[3]);
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
                                tricount.Add(Convert.ToInt64(chunks[1]));


                                //any other end of group activities
                                for (int i = 0; i < faceinfolist.Count; i++)
                                {
                                    faces.Add(new ulong[3] { faceinfolist[i][0], faceinfolist[i][3], faceinfolist[i][6] });
                                    fts.Add(new ulong[3] { faceinfolist[i][1], faceinfolist[i][4], faceinfolist[i][7] });
                                    fns.Add(new ulong[3] { faceinfolist[i][2], faceinfolist[i][5], faceinfolist[i][8] });
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
                        totaltris = Convert.ToInt64(chunks[1]);
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
            public List<float[]> verts, norms;
            public List<float[]> texcoords;
            public List<FaceGroup> facegroups;
            public long vertcount, texcoordcount, normcount, totaltris;
            public short smooth;

            public ObjData(string mtllib, List<float[]> verts, List<float[]> norms, List<float[]> texcoords,
                            List<FaceGroup> facegroups, long vertcount, long texcoordcount, long normcount, long totaltris,
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
        private static ulong[] faceString(string s1, string s2, string s3)
        {
            ulong[] result = new ulong[9];
            string[] s1str = s1.Split('/');
            string[] s2str = s2.Split('/');
            string[] s3str = s3.Split('/');
            for (int i = 0; i < 3; i++)
            {
                result[0 + i] = (ulong)Convert.ToInt64(s1str[i]);
                result[3 + i] = (ulong)Convert.ToInt64(s2str[i]);
                result[6 + i] = (ulong)Convert.ToInt64(s3str[i]);
            }
            return result;
        }
        */
        //entity-wide variables
        public List<float[]> vertexList, textureCoordinates, normalList, parametricVerts;
        public string name;

        //group information-each index value represents an object
        public List<List<ulong[]>> vertexElements, normalElements, textureElements;
        public List<string> materialnames, groupnames;
        //material information


        //constructor - file location
        public ObjFile(string filename){
            this.name = filename;
		    filename = "./"+filename;
            this.vertexList = new List<float[]>();
            this.textureCoordinates = new List<float[]>();
            this.normalList = new List<float[]>();
            this.groupnames = new List<string>();
            this.materialnames = new List<string>();
            this.vertexElements = new List<List<ulong[]>>();
            this.normalElements = new List<List<ulong[]>>();
            this.textureElements = new List<List<ulong[]>>();

                

		        using(StreamReader sr = File.OpenText(filename)){
			        //variables for this loop
			        string mtllib = "";
			        string s = "";
                    string[] chunks;

			        while ((s=sr.ReadLine()) != null){
			        if (s.Length>0 ){
				        switch (s.Substring(0,1)){
					        case("m"):
						        if(s.Length>7){
							        mtllib = s.Substring(7);
						        }
						        break;
                            #region v-
                            case ("v"):
                                //digest the vertex list do first, while rest
                                if (s.Substring(1, 1) == " ")
                                {
                                    float[] floatArray = new float[3];
                                    chunks = s.Split(' ');
                                    if (chunks.Length > 3)
                                    {
                                        floatArray = new float[3] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]) };
                                        this.vertexList.Add(floatArray);
                                    }
                                    while ((s = sr.ReadLine()) != "" && s.Substring(0, 1) != "#")
                                    {
                                        chunks = s.Split(' ');
                                        if (chunks.Length > 3)
                                        {
                                            floatArray = new float[3] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]) };
                                            this.vertexList.Add(floatArray);
                                        }
                                    }
                                }
                                else if (s.Substring(1, 1) == "t")
                                {
                                    float[] floatArray;
                                    chunks = s.Split(' ');
                                    if (chunks.Length > 2)
                                    {
                                        floatArray = new float[2] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2])};
                                        this.textureCoordinates.Add(floatArray);
                                    }
                                    while ((s = sr.ReadLine()) != "" && s.Substring(0,1)!="#")
                                    {
                                        chunks = s.Split(' ');
                                        if (chunks.Length > 2)
                                        {
                                            floatArray = new float[2] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2])};
                                            this.textureCoordinates.Add(floatArray);
                                        }
                                    }
                                }
                                else if (s.Substring(1, 1) == "n")
                                {
                                    float[] floatArray;
                                    chunks = s.Split(' ');
                                    if (chunks.Length > 3)
                                    {
                                        floatArray = new float[3] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]) };
                                        this.normalList.Add(floatArray);
                                    }
                                    while ((s = sr.ReadLine()) != ""  && s.Substring(0,1)!="#")
                                    {
                                        chunks = s.Split(' ');
                                        if (chunks.Length > 3)
                                        {
                                            floatArray = new float[3] { Convert.ToSingle(chunks[1]), Convert.ToSingle(chunks[2]), Convert.ToSingle(chunks[3]) };
                                            this.normalList.Add(floatArray);
                                        }
                                    }
                                }
						        break;
                            #endregion

                            #region g-
                            case ("g"):
                                this.groupnames.Add(s.Substring(2));
                                List<ulong[]> groupulongarray;
                                List<ulong[]> groupfaces = new List<ulong[]>();
                                List<ulong[]> grouptextures = new List<ulong[]>();
                                List<ulong[]> groupnormals = new List<ulong[]>();

                                while ((s = sr.ReadLine()) != ""  && s.Substring(0,1)!="#")//this might be fucky
                                {
                                    //face format: f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3
                                    if (s.Substring(0, 1) == "f")
                                    {
                                        chunks = s.Split(' ');
                                        groupulongarray = faceInfoTo3b3(chunks[1], chunks[2], chunks[3]);
                                        groupfaces.Add(groupulongarray[0]);
                                        grouptextures.Add(groupulongarray[1]);
                                        groupnormals.Add(groupulongarray[2]);
                                        continue;
                                    }
                                    else if (s.Length > 7 && s.Substring(0, 7) == "usemtl ")
                                    {
                                        materialnames.Add(s.Substring(7));
                                        continue;
                                    }
                                }
                                //after lists compiled, combine into master object
                                this.vertexElements.Add(groupfaces);
                                this.normalElements.Add(groupnormals);
                                this.textureElements.Add(grouptextures);
						        break;
                            #endregion

                            default:
						        break;

				        }//end switch
			        }//end if
		        }//end while
            }//end using
        }//end constructor

        //the help -- helper functions
        private List<ulong[]> faceInfoTo3b3(string v1, string v2, string v3)
        {
            List<ulong[]> result = new List<ulong[]>();
            string[] vals = { v1, v2, v3 };
            ulong[] vertex =new ulong[3] ;
            ulong[] texture = new ulong[3];
            ulong[] normal = new ulong[3];
            string[] digest;


            //wavefront indexing starts at 1, so subtract 1
            for (int i = 0; i < 3; i++)
            {
                digest = vals[i].Split('/');
                vertex[i] = (ulong)(Convert.ToInt64(digest[0])-1);
                texture[i] = (ulong)(Convert.ToInt64(digest[1])-1);
                normal[i] = (ulong)(Convert.ToInt64(digest[2])-1);
            }

            result.Add(vertex);
            result.Add(texture);
            result.Add(normal);
            return result;
        }
    }//end namespace
}//end of the goddamned file;