using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KotOR_IO.File_Formats
{
    /// <summary>
    /// Bioware Aurora Model Walkmesh File.
    /// </summary>
    /// <remarks>
    /// <para/> Any model with collsion has a walkmesh. The walkmesh just keeps track of a bunch of triangles each with a different surface type, that determines how the player should interact with it when they come into contact.
    /// </remarks>
    class WOK : KFile
    {
        #region Base Methods
        public WOK(byte[] rawdata)
        {

        }

        public WOK(string path)
        {

        }

        protected WOK(Stream s)
        {

        }

        internal override void Write(Stream s)
        {

        }
        #endregion

        #region class specific methods

        public bool isAreaMesh() { return MeshType == 1; }

        #endregion

        #region Structs
        //Vertice of a triangle thta forms a face
        struct Vert
        {
            float x;
            float y;
            float z;

            Vert(float X, float Y, float Z)
            {
                x = X; y = Y; z = Z;
            }
        }
        //A colliding Face
        struct Face
        {
            //The 3 vertexes that make up teh triangular plane
            Vert a;
            Vert b;
            Vert c;
            //The Collsion type for this plane
            Walk_Types WalkType;
            //Normal vector for this plane (perhaps the direction the player is pushed from it?)
            Tuple<float, float, float> Normal;
            //Each Face has one... no idea
            float PlaneCoefficient;

            Face(Vert A, Vert B, Vert C, Walk_Types W, Tuple<float, float, float> N, float PC)
            {
                a = A; b = B; c = C; WalkType = W; Normal = N; PlaneCoefficient = PC;
            }
        }
        //Bounding Box Node
        struct AABB
        {
            Tuple<float, float, float> Min_Bound_Box;
            Tuple<float, float, float> Max_Bound_Box;
            int Face_Index;
            int Unknown;
            int Most_Significant_Plane;
            int Left_Child;
            int Right_Child;

            AABB(Tuple<float, float, float> minBB, Tuple<float, float, float> maxBB, int Face, int MSP, int left, int right, int unk= 4,)
            {
                Min_Bound_Box = minBB;
                Max_Bound_Box = maxBB;
                Face_Index = Face;
                Unknown = unk;
                Most_Significant_Plane = MSP;
                Left_Child = left;
                Right_Child = right;
            }
        }
        //Tracks edges of walkable faces connecting to other walkable faces
        struct Adjacency
        {
            int index1;
            int index2;
            int index3;

            Adjacency(int i1, int i2, int i3)
            {
                index1 = i1; index2 = i2; index3 = i3;
            }
        }
        //References those edges from Adjacencies
        struct Edge
        {
            int Index;
            int Transition;

            Edge(int i, int t)
            {
                Index = i; Transition = t;
            }
        }
        //Represents an "island" of faces that are all connected, by the "last" edge of that perimeter.
        struct Perimeter
        {
            int edge_index;

            Perimeter(int i)
            {
                edge_index = i;
            }
        }
        #endregion

        #region dynamic members

        /// <summary>
        /// The Type of WOK file. 1 for areas, 0 for placables and doors.
        /// </summary>
        private int MeshType;

        /// <summary>
        /// Unused byte block, likely for backwards compatability. Left public in the event it becomes important
        /// </summary>
        public byte[] Reserved;

        /// <summary>
        /// An x,y,z position for the walkmesh. This doesn't appear to do anything in KotOR, so it is left public just in case.
        /// </summary>
        public Tuple<float, float, float> Position;

        /// <summary>
        /// Yet another chunk of unknown data (better get used to this)
        /// </summary>
        int Unknown;

        /// <summary>
        /// Collection of all triangular vertices
        /// </summary>
        List<Vert> Vertices;
        /// <summary>
        /// Collection of all colliding faces, and their walktypes, Normals, and more.
        /// </summary>
        List<Face> Faces;
        /// <summary>
        /// Tree of Bounding Box Nodes (Once I fully understand these I might reorganize them into an actual tree structure, but for now this is justa list)
        /// </summary>
        List<AABB> BBNodeTree;
        /// <summary>
        /// Collection of Adjacencies (another thing I don't fully understand)
        /// </summary>
        List<Adjacency> Adjacenies;
        /// <summary>
        /// Collection of Edges that connect to walkable faces.
        /// </summary>
        List<Edge> Edges;
        /// <summary>
        /// Collection of indevidual isolated walk areas.
        /// </summary>
        List<Perimeter> Perimeters;
        #endregion

        public enum Walk_Types
        {
            Dirt,
            Obscuring,
            Grass,
            Stone,
            Wood,
            Water,
            Non_walk,
            Transparent,
            Carpet,
            Metal,
            Puddles,
            Swamp,
            Mud,
            Leaves,
            Lava,
            BottomlessPit,
            DeepWater,
            Door,
            NonWalkGrass,
        }


    }
}
