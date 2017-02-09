using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_SeedData : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		SeedData data = (SeedData)obj;
		// Add your writer.Write calls here.

	}
	
	public override object Read(ES2Reader reader)
	{
		SeedData data = new SeedData();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		SeedData data = (SeedData)c;
		// Add your reader.Read calls here to read the data into the object.

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_SeedData():base(typeof(SeedData)){}
}