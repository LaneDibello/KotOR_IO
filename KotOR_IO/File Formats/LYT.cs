using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// Kotor module layout file format.
    /// </summary>
    public class LYT
    {
        #region Properties

        /// <summary>
        /// FileDependancy string from the layout file.
        /// </summary>
        public string FileDependancy { get; set; }

        /// <summary>
        /// List of the rooms in this layout.
        /// </summary>
        public List<Room> Rooms { get; set; } = new List<Room>();

        /// <summary>
        /// List of the tracks in this layout.
        /// </summary>
        public List<ArtPlaceable> Tracks { get; set; } = new List<ArtPlaceable>();

        /// <summary>
        /// List of obstacles in this layout.
        /// </summary>
        public List<ArtPlaceable> Obstacles { get; set; } = new List<ArtPlaceable>();

        /// <summary>
        /// List of door hooks in this layout.
        /// </summary>
        public List<DoorHook> DoorHooks { get; set; } = new List<DoorHook>();

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Parse raw byte data as a layout file.
        /// </summary>
        /// <param name="rawData">Byte array to parse.</param>
        public LYT(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Parse layout file from stream.
        /// </summary>
        /// <param name="s">Stream to parse.</param>
        public LYT(Stream s)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                // Read stream as plain text, then split it into individual lines of text.
                string content = sr.ReadToEnd();
                var lines = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                ParsingCategory category = ParsingCategory.None;

                // Parse each line.
                foreach (var line in lines)
                {
                    var split = line.Trim().Split(' ');

                    switch (split[0])
                    {
                        case "#MAXLAYOUT":
                        case "beginlayout":
                        case "donelayout":
                            category = ParsingCategory.None;
                            continue;   // Skip these lines.
                        case "filedependancy":
                            category = ParsingCategory.None;
                            FileDependancy = split.Last();
                            continue;
                        case "roomcount":
                            category = ParsingCategory.Room;
                            continue;   // Set default action to add to rooms.
                        case "trackcount":
                            category = ParsingCategory.Track;
                            continue;   // Set default action to add to tracks.
                        case "obstaclecount":
                            category = ParsingCategory.Obstacle;
                            continue;   // Set default action to add to obstacles.
                        case "doorhookcount":
                            category = ParsingCategory.DoorHook;
                            continue;   // Set default action to add to doorhooks.
                        default:
                            break;
                    }

                    // Add to the appropriate list, and continue to the next line.
                    switch (category)
                    {
                        case ParsingCategory.Room:
                            if (split.Count() != 4)
                                throw new Exception($"LYT(): Invalid token count '{split.Count()}' for object 'Room'");
                            Rooms.Add(new Room
                            {
                                Name = split[0],
                                X = float.Parse(split[1]),
                                Y = float.Parse(split[2]),
                                Z = float.Parse(split[3]),
                            });
                            break;
                        case ParsingCategory.Track:
                            if (split.Count() != 4)
                                throw new Exception($"LYT(): Invalid token count '{split.Count()}' for object 'Track'");
                            Tracks.Add(new ArtPlaceable
                            {
                                Model = split[0],
                                X = float.Parse(split[1]),
                                Y = float.Parse(split[2]),
                                Z = float.Parse(split[3]),
                            });
                            break;
                        case ParsingCategory.Obstacle:
                            if (split.Count() != 4)
                                throw new Exception($"LYT(): Invalid token count '{split.Count()}' for object 'Obstacle'");
                            Obstacles.Add(new ArtPlaceable
                            {
                                Model = split[0],
                                X = float.Parse(split[1]),
                                Y = float.Parse(split[2]),
                                Z = float.Parse(split[3]),
                            });
                            break;
                        case ParsingCategory.DoorHook:
                            if (split.Count() != 10)
                                throw new Exception($"LYT(): Invalid token count '{split.Count()}' for object 'DoorHook'");
                            DoorHooks.Add(new DoorHook
                            {
                                Room = split[0],
                                Name = split[1],
                                Unk1 = float.Parse(split[2]),
                                X = float.Parse(split[3]),
                                Y = float.Parse(split[4]),
                                Z = float.Parse(split[5]),
                                Unk2 = float.Parse(split[6]),
                                Unk3 = float.Parse(split[7]),
                                Unk4 = float.Parse(split[8]),
                                Unk5 = float.Parse(split[9]),
                            });
                            break;
                        case ParsingCategory.None:
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a copy of the given layout.
        /// </summary>
        public LYT(LYT other)
        {
            foreach (var room in other.Rooms)
                Rooms.Add(new Room(room));

            foreach (var track in other.Tracks)
                Tracks.Add(new ArtPlaceable(track));

            foreach (var obstacle in other.Obstacles)
                Obstacles.Add(new ArtPlaceable(obstacle));

            foreach (var door in other.DoorHooks)
                DoorHooks.Add(new DoorHook(door));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Write layout data to stream.
        /// </summary>
        public void Write(Stream s)
        {
            using (StreamWriter sw = new StreamWriter(s))
            {
                sw.WriteLine("#MAXLAYOUT ASCII");
                sw.WriteLine($"filedependancy {FileDependancy}");
                sw.WriteLine("beginlayout");

                sw.WriteLine($"   roomcount {Rooms.Count}");
                foreach (var room in Rooms)
                    sw.WriteLine($"      {room}");

                sw.WriteLine($"   trackcount {Tracks.Count}");
                foreach (var track in Tracks)
                    sw.WriteLine($"      {track}");

                sw.WriteLine($"   obstaclecount {Obstacles.Count}");
                foreach (var obstacle in Obstacles)
                    sw.WriteLine($"      {obstacle}");

                sw.WriteLine($"   doorhookcount {DoorHooks.Count}");
                foreach (var doorhook in DoorHooks)
                    sw.WriteLine($"      {doorhook}");

                sw.WriteLine("donelayout");
            }
        }

        /// <summary>
        /// Write layout data to file.
        /// </summary>
        public void WriteToFile(string path)
        {
            Write(File.OpenWrite(path));
        }

        /// <summary>
        /// Get layout data as a byte array.
        /// </summary>
        public byte[] ToRawData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Convert layout object to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Rooms: {Rooms.Count}, Tracks: {Tracks.Count}, Obstacles: {Obstacles.Count}, DoorHooks: {DoorHooks.Count}";
        }

        #endregion Methods

        #region Nested Classes

        /// <summary>
        /// Categories used during parsing of this file format.
        /// </summary>
        private enum ParsingCategory
        {
            None = 0,
            Room,
            Track,
            Obstacle,
            DoorHook,
        }

        /// <summary>
        /// Representation of a rendering room within a module.
        /// </summary>
        public class Room
        {
            /// <summary>
            /// Name of this room.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z coordinate.
            /// </summary>
            public float Z { get; set; }

            /// <summary>
            /// Constructs an empty object.
            /// </summary>
            public Room() { }

            /// <summary>
            /// Creates a copy of a room object.
            /// </summary>
            public Room(Room other)
            {
                Name = other.Name;
                X = other.X;
                Y = other.Y;
                Z = other.Z;
            }

            /// <summary>
            /// String representation of a room as "Name X Y Z".
            /// </summary>
            public override string ToString()
            {
                return $"{Name} {X} {Y} {Z}";
            }
        }

        /// <summary>
        /// Placeable object within a module. Typically used for tracks and obstacles.
        /// </summary>
        public class ArtPlaceable
        {
            /// <summary>
            /// Name of this placeable.
            /// </summary>
            public string Model { get; set; }

            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z coordinate.
            /// </summary>
            public float Z { get; set; }

            /// <summary>
            /// Creates an empty object.
            /// </summary>
            public ArtPlaceable() { }

            /// <summary>
            /// Create a copy of an art placeable object.
            /// </summary>
            public ArtPlaceable(ArtPlaceable other)
            {
                Model = other.Model;
                X = other.X;
                Y = other.Y;
                Z = other.Z;
            }

            /// <summary>
            /// String representation of an art placeable as "Model X Y Z".
            /// </summary>
            public override string ToString()
            {
                return $"{Model} {X} {Y} {Z}";
            }
        }

        /// <summary>
        /// DoorHook object within a module.
        /// </summary>
        public class DoorHook
        {
            /// <summary>
            /// Name of the room that contains this door hook.
            /// </summary>
            public string Room { get; set; }

            /// <summary>
            /// Name of this door hook.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z coordinate.
            /// </summary>
            public float Z { get; set; }

            /// <summary>
            /// Unknown float value.
            /// </summary>
            public float Unk1 { get; set; }

            /// <summary>
            /// Unknown float value.
            /// </summary>
            public float Unk2 { get; set; }

            /// <summary>
            /// Unknown float value.
            /// </summary>
            public float Unk3 { get; set; }

            /// <summary>
            /// Unknown float value.
            /// </summary>
            public float Unk4 { get; set; }

            /// <summary>
            /// Unknown float value.
            /// </summary>
            public float Unk5 { get; set; }

            /// <summary>
            /// Creates an empty DoorHook object.
            /// </summary>
            public DoorHook() { }

            /// <summary>
            /// Creates a copy of a DoorHook object.
            /// </summary>
            public DoorHook(DoorHook other)
            {
                Room = other.Room;
                Name = other.Name;
                X = other.X;
                Y = other.Y;
                Z = other.Z;
                Unk1 = other.Unk1;
                Unk2 = other.Unk2;
                Unk3 = other.Unk3;
                Unk4 = other.Unk4;
                Unk5 = other.Unk5;
            }

            /// <summary>
            /// String representation of a DoorHook as "Room Name Unk1 X Y Z Unk2 Unk3 Unk4 Unk5".
            /// </summary>
            public override string ToString()
            {
                return $"{Room} {Name} {Unk1} {X} {Y} {Z} {Unk2} {Unk3} {Unk4} {Unk5}";
            }
        }

        #endregion Nested Classes
    }
}
