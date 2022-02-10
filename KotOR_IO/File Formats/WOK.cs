using System;
using System.Collections.Generic;
using System.IO;

namespace KotOR_IO
{
    /// <summary>
    /// Kotor walkmesh file format.
    /// </summary>
    public class WOK
    {
        #region Private Members

        private int vert_count, face_count;
        private float minX, minY, minZ, maxX, maxY, maxZ;

        #endregion Private Members

        #region Properties

        /// <summary>
        /// List of vertices contained in this walkmesh.
        /// </summary>
        public List<Vert> Verts { get; set; } = new List<Vert>();

        /// <summary>
        /// List of faces (triangles) that form the walkmesh.
        /// </summary>
        public List<Face> Faces { get; set; } = new List<Face>();


        /// <summary>
        /// Minimum X coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinX => minX;

        /// <summary>
        /// Minimum Y coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinY => minY;

        /// <summary>
        /// Minimum Z coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MinZ => minZ;

        /// <summary>
        /// Maximum X coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxX => maxX;

        /// <summary>
        /// Maximum Y coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxY => maxY;

        /// <summary>
        /// Maximum Z coordinate among the vertices in this walkmesh.
        /// </summary>
        public float MaxZ => maxZ;

        /// <summary>
        /// Range of X values among the vertices in this walkmesh.
        /// </summary>
        public float RangeX => minX == float.MaxValue ? 0f : maxX - minX;

        /// <summary>
        /// Range of Y values among the vertices in this walkmesh.
        /// </summary>
        public float RangeY => minY == float.MaxValue ? 0f : maxY - minY;

        /// <summary>
        /// Range of Z values among the vertices in this walkmesh.
        /// </summary>
        public float RangeZ => minZ == float.MaxValue ? 0f : maxZ - minZ;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an empty walkmesh object.
        /// </summary>
        public WOK()
        {
            minX = minY = minZ = float.MaxValue;
            maxX = maxY = maxZ = float.MinValue;
        }

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
            : this()
        {
            using (var br = new BinaryReader(s))
            {
                // Skip header.
                _ = br.ReadBytes(72);

                // Read counts and offsets.
                vert_count = br.ReadInt32();
                var vert_offset = br.ReadInt32();
                face_count = br.ReadInt32();
                var face_offset = br.ReadInt32();
                var walk_offset = br.ReadInt32();

                // Read verts.
                _ = br.BaseStream.Seek(vert_offset, SeekOrigin.Begin);
                for (var i = 0; i < vert_count; i++)
                {
                    // Vertices are stored as a set of three floats.
                    var v = new Vert
                    {
                        X = br.ReadSingle(),
                        Y = br.ReadSingle(),
                        Z = br.ReadSingle()
                    };
                    Verts.Add(v);
                    UpdateMinMax(v);
                }

                // Read faces.
                _ = br.BaseStream.Seek(face_offset, SeekOrigin.Begin);
                for (var i = 0; i < face_count; i++)
                {
                    // Vertices are stored as an index to the vertex array.
                    // Walkable will be set later. Assume true for now.
                    Faces.Add(new Face
                    {
                        A = Verts[br.ReadInt32()],
                        B = Verts[br.ReadInt32()],
                        C = Verts[br.ReadInt32()],
                        Walkable = true
                    });
                }

                // Read walkable.
                _ = br.BaseStream.Seek(walk_offset, SeekOrigin.Begin);
                foreach (var f in Faces)
                {
                    var temp = br.ReadInt32();
                    f.Walkable = temp != 7 && temp != 19;
                }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Use the given vertex to update the minimum and maximum coordinate values of this walkmesh
        /// if the vertex is outside of the walkmesh's current bounds.
        /// </summary>
        private void UpdateMinMax(Vert v)
        {
            // Update minimum values.
            if (v.X < minX) minX = v.X;
            if (v.Y < minY) minY = v.Y;
            if (v.Z < minZ) minZ = v.Z;

            // Update maximum values.
            if (v.X > maxX) maxX = v.X;
            if (v.Y > maxY) maxY = v.Y;
            if (v.Z > maxZ) maxZ = v.Z;
        }

        /// <summary>
        /// Use the given walkmesh to update the minimum and maximum coordinate values if the other
        /// walkmesh is outside of this object's current bounds.
        /// </summary>
        private void UpdateMinMax(WOK other)
        {
            // Update minimum values.
            if (other.minX < minX) minX = other.minX;
            if (other.minY < minY) minY = other.minY;
            if (other.minZ < minZ) minZ = other.minZ;

            // Update maximum values.
            if (other.maxX > maxX) maxX = other.maxX;
            if (other.maxY > maxY) maxY = other.maxY;
            if (other.maxZ > maxZ) maxZ = other.maxZ;
        }

        /// <summary>
        /// Adds a walkmesh's vertices and faces to this walkmesh.
        /// </summary>
        public void Add(WOK other)
        {
            if (other is null) return;
            if (ReferenceEquals(this, other)) return;

            vert_count += other.vert_count;
            face_count += other.face_count;

            Verts.AddRange(other.Verts);
            Faces.AddRange(other.Faces);

            UpdateMinMax(other);
        }

        /// <summary>
        /// String representation of the walkmesh with a count of faces and vertices.
        /// </summary>
        public override string ToString()
        {
            return $"Faces: {Faces.Count}, Verts: {Verts.Count}";
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
            /// <summary> First vertex of the face. </summary>
            public Vert A { get; set; }
            /// <summary> Second vertex of the face. </summary>
            public Vert B { get; set; }
            /// <summary> Third vertex of the face. </summary>
            public Vert C { get; set; }
            /// <summary> Is this face walkable? </summary>
            public bool Walkable { get; set; }

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
            /// Determines if the given x and y coordinate pair is contained within this face projected
            /// into two-dimensional space. The Z axis is ignored for this calculation.
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
            /// String representation of this face as "Walkable?, [Ax, Ay, Az], [Bx, By, Bz], [Cx, Cy, Cz]".
            /// </summary>
            public override string ToString()
            {
                return $"{Walkable}, {A}, {B}, {C}";
            }
        }

        #endregion Nested Classes
    }
}
