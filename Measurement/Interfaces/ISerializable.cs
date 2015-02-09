
namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// A simple interface to provide functionality to serialize an object.
	/// Classes that implement this interface should also provide a static method
	/// with the signature <code>public static ClassImplementingISerializable FromJson(string json)</code>
	/// that should deserialize the serialized object.
	/// </summary>
	public interface ISerializable {

		/// <summary>
		/// Serializes an object that implements ISerializable.
		/// </summary>
		/// <returns>A json string representing the serialized object</returns>
		string ToJson();
	}
}
