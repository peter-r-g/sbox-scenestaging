using System.Collections.Generic;
using Sandbox;

public abstract partial class BaseComponent
{
	//
	// We should have the same get component functionality here as in GameObject
	//

	/// <inheritdoc cref="GameObject.GetComponent{T}(bool, bool)"/>
	[Pure]
	public T GetComponent<T>( [NodeProperty] bool enabledOnly = true, [NodeProperty] bool deep = false ) => GameObject.GetComponent<T>( enabledOnly, deep );

	/// <inheritdoc cref="GameObject.GetComponents{T}(bool, bool)"/>
	[Pure]
	public IEnumerable<T> GetComponents<T>( [NodeProperty] bool enabledOnly = true, [NodeProperty] bool deep = false ) => GameObject.GetComponents<T>( enabledOnly, deep );
	
	[Pure]
	public bool TryGetComponent<T>( out T component, [NodeProperty] bool enabledOnly = true, [NodeProperty] bool deep = false ) => GameObject.TryGetComponent( out component, enabledOnly, deep );
	
	[Pure]
	public T GetComponentInParent<T>( [NodeProperty] bool enabledOnly = true, [NodeProperty] bool andSelf = false ) => GameObject.GetComponentInParent<T>( enabledOnly, andSelf );
}
