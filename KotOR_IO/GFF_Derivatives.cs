using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

/* TO DO:
 * I haven't decided how I want to layout these files yet.
 * The concept of using structs outlined below may be do-able but it still seems rather messy.
 * I want to avoid drowning the user in Fields/properties
 * The end goal should be having the end user experience as simplistic as possible, and ultimately simplify the GFF for their use
 * Unfortunately many of the files derived from GFF are also quite complicated.
 * 
 */
namespace KotOR_IO
{
    /// <summary>
    /// Contains <see cref="GFF"/> Derivative classes that represent common objects in the game.
    /// </summary>
    public class Blueprint
    {
        /// <summary>
        /// <see cref="GFF"/> Creature <see cref="Blueprint"/>. Represents any creature or character in the game.
        /// </summary>
        public class UTC : GFF
        {
            public byte Race;
            public byte SubraceIndex;
            public string FirstName;
            public string LastName;
            public short Appearance_Type;
            public byte Gender;
            public int Phenotype;
            public short PortraitId;
            public string Description;
            public string Subrace;
            public byte BodyBag;
            public byte BodyVariation;
            public byte TextureVar;

            public string TemplateResRef;
            public string Tag;
            public byte IsPC;
            public short FactionID;
            public byte Plot;

            public string Conversation;
            public byte Disarmable;
            public string Deity;
            public short SoundSetFile;
            public byte Interruptable;
            public byte NoPermDeath;
            public byte Min1HP;
            public byte PartyInteract;

            public byte Str;
            public byte Dex;
            public byte Con;
            public byte Int;
            public byte Wis;
            public byte Cha;

            public int WalkRate;
            public byte NaturalAC;

            public short HitPoints;
            public short CurrentHitPoints;
            public short MaxHitPoints;

            public short ForcePoints;
            public short CurrentForce;

            public short refbonus;
            public short willbonus;
            public short fortbonus;

            public byte GoodEvil;
            public byte LawfulChaotic;

            public float ChallengeRating;
            public byte PerceptionRange;

            public string ScriptHeartbeat;
            public string ScriptOnNotice;
            public string ScriptSpellAt;
            public string ScriptAttacked;
            public string ScriptDamaged;
            public string ScriptDisturbed;
            public string ScriptEndRound;
            public string ScriptEndDialogu;
            public string ScriptDialogue;
            public string ScriptSpawn;
            public string ScriptRested;
            public string ScriptDeath;
            public string ScriptUserDefine;
            public string ScriptOnBlocked;

            public List<byte> SkillList; //contains ranks
            public List<int> FeatList; //int feat indexes
            public List<object> ClassList; //Contains class data (int class, short level, list of known spells (int spell, byte spellflags?, byte SpellMetaMagic)
            public List<string> Equip_ItemList; //string EquippedRes
            public byte PaletteID;
            public string Comment;
        }

        /// <summary>
        /// <see cref="GFF"/> Door <see cref="Blueprint"/>. Represents most door objects in the Game.
        /// </summary>
        public class UTD : GFF
        {
            public byte KeyRequired;
            public byte TrapFlag;
            public byte TrapOneShot;
            public byte DisarmDC;
            public string Description;
            public string OnTrapTriggered;
            public string Comment;
            public string OnFailToOpen;
            public byte OpenLockDC;
            public byte Locked;
            public string Conversation;
            public string OnMeleeAttacked;
            public string Portrait;
            public byte Interruptable;
            public string TemplateResRef;
            public byte TrapDisarmable;
            public string OnHeartbeat;
            public string OnSpellCastAt;
            public string OnDamaged;
            public string OnOpen;
            public byte Hardness;
            public byte AnimationState;
            public string OnLock;
            public byte GenericType;
            public string OnUnlock;
            public byte Will;
            public byte TrapDetectable;
            public byte LinkedToFlags;
            public byte TrapType;
            public byte Lockable;
            public short HP;
            public string OnUserDefined;
            public int Faction;
            public byte PaletteID;
            public byte Plot;
            public string LinkedTo;
            public byte TrapDetectDC;
            public string LocName;
            public int Appearance;
            public string KeyName;
            public string OnClosed;
            public short CurrentHP;
            public byte AutoRemoveKey;
            public string OnDisarm;
            public byte Fort;
            public string OnDeath;
            public string Tag;
            public string OnClick;
            public byte CloseLockDC;
        }

        /// <summary>
        /// <see cref="GFF"/> Encounter <see cref="Blueprint"/>. Represents spawn-enemy event triggers.
        /// </summary>
        public class UTE : GFF
        {
            public string Tag;
            public string LocalizedName;
            public string TemplateResRef;
            public byte Active;
            public int Difficulty;
            public int DifficultyIndex;
            public int Faction;
            public int MaxCreatures;
            public byte PlayerOnly;
            public int RecCreatures;
            public byte Reset;
            public int ResetTime;
            public int Respawns;
            public int SpawnOption;
            public string OnEntered;
            public string OnExit;
            public string OnExhausted;
            public string OnHeartbeat;
            public string OnUserDefined;

            public List<Creature> CreatureList;
            public struct Creature
            {
                public int Appearance;
                public float CR;
                public string ResRef;
                public byte SingleSpawn;
            }

            public byte PaletteID;
            public string Comment;
        }

        /// <summary>
        /// <see cref="GFF"/> Item <see cref="Blueprint"/>. Represents all in-game items.
        /// </summary>
        public class UTI : GFF
        {

        }

        /// <summary>
        /// <see cref="GFF"/> Placeable <see cref="Blueprint"/>. Represents placeable objects (i.e. metal cylinders, footlockers, marker post) in the game.
        /// </summary>
        public class UTP : GFF
        {

        }

        /// <summary>
        /// <see cref="GFF"/> Sound <see cref="Blueprint"/>. Represents a positional sound object.
        /// </summary>
        public class UTS : GFF
        {

        }

        /// <summary>
        /// <see cref="GFF"/> Merchant <see cref="Blueprint"/>. Represents a vendor or mercahnt that sells listed goods.
        /// </summary>
        public class UTM : GFF
        {

        }

        /// <summary>
        /// <see cref="GFF"/> Trigger <see cref="Blueprint"/>. Represents a trigger area that fires ascript when entered.
        /// </summary>
        public class UTT : GFF
        {

        }

        /// <summary>
        /// <see cref="GFF"/> Waypoint <see cref="Blueprint"/>. Represents a coordinate point reference.
        /// </summary>
        public class UTW : GFF
        {

        }
    }
    
    /// <summary>
    /// <see cref="GFF"/> Static Area Info file. Contains Static information (hallways, layouts, structures) about a module area.
    /// </summary>
    public class ARE : GFF
    {
    
    }
    
    /// <summary>
    /// <see cref="GFF"/> Dialogue File. Contains conversion/cutscene nodes used for in-game dialogues.
    /// </summary>
    public class DLG : GFF
    {
    
    }
    
    /// <summary>
    /// <see cref="GFF"/> Faction FIle. Contains information about different factions/alignment groups in the module (i.e. hostile, passive, etc.)
    /// </summary>
    public class FAC : GFF
    {
    
    }
    
    /// <summary>
    /// <see cref="GFF"/> Dynamic Area Information File. Contains dynamic information (Creatures, Placeables, Triggers) about a module.
    /// </summary>
    public class GIT : GFF
    {
    
    }
    
    /// <summary>
    /// GFF General User Interface File. Contains UI element information for various menus in the game.
    /// </summary>
    public class GUI : GFF
    {
    
    }
    
    /// <summary>
    /// <see cref="GFF"/> Module Info File. Contains information about the module (spawnpoint, lighting data, etc)
    /// </summary>
    public class IFO : GFF
    {
    
    }
    
    /// <summary>
    /// <see cref="GFF"/> Journal File. Contains the log entries for the player Journal/ mission log stored with each save.
    /// </summary>
    public class JRL : GFF
    {
    
    }
}
