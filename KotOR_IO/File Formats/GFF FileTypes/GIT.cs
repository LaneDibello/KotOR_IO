using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.GffFile
{
    public class GIT : GffDerivative
    {
        #region Consts
        private const string AREA_PROPERTIES = "AreaProperties";
        private const string CAMERA_LIST = "CameraList";
        private const string CREATURE_LIST = "Creature List";
        private const string DOOR_LIST = "Door List";
        private const string ENCOUNTER_LIST = "Encounter List";
        private const string LIST = "List";
        private const string PLACEABLE_LIST = "Placeable List";
        private const string SOUND_LIST = "SoundList";
        private const string STORE_LIST = "StoreList";
        private const string TRIGGER_LIST = "TriggerList";
        private const string USE_TEMPLATES = "UseTemplates";
        private const string WAYPOINT_LIST = "WaypointList";
        #endregion

        #region Properties
        public GFF.STRUCT AreaProperties { get; set; }
        public GFF.LIST Cameras { get; set; }
        public GFF.LIST Creatures { get; set; }
        public GFF.LIST Doors { get; set; }
        public GFF.LIST Encounters { get; set; }
        public GFF.LIST List { get; set; }
        public IEnumerable<Placeable> Placeables { get; set; }
        public GFF.LIST Sounds { get; set; }
        public GFF.LIST Stores { get; set; }
        public GFF.LIST Triggers { get; set; }
        public GFF.BYTE UseTemplates { get; set; }
        public GFF.LIST Waypoints { get; set; }
        #endregion

        #region Constructors

        private GIT(GFF gff)
            : base(gff)
        {
            AreaProperties = GFF.Top_Level.Fields.FirstOrDefault(f => f.Label == AREA_PROPERTIES) as GFF.STRUCT;
            Cameras = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == CAMERA_LIST) as GFF.LIST;
            Creatures = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == CREATURE_LIST) as GFF.LIST;
            Doors = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == DOOR_LIST) as GFF.LIST;
            Encounters = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == ENCOUNTER_LIST) as GFF.LIST;
            List = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == LIST) as GFF.LIST;

            // Create list of Placeable objects.
            if (gff.Top_Level.Fields.FirstOrDefault(f => f.Label == PLACEABLE_LIST) is GFF.LIST list && list.Structs.Any())
                Placeables = list.Structs.Select(s => new Placeable(s));

            Sounds = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == SOUND_LIST) as GFF.LIST;
            Stores = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == STORE_LIST) as GFF.LIST;
            Triggers = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == TRIGGER_LIST) as GFF.LIST;
            UseTemplates = GFF.Top_Level.Fields.FirstOrDefault(f => f.Label == USE_TEMPLATES) as GFF.BYTE;
            Waypoints = gff.Top_Level.Fields.FirstOrDefault(f => f.Label == WAYPOINT_LIST) as GFF.LIST;
        }

        /// <summary>
        /// Creates a new GIT object that wraps a GFF object.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if argument gff is null.</exception>
        /// <exception cref="ArgumentException">Thrown if argument gff is not of type GIT.</exception>
        /// <exception cref="InvalidDataException">Thrown if argument gff contains no fields.</exception>
        public static GIT NewGIT(GFF gff)
        {
            if (gff == null)
            {
                throw new ArgumentNullException($"Argument {nameof(gff)} cannot be null.");
            }

            if (gff.FileType != ResourceType.GIT.ToDescription())
            {
                throw new ArgumentException($"Unable to create GIT from a GFF of type {gff.FileType.Trim()}.");
            }

            if (gff.Top_Level == null || (gff.Top_Level.Fields?.Count ?? 0) == 0)
            {
                throw new InvalidDataException($"Argument {nameof(gff)} must have fields.");
            }

            return new GIT(gff);
        }

        #endregion

        #region Nested Classes

        public struct Placeable
        {
            public readonly string TemplateResRef;
            public readonly float Bearing;
            public readonly float X;
            public readonly float Y;
            public readonly float Z;

            public Placeable(GFF.STRUCT gffStruct)
            {
                Bearing = (gffStruct.Fields.FirstOrDefault(f => f.Label == nameof(Bearing)) as GFF.FLOAT).Value;
                TemplateResRef = (gffStruct.Fields.FirstOrDefault(f => f.Label == nameof(TemplateResRef)) as GFF.ResRef).Reference;
                X = (gffStruct.Fields.FirstOrDefault(f => f.Label == nameof(X)) as GFF.FLOAT).Value;
                Y = (gffStruct.Fields.FirstOrDefault(f => f.Label == nameof(Y)) as GFF.FLOAT).Value;
                Z = (gffStruct.Fields.FirstOrDefault(f => f.Label == nameof(Z)) as GFF.FLOAT).Value;
            }
        }

        #endregion
    }
}
