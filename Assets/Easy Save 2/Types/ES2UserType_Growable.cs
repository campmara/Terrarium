using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_Growable : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		Growable data = (Growable)obj;
		// Add your writer.Write calls here.

	}
	
	public override object Read(ES2Reader reader)
	{
		Growable data = GetOrCreate<Growable>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		Growable data = (Growable)c;
		// Add your reader.Read calls here to read the data into the object.

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_Growable():base(typeof(Growable)){}
}