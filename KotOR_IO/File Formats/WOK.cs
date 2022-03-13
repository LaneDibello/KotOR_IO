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
        #region Private Members

        private readonly byte[] rawFileData;
        private readonly List<Vert> verts = new List<Vert>();
        private readonly List<Face> faces = new List<Face>();

        #endregion Private Members

        #region Properties

        /// <summary>
        /// Name of the room.
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// List of vertices contained in this walkmesh.
        /// </summary>
        public IReadOnlyList<Vert> Verts => verts;

        /// <summary>
        /// List of faces (triangles) that form the walkmesh.
        /// </summary>
        public IReadOnlyList<Face> Faces => faces;

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

        /// <summary>
        /// Parse walkmesh file from stream.
        /// </summary>
        public WOK(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                rawFileData = br.ReadAllBytes();            // Save the entire file as a byte[].
                br.BaseStream.Seek(0, SeekOrigin.Begin);    // Return to the beginning.

                /*
                 *  Header Format (72 bytes total):
                 *       byte[8] - "BWM V1.0"
                 *       4 bytes - walkmesh type
                 *      60 bytes - unknown / unused
                 */
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                WalkmeshType = br.ReadInt32();
                _ = br.ReadBytes(60);   // Skip 60 bytes.

                /*
                 *  Counts and Offsets (all ints, 56 bytes total):
                 *     1. Vertex Count
                 *     2. Vertex Offset
                 *     3. Face Count
                 *     4. Face Offset
                 *     5. Face Type Offset
                 */
                var vertCount = br.ReadInt32();
                var vertOffset = br.ReadInt32();
                var faceCount = br.ReadInt32();
                var faceOffset = br.ReadInt32();
                var walkOffset = br.ReadInt32();

                /*
                 *  The remaining items in this section are ignored for now.
                 *     6. Unknown int
                 *     7. Unknown int
                 *     8. AABB Count
                 *     9. AABB Offset
                 *    10. Unknown int
                 *    11. Face Adj Count
                 *    12. Face Adj Offset
                 *    13. Perim Edges Count
                 *    14. Perim Edges Offset
                 */

                // Read verts.
                _ = br.BaseStream.Seek(vertOffset, SeekOrigin.Begin);
                for (var i = 0; i < vertCount; i++)
                {
                    // Vertices are stored as a set of three floats.
                    var v = new Vert
                    {
                        X = br.ReadSingle(),
                        Y = br.ReadSingle(),
                        Z = br.ReadSingle()
                    };
                    verts.Add(v);
                    UpdateMinMax(v);
                }

                // Read faces.
                _ = br.BaseStream.Seek(faceOffset, SeekOrigin.Begin);
                for (var i = 0; i < faceCount; i++)
                {
                    // Vertices are stored as an index to the vertex array.
                    // Surface material will be set later.
                    faces.Add(new Face
                    {
                        A = Verts[br.ReadInt32()],
                        B = Verts[br.ReadInt32()],
                        C = Verts[br.ReadInt32()],
                        SurfaceMaterial = SurfaceMaterial.NotDefined
                    });
                }

                // Read surface material.
                _ = br.BaseStream.Seek(walkOffset, SeekOrigin.Begin);
                foreach (Face f in Faces)
                {
                    f.SurfaceMaterial = (SurfaceMaterial)br.ReadInt32();
                }
            }
        }

        #endregion Constructors

        #region Methods

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
            return faces.Where(f => f.IsWalkable).Any(f => f.ContainsPoint2D(x, y));
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
            return faces.Where(f => !f.IsWalkable).Any(f => f.ContainsPoint2D(x, y));
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
            return faces.Any(f => f.ContainsPoint2D(x, y));
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
            var matching = faces.Where(f => f.ContainsPoint2D(x, y));

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
        /// String representation of the walkmesh with a count of faces and vertices.
        /// </summary>
        public override string ToString()
        {
            return $"Faces: {Faces.Count}, Verts: {Verts.Count}";
        }

        /// <summary>
        /// Writes walkmesh file data.
        /// </summary>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                bw.Write(rawFileData);
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

        #endregion Nested Classes
    }
}
