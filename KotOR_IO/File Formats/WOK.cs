using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KotOR_IO
{
    /// <summary>
    /// Kotor walkmesh file format.
    /// </summary>
    public class WOK : KFile
    {
        #region Notes
        /*
        The basic and easy to undertsand portion of walkmeshes is the Faces and vertices.
        Vertex:
            A Vertex is a point in 3D space
        Face:
            A face is a triangle of 3 vertices
            Faces have a a material that determines if you can walk on them and what sound to make when you walk on them
            A Face has 3 edge IDs (faces that share an edge ID border each other, and so a player can walk between them)
                An edge ID of -1 indicates a face on the perimeter of the room
            A Face has a Unit Normal Vector (norm), this indicates the slant of the face, and how to push the player
            A Face has a Coeficient (coef), I have no idea what this does
        Edges: (This particularly refers to edges on the perimeter of the room)
            Right now I'm storing the edges as an Index into the Face adjacency array (yes this is bad)
            If an edge borders another room, the Edges Dictionary will map to the index of this room in the layout file
            Otherwise the dictionary will map to -1
        Perimeters: 
            These represent each disconnected "island" in the mesh
            They are denoted by the ID (or is it index?) of the last Edge in the permeter. (Whatever last means)
        AABB node:
            An Axis aligned Bounding Box, which does "something" in the game
            A node in the AABB tree, has two 3D points BBmax and BBmin. These form a bounding box
            There's also a face index which is negatvie -1 for all except child nodes
                It represents the face that this Bounding Box applies to?
            The unknown value is usally 4, I'm sure what this does
            The Plane value indicates the "most significant plane", I believe this just gives a general simple indication of how the moduel is shaped
            If the face index is -1 then left and right will be AABB nodes for bounding boxes within this one
        Position:
            I'm not sure what this does?
         */
        #endregion Notes

        #region Constants

        const uint AABBNODESIZE = 44;
        const uint DEFAULTVERTOFF = 136;
        const uint VERTEXSIZE = 12;
        const uint FACESIZE = 12;
        const uint INT32SIZE = 4;
        const uint FLOATSIZE = 4;
        const uint EDGESIZE = 8;

        #endregion Constants

        #region Members

        public List<Vert> Vertices = new List<Vert>();
        public List<Face> Faces = new List<Face>();
        public Dictionary<int, int> Edges = new Dictionary<int, int>(); //Matches Edge index to room index in layout for faces that connect to other modules
        public List<int> Perimeters = new List<int>(); //Indices of the final edge of each perimeter in this walkmesh
        public AABB AABBTree;

        //Unknown members
        private uint unknown1;
        public Tuple<float, float, float> positon;

        #endregion Members

        #region Properties

        /// <summary>
        /// Name of the room.
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// List of vertices contained in this walkmesh.
        /// </summary>
        public List<Vert> Verts => Vertices;

        /// <summary>
        /// List of faces (triangles) that form the walkmesh.
        /// </summary>
        //public IReadOnlyList<Face> Faces => Faces;

        /// <summary>
        /// Walkmesh type header information.
        /// </summary>
        public int WalkmeshType { get; private set; }

        /// <summary>
        /// Minimum X coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinX { get; private set; } = float.MaxValue;

        /// <summary>
        /// Minimum Y coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinY { get; private set; } = float.MaxValue;

        /// <summary>
        /// Minimum Z coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinZ { get; private set; } = float.MaxValue;

        /// <summary>
        /// Maximum X coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxX { get; private set; } = float.MinValue;

        /// <summary>
        /// Maximum Y coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxY { get; private set; } = float.MinValue;

        /// <summary>
        /// Maximum Z coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxZ { get; private set; } = float.MinValue;

        /// <summary>
        /// Range of X values among the vertices in this walkmesh.
        /// </summary>
        public float RangeX => MinX == float.MaxValue ? 0f : MaxX - MinX;

        /// <summary>
        /// Range of Y values among the vertices in this walkmesh.
        /// </summary>
        public float RangeY => MinY == float.MaxValue ? 0f : MaxY - MinY;

        /// <summary>
        /// Range of Z values among the vertices in this walkmesh.
        /// </summary>
        public float RangeZ => MinZ == float.MaxValue ? 0f : MaxZ - MinZ;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Parse raw byte data as a walkmesh file.
        /// </summary>
        public WOK(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        public WOK(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Parse walkmesh file from stream.
        /// </summary>
        public WOK(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //Header
                string version = new string(br.ReadChars(8));
                if (version != "BWM V1.0")
                {
                    Console.WriteLine($"UNSUPPORTED WALKMESH FORMAT: {version}");
                    return;
                }
                if (br.ReadUInt32() == 0) Console.WriteLine("FILE INDICATES MESH IS FOR DOORS OR PLACABLES");

                br.ReadBytes(48); //Reserved
                positon = new Tuple<float, float, float>(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()); //Apparently does nothing?

                uint vertexCount = br.ReadUInt32();
                uint vertexOffset = br.ReadUInt32();
                uint faceCount = br.ReadUInt32();
                uint faceOffset = br.ReadUInt32();
                uint faceMatOffset = br.ReadUInt32();
                uint faceNormalsOffset = br.ReadUInt32();
                uint faceCoefsOffset = br.ReadUInt32();
                uint AABBCount = br.ReadUInt32();
                uint AABBOffset = br.ReadUInt32();
                unknown1 = br.ReadUInt32(); //Unknown value - Gee I hope it isn't important
                uint faceAdjCount = br.ReadUInt32(); //Equal to the number of walkable faces
                uint faceAdjOffset = br.ReadUInt32();
                uint edgesCount = br.ReadUInt32();
                uint edgesOffset = br.ReadUInt32();
                uint perimCount = br.ReadUInt32();
                uint perimOffset = br.ReadUInt32();

                //Get Vertices
                br.BaseStream.Seek(vertexOffset, SeekOrigin.Begin);
                for (uint i = 0; i < vertexCount; i++)
                {
                    var v = readVertex(br);
                    Vertices.Add(v);
                    UpdateMinMax(v);
                }

                //Get Faces
                br.BaseStream.Seek(faceOffset, SeekOrigin.Begin);
                for (uint i = 0; i < faceCount; i++) Faces.Add(readFace(br));

                //Get Face Material Type
                br.BaseStream.Seek(faceMatOffset, SeekOrigin.Begin);
                for (int i = 0; i < faceCount; i++) Faces[i].SurfaceMaterial = (SurfaceMaterial)br.ReadInt32();

                //Get Face Normals
                br.BaseStream.Seek(faceNormalsOffset, SeekOrigin.Begin);
                for (int i = 0; i < faceCount; i++) Faces[i].norm = new Tuple<float, float, float>(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                //Get Face Plane Coeficients
                br.BaseStream.Seek(faceCoefsOffset, SeekOrigin.Begin);
                for (int i = 0; i < faceCount; i++) Faces[i].coef = br.ReadSingle();

                //Get Face Adjacencies
                br.BaseStream.Seek(faceAdjOffset, SeekOrigin.Begin);
                Faces.Where(f => f.IsWalkable).ToList().ForEach(f => f.grabEdges(br));

                //Get Perimeter Edges
                br.BaseStream.Seek(edgesOffset, SeekOrigin.Begin);
                for (int i = 0; i < edgesCount; i++) Edges[br.ReadInt32()] = br.ReadInt32();

                //Get Perimeters
                br.BaseStream.Seek(perimOffset, SeekOrigin.Begin);
                for (int i = 0; i < perimCount; i++) Perimeters.Add(br.ReadInt32());

                //AABB Tree
                if (AABBCount > 0) AABBTree = getAABB(br, AABBOffset, AABBOffset);
                else AABBTree = null;

            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Performs a translation transform on the entire walkmesh
        /// </summary>
        /// <param name="delta_x">change along X axis</param>
        /// <param name="delta_y">change along Y axis</param>
        /// <param name="delta_z">change along Z axis</param>
        public void translate(float delta_x, float delta_y, float delta_z)
        {
            foreach (var v in Vertices)
            {
                v.X += delta_x;
                v.Y += delta_y;
                v.Z += delta_z;
            }

            translateAABB(AABBTree, delta_x, delta_y, delta_z);

        }

        /// <summary>
        /// Determines if the given x and y coordinate pair is contained within a
        /// walkable face projected into two-dimensional space. The Z axis is ignored
        /// for this calculation.
        /// </summary>
        public bool ContainsWalkablePoint(float x, float y)
        {
            // Return false if point is outside of bounds.
            if (x < MinX || x > MaxX || y < MinY || y > MaxY) return false;

            // Check if any walkable face contains point.
            return Faces.Where(f => f.IsWalkable).Any(f => f.ContainsPoint2D(x, y));
        }

        /// <summary>
        /// Determines if the given x and y coordinate pair is contained within a
        /// non-walkable face projected into two-dimensional space. The Z axis is
        /// ignored for this calculation.
        /// </summary>
        public bool ContainsNonWalkablePoint(float x, float y)
        {
            // Return false if point is outside of bounds.
            if (x < MinX || x > MaxX || y < MinY || y > MaxY) return false;

            // Check if any walkable face contains point.
            return Faces.Where(f => !f.IsWalkable).Any(f => f.ContainsPoint2D(x, y));
        }

        /// <summary>
        /// Determines if the given x and y coordinate pair is contained within any
        /// face projected into two-dimensional space. The Z axis is ignored for
        /// this calculation.
        /// </summary>
        public bool ContainsPoint(float x, float y)
        {
            // Return false if point is outside of bounds.
            if (x < MinX || x > MaxX || y < MinY || y > MaxY) return false;

            // Check if any walkable face contains point.
            return Faces.Any(f => f.ContainsPoint2D(x, y));
        }

        /// <summary>
        /// Returns the face found at the given x and y coordinate pair within this
        /// walkmesh. If no face is found, null is returned. If multiple faces are
        /// found, the first walkable face is returned. If no walkable faces are
        /// found, the first non-walkable face is returned.
        /// </summary>
        public Face FaceAtPoint(float x, float y)
        {
            // Return null if point is outside of bounds.
            if (x < MinX || x > MaxX || y < MinY || y > MaxY) return null;

            // Find any faces that contain the point.
            var matching = Faces.Where(f => f.ContainsPoint2D(x, y));

            // If 0 or 1 faces, return the first or default (null).
            if (matching.Count() < 2) return matching.FirstOrDefault();

            // If more than 1, check for walkable faces.
            var walkmatch = matching.Where(f => f.IsWalkable);

            // If any walkable faces, return first.
            if (walkmatch.Any()) return walkmatch.First();

            // Else, return first of matching (a non-walkable face).
            return matching.First();
        }

        /// <summary>
        /// Use the given vertex to update the minimum and maximum coordinate values
        /// of this walkmesh if the vertex is outside of the walkmesh's current bounds.
        /// </summary>
        private void UpdateMinMax(Vert v)
        {
            // Update minimum values.
            if (v.X < MinX) MinX = v.X;
            if (v.Y < MinY) MinY = v.Y;
            if (v.Z < MinZ) MinZ = v.Z;
            
            // Update maximum values.
            if (v.X > MaxX) MaxX = v.X;
            if (v.Y > MaxY) MaxY = v.Y;
            if (v.Z > MaxZ) MaxZ = v.Z;
        }

        /// <summary>
        /// Generates a new Vetex using a binary reader
        /// </summary>
        /// <param name="br">BinaryReader sought to the position of a vertex in the BaseStream</param>
        /// <returns></returns>
        private Vert readVertex(BinaryReader br)
        {
            return new Vert
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle()
            };
        }

        /// <summary>
        /// Generates a new Face using a binary reader
        /// </summary>
        /// <param name="br">BinaryReader sought to the position of a face in the BaseStream</param>
        /// <returns></returns>
        private Face readFace(BinaryReader br)
        {
            return new Face
            {
                A = Vertices[(int)br.ReadUInt32()],
                B = Vertices[(int)br.ReadUInt32()],
                C = Vertices[(int)br.ReadUInt32()]
            };
        }

        /// <summary>
        /// Recursive read for AABB tree
        /// </summary>
        /// <param name="br">Binary Reader open to input file</param>
        /// <param name="offset">Offset of this node in the file</param>
        /// <param name="AABBOffset">Offset from the start of the file to the AABB tree</param>
        /// <returns></returns>
        private AABB getAABB(BinaryReader br, uint offset, uint AABBOffset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            AABB aabb = new AABB();
            aabb.BBmin = new Tuple<float, float, float>(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            aabb.BBmax = new Tuple<float, float, float>(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            aabb.faceIndex = br.ReadInt32();
            aabb.unknown = br.ReadInt32();
            aabb.plane = br.ReadInt32();

            //Check faceindex before doing this to make sure no children.
            if (aabb.faceIndex == -1)
            {
                uint leftoffset = br.ReadUInt32() * AABBNODESIZE + AABBOffset;
                uint rightoffset = br.ReadUInt32() * AABBNODESIZE + AABBOffset;

                aabb.left = getAABB(br, leftoffset, AABBOffset);
                aabb.right = getAABB(br, rightoffset, AABBOffset);
            }

            return aabb;
        }
        
        /// <summary>
        /// Recursive write for AABBs
        /// </summary>
        /// <param name="aabb">The Axis Align Bounding Box node</param>
        /// <param name="bw">A Binary writer open to the output fil</param>
        /// <param name="AABBOffset">Offset from the start of the file to the AABB tree</param>
        /// <param name="index">reference to the Index of this AABB node</param>
        private void writeAABB(AABB aabb, BinaryWriter bw, uint AABBOffset, ref uint index)
        {
            if (aabb == null) return;

            bw.BaseStream.Seek(index * AABBNODESIZE + AABBOffset, SeekOrigin.Begin);

            bw.Write(aabb.BBmin.Item1);
            bw.Write(aabb.BBmin.Item2);
            bw.Write(aabb.BBmin.Item3);
            bw.Write(aabb.BBmax.Item1);
            bw.Write(aabb.BBmax.Item2);
            bw.Write(aabb.BBmax.Item3);
            bw.Write(aabb.faceIndex);
            bw.Write(aabb.unknown);
            bw.Write(aabb.plane);

            //Check faceindex for children.
            if (aabb.faceIndex == -1)
            {
                index++;
                bw.Write(index);
                long pos = bw.BaseStream.Position;
                writeAABB(aabb.left, bw, AABBOffset, ref index);
                bw.BaseStream.Seek(pos, SeekOrigin.Begin);
                index++;
                bw.Write(index);
                writeAABB(aabb.right, bw, AABBOffset, ref index);
            }
            else
            {
                bw.Write(-1);
                bw.Write(-1);
            }
        }
        
        /// <summary>
        /// Recursively count AABB nodes in the provided tree
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        private int getAABBCount(AABB aabb)
        {
            if (aabb == null) return 0;
            else return 1 + getAABBCount(aabb.left) + getAABBCount(aabb.right);
        }

        /// <summary>
        /// Recusively translate AABB
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="delta_x"></param>
        /// <param name="delta_y"></param>
        /// <param name="delta_z"></param>
        private void translateAABB(AABB aabb, float delta_x, float delta_y, float delta_z)
        {
            if (aabb == null) return;

            aabb.BBmin = new Tuple<float, float, float>(aabb.BBmin.Item1 + delta_x, aabb.BBmin.Item2 + delta_y, aabb.BBmin.Item3 + delta_z);
            aabb.BBmax = new Tuple<float, float, float>(aabb.BBmax.Item1 + delta_x, aabb.BBmax.Item2 + delta_y, aabb.BBmax.Item3 + delta_z);

            translateAABB(aabb.left, delta_x, delta_y, delta_z);
            translateAABB(aabb.right, delta_x, delta_y, delta_z);
        }

        /// <summary>
        /// String representation of the walkmesh with a count of faces and vertices.
        /// </summary>
        public override string ToString()
        {
            return $"Faces: {Faces.Count}, Verts: {Verts.Count}, AABBs: {getAABBCount(AABBTree)}";
        }

        /// <summary>
        /// Writes walkmesh file data.
        /// </summary>
        internal override void Write(Stream s)
        {
            int AABBCount = getAABBCount(AABBTree);
            int walkableCount = Faces.Where(f => f.IsWalkable).Count();

            //Offset Calculations
            uint vertexOffset = DEFAULTVERTOFF;
            uint faceOffset = (uint)(vertexOffset + VERTEXSIZE * Vertices.Count());
            uint faceMatOffset = (uint)(faceOffset + FACESIZE * Faces.Count());
            uint faceNormalsOffset = (uint)(faceMatOffset + INT32SIZE * Faces.Count());
            uint faceCoefsOffset = (uint)(faceNormalsOffset + FACESIZE * Faces.Count());
            uint AABBOffset = (uint)(faceCoefsOffset + FLOATSIZE * Faces.Count());
            uint faceAdjOffset = (uint)(AABBOffset + AABBNODESIZE * AABBCount);
            uint edgesOffset = (uint)(faceAdjOffset + FACESIZE * walkableCount);
            uint perimOffset = (uint)(edgesOffset + EDGESIZE * Edges.Count());

            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write("BWM V1.0".ToCharArray());
                bw.Write((uint)1);
                bw.BaseStream.Seek(48, SeekOrigin.Current);
                bw.Write(positon.Item1);
                bw.Write(positon.Item2);
                bw.Write(positon.Item3);
                bw.Write(Vertices.Count());
                bw.Write(vertexOffset);
                bw.Write(Faces.Count());
                bw.Write(faceOffset);
                bw.Write(faceMatOffset);
                bw.Write(faceNormalsOffset);
                bw.Write(faceCoefsOffset);
                bw.Write(AABBCount);
                bw.Write(AABBOffset);
                bw.Write(unknown1);
                bw.Write(walkableCount);
                bw.Write(faceAdjOffset);
                bw.Write(Edges.Count());
                bw.Write(edgesOffset);
                bw.Write(Perimeters.Count());
                bw.Write(perimOffset);

                //Vertices
                bw.BaseStream.Seek(vertexOffset, SeekOrigin.Begin);
                foreach (Vert v in Vertices)
                {
                    bw.Write(v.X);
                    bw.Write(v.Y);
                    bw.Write(v.Z);
                }

                //Faces
                bw.BaseStream.Seek(faceOffset, SeekOrigin.Begin);
                foreach (Face f in Faces)
                {
                    bw.Write(Vertices.IndexOf(f.A)); //Consdier making this more efficient?
                    bw.Write(Vertices.IndexOf(f.B));
                    bw.Write(Vertices.IndexOf(f.C));
                }

                //Face Material Types
                bw.BaseStream.Seek(faceMatOffset, SeekOrigin.Begin);
                foreach (Face f in Faces)
                {
                    bw.Write((int)f.SurfaceMaterial);
                }

                //Face Normals
                bw.BaseStream.Seek(faceNormalsOffset, SeekOrigin.Begin);
                foreach (Face f in Faces)
                {
                    bw.Write(f.norm.Item1);
                    bw.Write(f.norm.Item2);
                    bw.Write(f.norm.Item3);
                }

                //Face Plane Coeficients
                bw.BaseStream.Seek(faceCoefsOffset, SeekOrigin.Begin);
                foreach (Face f in Faces)
                {
                    bw.Write(f.coef);
                }

                //Face Adjacencies
                bw.BaseStream.Seek(faceAdjOffset, SeekOrigin.Begin);
                foreach (Face f in Faces.Where(f => f.IsWalkable))
                {
                    bw.Write(f.e1);
                    bw.Write(f.e2);
                    bw.Write(f.e3);
                }

                //Perimeter Edges
                bw.BaseStream.Seek(edgesOffset, SeekOrigin.Begin);
                foreach (var e in Edges)
                {
                    bw.Write(e.Key);
                    bw.Write(e.Value);
                }

                //Perimeters
                bw.BaseStream.Seek(perimOffset, SeekOrigin.Begin);
                foreach (var p in Perimeters)
                {
                    bw.Write(p);
                }

                //AABB Node Tree
                bw.BaseStream.Seek(AABBOffset, SeekOrigin.Begin);
                uint index = 0;
                writeAABB(AABBTree, bw, AABBOffset, ref index);

            }
        }

        #endregion Methods

        #region Nested Classes

        /// <summary>
        /// Three-dimensional coordinate representing a vertex of a walkmesh face.
        /// </summary>
        public class Vert
        {
            /// <summary> X coordinate </summary>
            public float X { get; set; }
            /// <summary> Y coordinate </summary>
            public float Y { get; set; }
            /// <summary> Z coordinate </summary>
            public float Z { get; set; }

            /// <summary>
            /// String representation of the vertex as "[X, Y, Z]"
            /// </summary>
            public override string ToString()
            {
                return $"[{X}, {Y}, {Z}]";
            }
        }
        
        /// <summary>
        /// Triangular face of the walkmesh.
        /// </summary>
        public class Face
        {
            /// <summary>
            /// First vertex of the face.
            /// </summary>
            public Vert A { get; set; }

            /// <summary>
            /// Second vertex of the face.
            /// </summary>
            public Vert B { get; set; }

            /// <summary>
            /// Third vertex of the face.
            /// </summary>
            public Vert C { get; set; }

            /// <summary>
            /// The surface material of this face.
            /// </summary>
            public SurfaceMaterial SurfaceMaterial { get; set; }

            //Edge IDs
            internal int e1 = -1;
            internal int e2 = -1;
            internal int e3 = -1;

            internal Tuple<float, float, float> norm;

            internal float coef;

            internal void grabEdges(BinaryReader br)
            {
                e1 = br.ReadInt32();
                e2 = br.ReadInt32();
                e3 = br.ReadInt32();
            }

            /// <summary>
            /// Is this face walkable?
            /// </summary>
            public bool IsWalkable => SurfaceMaterial.IsWalkable();

            /// <summary>
            /// Calculates the area of this face.
            /// </summary>
            public float Area()
            {
                return AreaOfTriangle(A, B, C);
            }

            /// <summary>
            /// Calculates the area of a triangle given its three vertices.
            /// </summary>
            public static float AreaOfTriangle(Vert a, Vert b, Vert c)
            {
                return Math.Abs(
                    (a.X * (b.Y - c.Y)) +
                    (b.X * (c.Y - a.Y)) +
                    (c.X * (a.Y - b.Y))
                ) / 2f;
            }

            /// <summary>
            /// Determines if the given x and y coordinate pair is contained within
            /// this face projected into two-dimensional space. The Z axis is ignored
            /// for this calculation.
            /// </summary>
            public bool ContainsPoint2D(float x, float y)
            {
                var p = new Vert { X = x, Y = y, Z = 0 };

                // Calculate the face's area.
                var abc = Area();

                // If this face has an area of 0, it contains nothing.
                if (abc <= 0.0001) return false;

                // Calculate the area of the three other triangles made with p and this face's vertices.
                var pab = AreaOfTriangle(p, A, B);
                var pbc = AreaOfTriangle(p, B, C);
                var pac = AreaOfTriangle(p, A, C);

                // If p lies inside this face, the sum of the three areas will equal the face's area.
                return Math.Abs(abc - (pab + pbc + pac)) <= 0.0001;
            }

            /// <summary>
            /// String representation of this face as
            /// "SurfaceMaterial, [Ax, Ay, Az], [Bx, By, Bz], [Cx, Cy, Cz]".
            /// </summary>
            public override string ToString()
            {
                return $"{SurfaceMaterial}, {A}, {B}, {C}";
            }
        }
        
        /// <summary>
        ///An axis-aligned bounding box.
        /// </summary>
        public class AABB 
        {
            internal Tuple<float, float, float> BBmin; //Minimum bounding box
            internal Tuple<float, float, float> BBmax; //Maximum bounding box

            internal int faceIndex = -1; //Will be -1 unless this node has children

            internal int unknown = 4; //Not, I, the wiki, nor Xoreos are sure what this value does, but it's usually 4

            internal int plane = 0; //The most significant plane 0:None, 1:X, 2:Y, 4:Z. Will be 0 if faceIndex = -1

            internal AABB left = null; //Child nodes for the tree structure
            internal AABB right = null;
        }
        

        #endregion Nested Classes
    }
}
