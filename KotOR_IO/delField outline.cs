public void delete_field(string label, int occurance)
{
	int index = get_Field_Index(label, occurance);
	
	//Some checking to see if any other fields use this label, otherwise delete label
	
	if (!Field_Array[index].Complex)
	{
        LabelOffset -= 12;
        FieldDataOffset -= 12;
        FieldIndicesOffset -= 12;
        ListIndicesOffset -= 12;
	}
	else
	{
		switch(Field_Array[index].Type)
		{
			case 6:
				//Work out offset change
				//remove from field data block
			case 7:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
				break;
		}
	}
	
	FieldCount--;
	
	Field_Array.RemoveAt();
}
	