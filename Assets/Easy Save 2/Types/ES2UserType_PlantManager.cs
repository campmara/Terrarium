using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_PlantManager : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		PlantManager data = (PlantManager)obj;
		// Add your writer.Write calls here.

	}
	
	public override object Read(ES2Reader reader)
	{
		PlantManager data = GetOrCreate<PlantManager>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		PlantManager data = (PlantManager)c;
		// Add your reader.Read calls here to read the data into the object.

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_PlantManager():base(typeof(PlantManager)){}
}