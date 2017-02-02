using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_UnityEngineRigidbody : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.Rigidbody data = (UnityEngine.Rigidbody)obj;
		// Add your writer.Write calls here.
		writer.Write(data.drag);
		writer.Write(data.mass);
		writer.Write(data.useGravity);
		writer.Write(data.isKinematic);
		writer.Write(data.constraints);

	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.Rigidbody data = GetOrCreate<UnityEngine.Rigidbody>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.Rigidbody data = (UnityEngine.Rigidbody)c;
		// Add your reader.Read calls here to read the data into the object.
		data.drag = reader.Read<System.Single>();
		data.mass = reader.Read<System.Single>();
		data.useGravity = reader.Read<System.Boolean>();
		data.isKinematic = reader.Read<System.Boolean>();
		data.constraints = reader.Read<UnityEngine.RigidbodyConstraints>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineRigidbody():base(typeof(UnityEngine.Rigidbody)){}
}