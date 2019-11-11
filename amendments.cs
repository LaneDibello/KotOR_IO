public object this[string SSField] //value from referene table
{
	get 
	{
		return StringTable[SSField];
	}
	set
	{
		StringTable[SSField] = value;
	}
}

/////////////////////////////////////////////////////////////


public string this[int str_ref] //integer string referene
{
	get
	{
		return String_Data_Table[str_ref].StringText;
	}
	set
	{
		int delta_offset = value.Length() - String_Data_Table[str_ref].StringSize;
		String_Data_Table[str_ref].StringSize = value.Length();
		String_Data_Table[str_ref].StringText = value;
		for (int i = str_ref + 1; i < StringCount; i++)
		{
			String_Data_Table[i].OffsetToSring += delta_offset;
		}
	}
}
