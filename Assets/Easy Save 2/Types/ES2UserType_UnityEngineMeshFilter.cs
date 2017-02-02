using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_UnityEngineMeshFilter : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.MeshFilter data = (UnityEngine.MeshFilter)obj;
		// Add your writer.Write calls here.
		writer.Write(data.mesh);

	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.MeshFilter data = GetOrCreate<UnityEngine.MeshFilter>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.MeshFilter data = (UnityEngine.MeshFilter)c;
		// Add your reader.Read calls here to read the data into the object.
		data.mesh = reader.Read<UnityEngine.Mesh>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineMeshFilter():base(typeof(UnityEngine.MeshFilter)){}
}