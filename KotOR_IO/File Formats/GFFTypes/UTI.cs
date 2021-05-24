using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KotOR_IO.GFFTypes
{
    /// <summary>
    /// Represents the KotOR Item format
    /// </summary>
    public partial class UTI
    {
        /* TODO:
         * Add Upgradable item features
         * Complete XML comments
         * Gets currently throws a linq exception if the field doesn't exist, decide if this is ok or not. Perhaps rethrow, with more details
        */
        private GFF uti;
        public UTI(baseitems b, string reference, string name)
        {
            uti = new GFF(Properties.Resources.blankItem);
            BaseItem = b;
            TemplateResRef = reference;
            Tag = reference;
            LocalizedName = name;
            Description = " ";
        }
        public void WriteToFile(string path)
        {
            uti.WriteToFile(path);
        }
        #region public properties
        public void AddProperty(ItemProperty ip)
        {
            if (uti.Top_Level.Fields.Any(f => f.Label == "PropertiesList"))
            {
                (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PropertiesList") as GFF.LIST).Structs.Add(ip.GetPropertyStruct());
            }
            else { GFF.LIST plist = new GFF.LIST("PropertiesList", new List<GFF.STRUCT>() { ip.GetPropertyStruct() }); }
        }
        public void RemoveProperty(ItemProperty ip)
        {
            (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PropertiesList") as GFF.LIST).Structs.RemoveAll(p => ip.GetPropertyStruct().Equals(p));
        }
        public List<ItemProperty> PropertiesList
        {
            get
            {
                GFF.LIST PropStructs = uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PropertiesList") as GFF.LIST;
                List<ItemProperty> PropList = new List<ItemProperty>();
                foreach (GFF.STRUCT p in PropStructs.Structs)
                {
                    PropList.Add(new ItemProperty(p));
                }
                return PropList;
            }
            private set
            {
                List<GFF.STRUCT> PropStructs = new List<GFF.STRUCT>();
                foreach (var p in value)
                {
                    PropStructs.Add(p.GetPropertyStruct());
                }
                if (uti.Top_Level.Fields.Any(f => f.Label == "PropertiesList"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PropertiesList") as GFF.LIST).Structs = PropStructs;
                }
                else { uti.Top_Level.Fields.Add(new GFF.LIST("PropertiesList", PropStructs)); }
            }
        }
        public uint AddCost
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "AddCost") as GFF.DWORD).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "AddCost"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "AddCost") as GFF.DWORD).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.DWORD("AddCost", value)); }
            }
        }
        public baseitems BaseItem
        {
            get { return (baseitems)(uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "BaseItem") as GFF.INT).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "BaseItem"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "BaseItem") as GFF.INT).Value = (int)value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.INT("BaseItem", (int)value)); }
            }
        }
        public byte BodyVariation
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "BodyVariation") as GFF.BYTE).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "BodyVariation"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "BodyVariation") as GFF.BYTE).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("BodyVariation", value)); }
            }
        }
        public byte Charges
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Charges") as GFF.BYTE).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Charges"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Charges") as GFF.BYTE).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("Charges", value)); }
            }
        }
        public string Comment
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Comment") as GFF.CExoString).CEString; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Comment"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Comment") as GFF.CExoString).CEString = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.CExoString("Comment", value)); }
            }
        }
        public uint Cost
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Cost") as GFF.DWORD).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Cost"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Cost") as GFF.DWORD).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.DWORD("Cost", value)); }
            }
        }
        public string DescIdentified
        {
            //For the purpose of this implementation this will always be the same as Description
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "DescIdentified") as GFF.CExoLocString).Strings[0].SString; }
            private set
            {
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (uti.Top_Level.Fields.Any(f => f.Label == "DescIdentified"))
                {
                    if ((uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "DescIdentified") as GFF.CExoLocString).Strings.Any())
                    {
                        (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "DescIdentified") as GFF.CExoLocString).Strings[0] = sval;
                    }
                    else
                    {
                        (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "DescIdentified") as GFF.CExoLocString).Strings.Add(sval);
                    }
                }
                else { uti.Top_Level.Fields.Add(new GFF.CExoLocString("DescIdentified", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public string Description
        {
            //SUBJECT TO CHANGE: For the purpose of this implementation DescIdentified and Description are synonyms
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings[0].SString; ; }
            set
            {
                DescIdentified = value;
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (uti.Top_Level.Fields.Any(f => f.Label == "Description"))
                {
                    if ((uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings.Any())
                    {
                        (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings[0] = sval;
                    }
                    else
                    {
                        (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Description") as GFF.CExoLocString).Strings.Add(sval);
                    }
                }
                else { uti.Top_Level.Fields.Add(new GFF.CExoLocString("Description", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public bool Identified
        {
            //For the purpose of this implementation Identified should always be true
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Identified") as GFF.BYTE).Value > 0; }
            private set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Identified"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Identified") as GFF.BYTE).Value = value ? (byte)1 : (byte)0;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("Identified", value ? (byte)1 : (byte)0)); }
            }
        }
        public string LocalizedName
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings[0].SString; }
            set
            {
                GFF.CExoLocString.SubString sval = new GFF.CExoLocString.SubString(0, value);
                if (uti.Top_Level.Fields.Any(f => f.Label == "LocalizedName"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings.Clear();
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "LocalizedName") as GFF.CExoLocString).Strings.Add(sval);
                }
                else { uti.Top_Level.Fields.Add(new GFF.CExoLocString("LocalizedName", -1, new List<GFF.CExoLocString.SubString>() { sval })); }
            }
        }
        public byte ModelVariation
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "ModelVariation") as GFF.BYTE).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "ModelVariation"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "ModelVariation") as GFF.BYTE).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("ModelVariation", value)); }
            }
        }
        public byte PaletteID
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PaletteID") as GFF.BYTE).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "PaletteID"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "PaletteID") as GFF.BYTE).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("PaletteID", value)); }
            }
        }
        public bool Plot
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Plot") as GFF.BYTE).Value > 0; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Plot"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Plot") as GFF.BYTE).Value = value ? (byte)1 : (byte)0;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("Plot", value ? (byte)1 : (byte)0)); }
            }
        }
        public ushort StackSize
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "StackSize") as GFF.WORD).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "StackSize"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "StackSize") as GFF.WORD).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.WORD("StackSize", value)); }
            }
        }
        public bool Stolen
        {
            //For the purpose of this implementation Stolen should always be false
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Stolen") as GFF.BYTE).Value > 0; }
            private set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Stolen"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Stolen") as GFF.BYTE).Value = value ? (byte)1 : (byte)0;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("Stolen", value ? (byte)1 : (byte)0)); }
            }
        }
        public string Tag
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Tag") as GFF.CExoString).CEString; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "Tag"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "Tag") as GFF.CExoString).CEString = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.CExoString("Tag", value)); }
            }
        }
        public string TemplateResRef
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "TemplateResRef") as GFF.ResRef).Reference; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "TemplateResRef"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "TemplateResRef") as GFF.ResRef).Reference = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.ResRef("TemplateResRef", value)); }
            }
        }
        public byte TextureVar
        {
            get { return (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "TextureVar") as GFF.BYTE).Value; }
            set
            {
                if (uti.Top_Level.Fields.Any(f => f.Label == "TextureVar"))
                {
                    (uti.Top_Level.Fields.FirstOrDefault(f => f.Label == "TextureVar") as GFF.BYTE).Value = value;
                }
                else { uti.Top_Level.Fields.Add(new GFF.BYTE("TextureVar", value)); }
            }
        }
        #endregion

        public class ItemProperty
        {
            private GFF.STRUCT prop;
            public GFF.STRUCT GetPropertyStruct()
            {
                return prop;
            }
            #region public properties
            public itemprops PropertyName
            {
                get { return (itemprops)(prop.Fields.FirstOrDefault(f => f.Label == "PropertyName") as GFF.WORD).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "PropertyName"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "PropertyName") as GFF.WORD).Value = (byte)value;
                    }
                    else { prop.Fields.Add(new GFF.WORD("PropertyName", (byte)value)); }
                }
            }
            public ushort Subtype
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "Subtype") as GFF.WORD).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "Subtype"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "Subtype") as GFF.WORD).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.WORD("Subtype", (byte)value)); }
                }
            }
            public byte CostTable
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "CostTable") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "CostTable"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "CostTable") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("CostTable", (byte)value)); }
                }
            }
            public ushort CostValue
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "CostValue") as GFF.WORD).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "CostValue"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "CostValue") as GFF.WORD).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.WORD("CostValue", value)); }
                }
            }
            public byte Param1
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "Param1") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "Param1"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "Param1") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("Param1", value)); }
                }
            }
            public byte Param1Value
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "Param1Value") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "Param1Value"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "Param1Value") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("Param1Value", value)); }
                }
            }
            public byte Param2
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "Param2") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "Param2"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "Param2") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("Param2", value)); }
                }
            }
            public byte Param2Value
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "Param2Value") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "Param2Value"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "Param2Value") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("Param2Value", value)); }
                }
            }
            public byte ChanceAppear
            {
                get { return (prop.Fields.FirstOrDefault(f => f.Label == "ChanceAppear") as GFF.BYTE).Value; }
                set
                {
                    if (prop.Fields.Any(f => f.Label == "ChanceAppear"))
                    {
                        (prop.Fields.FirstOrDefault(f => f.Label == "ChanceAppear") as GFF.BYTE).Value = value;
                    }
                    else { prop.Fields.Add(new GFF.BYTE("ChanceAppear", value)); }
                }
            }
            #endregion
            public ItemProperty(GFF.STRUCT prop)
            {
                this.prop = prop;
            }
            public ItemProperty(itemprops PropertyName = 0, ushort Subtype = 0, byte CostTable = 0, ushort CostValue = 0, byte Param1 = 255, byte Param1Value = 0, byte Param2 = 255, byte Param2Value = 0, byte ChanceAppear = 100)
            {
                List<GFF.FIELD> fs = new List<GFF.FIELD>();
                prop = new GFF.STRUCT("", 0, fs);

                this.PropertyName = PropertyName;
                this.Subtype = Subtype;
                this.CostTable = CostTable;
                this.CostValue = CostValue;
                this.Param1 = Param1;
                this.Param1Value = Param1Value;
                this.Param2 = Param2;
                this.Param2Value = Param2Value;
                this.ChanceAppear = ChanceAppear;
            }
            public string ToString(KEY chitin, BIF tda)
            {
                //Ready 2das
                if (tda.Name != PATH2DA) { tda.AttachKey(chitin, PATH2DA); } //If 2da.bif hasn't already been indexed, attach the Key

                //pull item property tables
                TwoDA itempropdef = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == ITEMPROPDEF).EntryData);
                TwoDA costtable = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == COSTTABLE).EntryData);
                TwoDA paramtable = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == PARAMTABLE).EntryData);

                StringBuilder sb = new StringBuilder();

                string propname = PropertyName.ToDescription();
                string subtypename = itempropdef.Data[SUBTYPECOL][(int)PropertyName];
                string subtypeval = "";
                if (subtypename != "")
                {
                    TwoDA subtemp = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == subtypename.ToLower()).EntryData);
                    subtypeval = subtemp.Data[LABELCOL][Subtype];
                }
                else { subtypename = "None"; }
                string costname = costtable.Data[LABELCOL][CostTable];
                string costvalue = "";
                if (costname != "Base1")
                {
                    TwoDA costtemp = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == costtable.Data[NAMECOL][CostTable].ToLower()).EntryData);
                    try { costvalue = costtemp.Data[LABELCOL][CostValue]; } catch (Exception e) { }
                }
                string param1name = "";
                try { param1name = paramtable.Data[LABELCOL][Param1]; } catch (Exception e) { param1name = "None"; }
                string param1value = "";
                if (param1name != "None")
                {
                    TwoDA paramtemp = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == paramtable.Data[PARAMTABLECOL][Param1].ToLower()).EntryData);
                    try { param1value = paramtemp.Data[LABELCOL][Param1Value]; } catch (Exception e) { }
                }
                string param2name = "";
                try { param2name = paramtable.Data[LABELCOL][Param2]; } catch (Exception e) { param2name = "None"; }
                string param2value = "";
                if (param2name != "None")
                {
                    TwoDA paramtemp = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == paramtable.Data[PARAMTABLECOL][Param2].ToLower()).EntryData);
                    try { param2value = paramtemp.Data[LABELCOL][Param2Value]; } catch (Exception e) { }
                }

                sb.AppendLine(propname);
                sb.AppendLine("Subtype: " + subtypename + " = " + subtypeval); //Make prettier in future
                sb.AppendLine("Cost: " + costname + " = " + costvalue);
                sb.AppendLine("Param1: " + param1name + " = " + param1value);
                sb.AppendLine("Param2: " + param2name + " = " + param2value);

                return sb.ToString();
            }
            public string prettyprint(KEY chitin, BIF tda, TLK t)
            {
                //CREATE PRETTY OUTPUTS FOR THE ITEM DESCRIPTIONS
                //Ready 2das
                if (tda.Name != PATH2DA) { tda.AttachKey(chitin, PATH2DA); } //If 2da.bif hasn't already been indexed, attach the Key

                //2DAs
                TwoDA costtable = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == COSTTABLE).EntryData);
                TwoDA itempropdef = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == ITEMPROPDEF).EntryData);
                TwoDA paramtable = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == PARAMTABLE).EntryData);
                TwoDA cost = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == costtable.Data[NAMECOL][CostTable].ToLower()).EntryData);
                TwoDA subt;
                try
                {
                    subt = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == itempropdef.Data[SUBTYPECOL][(int)PropertyName].ToLower()).EntryData);
                }
                catch (Exception nre) { subt = null; }
                TwoDA prm1 = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == paramtable.Data[PARAMTABLECOL][Param1 == 255 ? 0 : Param1].ToLower()).EntryData);
                TwoDA prm2 = new TwoDA(tda.VariableResourceTable.FirstOrDefault(r => r.ResRef == paramtable.Data[PARAMTABLECOL][Param2 == 255 ? 0 : Param2].ToLower()).EntryData);

                StringBuilder sb = new StringBuilder();

                #region Property Switch
                switch (PropertyName)
                {
                    case itemprops.Ability_Bonus:
                        sb.AppendLine($"Ability Bonus: {subt.Data["label"][Subtype]} +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.AC_Bonus:
                        sb.AppendLine($"Defense Bonus: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.AC_Bonus_vs_Alignment_Group:
                        sb.AppendLine($"Defense Bonus vs. {subt.Data["label"][Subtype]}: +{cost.Data["value"][CostValue]}".Replace('_', ' '));
                        break;
                    case itemprops.AC_Bonus_vs_Damage_Type:
                        sb.AppendLine($"Defense Bonus vs. {subt.Data["label"][Subtype]}: +{cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.AC_Bonus_vs_Racial_Group:
                        sb.AppendLine($"Defense Bonus vs. {subt.Data["label"][Subtype]}: +{cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Enhancement_Bonus:
                        sb.AppendLine($"Enhancement Bonus: +{cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Enhancement_Bonus_vs__Alignment_Group:
                        sb.AppendLine($"Enhancement Bonus vs. {subt.Data["label"][Subtype]}: +{cost.Data["value"][CostValue]}".Replace('_', ' '));
                        break;
                    case itemprops.Enchancement_Bonus_vs_Racial_Group:
                        sb.AppendLine($"Enhancement Bonus vs. {subt.Data["label"][Subtype]}: +{cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Attack_Penalty:
                        sb.AppendLine($"Attack Penalty: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Bonus_Feat:
                        sb.AppendLine($"Grants Feat: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText}");
                        string temp = subt.Data["description"][Subtype];
                        if (temp != "") sb.AppendLine(t.String_Data_Table[Convert.ToInt32(temp)].StringText);
                        break;
                    case itemprops.Activate_Item_IE_CAST_SPELL:
                        sb.AppendLine($"Uses: {t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        sb.AppendLine($"On Activation: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText}");
                        temp = subt.Data["spelldesc"][Subtype];
                        if (temp != "") sb.AppendLine(t.String_Data_Table[Convert.ToInt32(temp)].StringText);
                        break;
                    case itemprops.Damage_Bonus:
                        sb.AppendLine($"Damage Bonus: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        break;
                    case itemprops.Damage_Bonus_vs_Alignment_Group: //Param1
                        sb.AppendLine($"Damage Bonus vs. {subt.Data["label"][Subtype]} : {t.String_Data_Table[Convert.ToInt32(prm1.Data["name"][Subtype])].StringText} +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}".Replace('_', ' '));
                        break;
                    case itemprops.Damage_Bonus_vs_Racial_Group: //Param1
                        sb.AppendLine($"Damage Bonus vs. {subt.Data["label"][Subtype]} : {t.String_Data_Table[Convert.ToInt32(prm1.Data["name"][Subtype])].StringText} +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        break;
                    case itemprops.Damage_Immunity:
                        sb.AppendLine($"Damage Immunity: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Damage_Penalty:
                        sb.AppendLine($"Damage Penalty: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Damage_Reduction:
                        sb.AppendLine($"Damage Reduction: {cost.Data["amount"][CostValue]} | {subt.Data["label"][Subtype]}");
                        break;
                    case itemprops.Damage_Resistance:
                        sb.AppendLine($"Damage Resistance: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {cost.Data["amount"][CostValue]}");
                        break;
                    case itemprops.Damage_Vulnerability:
                        sb.AppendLine($"Damage Vulnerability: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Decreased_Ability_Score:
                        sb.AppendLine($"Ability Reduced: {subt.Data["label"][Subtype]} {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Decreased_AC:
                        sb.AppendLine($"Defense Reduced: {subt.Data["label"][Subtype].Substring(3)} {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Decreased_Skill_Modifier:
                        sb.AppendLine($"Skill Reduced: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Extra_Melee_Damage_Type:
                        sb.AppendLine($"Extra Melee Damage: {subt.Data["label"][Subtype]}");
                        break;
                    case itemprops.Extra_Ranged_Damage_Type:
                        sb.AppendLine($"Extra Ranged Damage: {subt.Data["label"][Subtype]}");
                        break;
                    case itemprops.Immunity:
                        temp = subt.Data["name"][Subtype];
                        if (temp == "") temp = subt.Data["label"][Subtype];
                        else temp = t.String_Data_Table[Convert.ToInt32(temp)].StringText;
                        sb.AppendLine($"Immunity: {temp}");
                        break;
                    case itemprops.Improved_Force_Resistance:
                        sb.AppendLine($"Force Resistance: +{cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Improved_Saving_Throws:
                        sb.AppendLine($"{t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} Saving Throws: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Improved_Saving_Throws_Specific:
                        sb.AppendLine($"{subt.Data["namestring"][Subtype]} Saving Throws: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Keen:
                        sb.AppendLine($"Keen");
                        break;
                    case itemprops.Light:
                        sb.AppendLine($"Light: {prm1.Data["label"][Subtype]}");
                        break;
                    case itemprops.Mighty:
                        sb.AppendLine($"Mighty: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.No_Damage:
                        sb.AppendLine($"No Damage");
                        break;
                    case itemprops.On_Hit_Properties:
                        sb.AppendLine($"On Hit: {t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText} {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {t.String_Data_Table[Convert.ToInt32(prm1.Data["name"][Subtype])].StringText}".Replace('_', ' '));
                        break;
                    case itemprops.Reduced_Saving_Throws:
                        sb.AppendLine($"{t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} Saving Throws: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Reduced_Saving_Throws_Specific:
                        sb.AppendLine($"{subt.Data["namestring"][Subtype]} Saving Throws: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Regeneration:
                        sb.AppendLine($"Regeneration: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Skill_Bonus:
                        sb.AppendLine($"Skill Bonus: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Security:
                        sb.AppendLine($"Security Spike: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Attack_Bonus:
                        sb.AppendLine($"Attack Bonus: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Attack_Bonus_vs_Alignment_Group:
                        sb.AppendLine($"Attack Bonus vs. {subt.Data["label"][Subtype]} : +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}".Replace('_', ' '));
                        break;
                    case itemprops.Attack_Bonus_vs_Racial_Group:
                        sb.AppendLine($"Damage Bonus vs. {subt.Data["label"][Subtype]} : +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        break;
                    case itemprops.To_Hit_Penalty:
                        sb.AppendLine($"To Hit: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Unlimited_Ammunition:
                        sb.AppendLine($"Unlmited Ammo (Does nothing?)");
                        break;
                    case itemprops.Use_Limitation_Alignment_Group:
                        sb.AppendLine($"Required: {subt.Data["label"][Subtype]}".Replace('_', ' '));
                        break;
                    case itemprops.Use_Limitation_Class:
                        sb.AppendLine($"Required: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText}");
                        break;
                    case itemprops.Use_Limitation_Racial_Type:
                        sb.AppendLine($"Required: {subt.Data["label"][Subtype]}");
                        break;
                    case itemprops.Trap:
                        temp = subt.Data["name"][Subtype];
                        if (Convert.ToInt32(temp) > 30000) temp = "Falling Rock Trap";
                        else temp = t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText;
                        sb.AppendLine(temp);
                        sb.AppendLine($"DC {subt.Data["setdc"][Subtype]} Set");
                        sb.AppendLine($"DC {subt.Data["detectdcmod"][Subtype]} Detect");
                        sb.AppendLine($"DC {subt.Data["disarmdcmod"][Subtype]} Disarm");
                        break;
                    case itemprops.True_Seeing:
                        sb.AppendLine($"Stealth Field Nullifier");
                        break;
                    case itemprops.On_Monster_Hit: //Params 1 & 2
                        temp = "";
                        if (Param1 != 255) temp += t.String_Data_Table[Convert.ToInt32(prm1.Data["name"][Subtype])].StringText;
                        if (Param2 != 255) temp += $" -{t.String_Data_Table[Convert.ToInt32(prm2.Data["name"][Subtype])].StringText}";
                        sb.AppendLine($"On Hit: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText} {temp}");
                        break;
                    case itemprops.Massive_Criticals:
                        sb.AppendLine($"Massive Criticals: +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        break;
                    case itemprops.Freedom_of_Movement:
                        sb.AppendLine($"Freedom of Movement (does nothing?)");
                        break;
                    case itemprops.Monster_Damage:
                        sb.AppendLine($"Damage Bonus: {cost.Data["label"][Subtype]}");
                        break;
                    case itemprops.Special_Walk: //I don't think this does anything, make more descriptive if so
                        sb.AppendLine(subt.Data["label"][Subtype]);
                        break;
                    case itemprops.Computer_Spike:
                        sb.AppendLine($"Computer Spike: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Regeneration_Force_Points:
                        sb.AppendLine($"Regenerate Force Points: +{t.String_Data_Table[Convert.ToInt32(cost.Data["name"][CostValue])].StringText}");
                        break;
                    case itemprops.Blaster_Bolt_Deflection_Increase:
                        sb.AppendLine($"Blaster Bolt Deflection: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Blaster_Bolt_Deflection_Decrease:
                        sb.AppendLine($"Blaster Bolt Deflection: {cost.Data["value"][CostValue]}");
                        break;
                    case itemprops.Use_Limitation_Feat:
                        sb.AppendLine($"Required Feat: {t.String_Data_Table[Convert.ToInt32(subt.Data["name"][Subtype])].StringText}");
                        break;
                    case itemprops.Droid_Repair:
                        sb.AppendLine($"Droid Repair Kit: +{cost.Data["label"][CostValue]}");
                        break;
                    case itemprops.Disguise:
                        temp = subt.Data["label"][Subtype];
                        temp = temp.Replace('_', ' ');
                        temp = temp.Replace("01", "");
                        temp = temp.Replace("02", "");
                        temp = temp.Replace("03", "");
                        temp = temp.Replace("04", "");
                        temp = temp.Replace("05", "");
                        temp = temp.Replace(" Mal ", " Male ");
                        temp = temp.Replace(" Fem ", " Female ");
                        if (temp[0] == 'P')
                        {
                            temp = temp.Replace("P", "Player");
                            temp = temp.Replace("FEM", "Female");
                            temp = temp.Replace("MAL", "Male");
                            temp = temp.Replace("A", "Asian");
                            temp = temp.Replace("B", "Black");
                            temp = temp.Replace("C", "Caucasian");
                            temp = temp.Replace("SML", "Small");
                            temp = temp.Replace("MED", "Medium");
                            temp = temp.Replace("LRG", "Large");
                        }
                        temp = temp.Replace("  ", " ");
                        sb.AppendLine($"Disguise: {temp}");
                        break;
                    default:
                        break;
                }
                #endregion

                return sb.ToString();

            }
        }

    }
}
