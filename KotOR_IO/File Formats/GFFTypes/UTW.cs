using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KotOR_IO.GFFTypes
{
    public partial class UTW
    {
        /* TODO:
         * ToString overload
         */
        
        private GFF utw;
        public UTW(string reference, string name)
        {
            utw = new GFF(Properties.Resources.blankWaypoint);
            TemplateResRef = reference;
            Tag = reference;
            LocalizedName = name;
            Description = " ";
        }
        public void WriteToFile(string path)
        {
            utw.WriteToFile(path);
        }

        #region public properties
        public string Comment
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Comment") as GFF.CExoString).CEString; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "Comment"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Comment") as GFF.CExoString).CEString = value;
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoString("Comment", value)); }
            }
        }
        public string Description
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings[0].SString; ; }
            set
            {
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (utw.Top_Level.Fields.Any(f => f.Label == "Description"))
                {
                    if ((utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings.Any())
                    {
                        (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings[0] = sval;
                    }
                    else
                    {
                        (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings.Add(sval);
                    }
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoLocString("Description", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public bool HasMapNote
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "HasMapNote") as GFF.BYTE).Value > 0; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "HasMapNote"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "HasMapNote") as GFF.BYTE).Value = value ? (byte)1 : (byte)0;
                }
                else { utw.Top_Level.Fields.Add(new GFF.BYTE("HasMapNote", value ? (byte)1 : (byte)0)); }
            }
        }
        public string LinkedTo
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "LinkedTo") as GFF.CExoString).CEString; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "LinkedTo"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "LinkedTo") as GFF.CExoString).CEString = value;
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoString("LinkedTo", value)); }
            }
        }
        public string LocalizedName
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings[0].SString; }
            set
            {
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (utw.Top_Level.Fields.Any(f => f.Label == "LocalizedName"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings.Clear();
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings.Add(sval);
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoLocString("LocalizedName", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public string MapNote
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "MapNote") as GFF.CExoLocString).Strings[0].SString; }
            set
            {
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (utw.Top_Level.Fields.Any(f => f.Label == "MapNote"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "MapNote") as GFF.CExoLocString).Strings.Clear();
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "MapNote") as GFF.CExoLocString).Strings.Add(sval);
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoLocString("MapNote", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public bool MapNoteEnabled
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "MapNoteEnabled") as GFF.BYTE).Value > 0; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "MapNoteEnabled"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "MapNoteEnabled") as GFF.BYTE).Value = value ? (byte)1 : (byte)0;
                }
                else { utw.Top_Level.Fields.Add(new GFF.BYTE("MapNoteEnabled", value ? (byte)1 : (byte)0)); }
            }
        }
        public byte PaletteID
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "PaletteID") as GFF.BYTE).Value; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "PaletteID"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "PaletteID") as GFF.BYTE).Value = value;
                }
                else { utw.Top_Level.Fields.Add(new GFF.BYTE("PaletteID", value)); }
            }
        }
        public string Tag
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Tag") as GFF.CExoString).CEString; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "Tag"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "Tag") as GFF.CExoString).CEString = value;
                }
                else { utw.Top_Level.Fields.Add(new GFF.CExoString("Tag", value)); }
            }
        }
        public string TemplateResRef
        {
            get { return (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "TemplateResRef") as GFF.ResRef).Reference; }
            set
            {
                if (utw.Top_Level.Fields.Any(f => f.Label == "TemplateResRef"))
                {
                    (utw.Top_Level.Fields.FirstOrDefault(f => f.Label == "TemplateResRef") as GFF.ResRef).Reference = value;
                }
                else { utw.Top_Level.Fields.Add(new GFF.ResRef("TemplateResRef", value)); }
            }
        }
        #endregion
    }
}
