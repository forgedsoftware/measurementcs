using System.Collections.Generic;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Basic interface for a tree node.
	/// </summary>
	/// <typeparam name="TValue">The concrete implementation of ITreeNode</typeparam>
	public interface ITreeNode<TValue>
		where TValue : ITreeNode<TValue> {

		/// <summary>
		/// The parent of the current node.
		/// </summary>
		TValue Parent { get; }

		/// <summary>
		/// A list of children of the current node.
		/// </summary>
		List<TValue> Children { get; }

		/// <summary>
		/// A function determining if this node is a root node.
		/// </summary>
		/// <returns>True if root node, else false.</returns>
		bool IsRoot();

	}
}
