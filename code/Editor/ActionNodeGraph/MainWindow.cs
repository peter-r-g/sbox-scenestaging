﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor.NodeEditor;
using Facepunch.ActionJigs;
using Sandbox;
using Sandbox.UI;

namespace Editor.ActionJigs;

public partial class MainWindow : DockWindow
{
	private static Dictionary<ActionJig, MainWindow> Instances { get; } = new Dictionary<ActionJig, MainWindow>();

	public static MainWindow Open( ActionJig actionJig, string name )
	{
		if ( !Instances.TryGetValue( actionJig, out var inst ) )
		{
			Instances[actionJig] = inst = new MainWindow( actionJig, name );
		}

		inst.Show();
		inst.Focus();
		return inst;
	}

	public ActionJig ActionJig { get; }

	public ActionGraph Graph { get; }
	public GraphView View { get; }

	public event Action Saved;

	private MainWindow( ActionJig actionJig, string name )
	{
		DeleteOnClose = true;

		ActionJig = actionJig;
		Graph = new ActionGraph( actionJig );
		View = new ActionGraphView( null )
		{
			Graph = Graph,
			WindowTitle = "Graph View"
		};
		View.SetBackgroundImage( "toolimages:/grapheditor/grapheditorbackgroundpattern_shader.png" );

		foreach ( var nodeDefinition in EditorNodeLibrary.All.Values )
		{
			View.AddNodeType( new ActionNodeType( nodeDefinition ) );
		}

		Title = $"{name} - Action Graph";
		Size = new Vector2( 1280, 720 );

		RebuildUI();
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		var updated = Graph.Update();

		if ( updated.Any() )
		{
			View.UpdateConnections( updated );
		}
	}

	[Event.Hotload]
	public void RebuildUI()
	{
		DockManager.Clear();
		DockManager.AddDock( null, View );

		MenuBar.Clear();

		{
			var file = MenuBar.AddMenu( "File" );
			file.AddOption( new Option( "Save" ) { Shortcut = "Ctrl+S", Triggered = Save } );
			file.AddSeparator();
			file.AddOption( new Option( "Exit" ) { Triggered = Close } );
		}
	}

	private void Save()
	{
		Saved?.Invoke();
	}

	protected override bool OnClose()
	{
		if ( Instances.TryGetValue( ActionJig, out var inst ) && inst == this )
		{
			Instances.Remove( ActionJig );
		}

		return base.OnClose();
	}
}

public class ActionGraphView : GraphView
{
	public ActionGraphView( Widget parent ) : base( parent )
	{
	}

	private static IEnumerable<INodeType> GetInstanceNodes( TypeDescription typeDesc )
	{
		foreach ( var propertyDesc in typeDesc.Properties )
		{
			if ( propertyDesc.CanRead )
			{
				yield return new PropertyNodeType( propertyDesc, PropertyNodeKind.Get );
			}

			if ( propertyDesc.CanWrite )
			{
				yield return new PropertyNodeType( propertyDesc, PropertyNodeKind.Set );
			}
		}
	}

	protected override IEnumerable<INodeType> GetRelevantNodes( Type inputValueType )
	{
		var baseNodes = base.GetRelevantNodes( inputValueType );
		var typeDesc = EditorTypeLibrary.GetType( inputValueType );

		if ( typeDesc == null )
		{
			return baseNodes;
		}

		return GetInstanceNodes( typeDesc ).Concat( baseNodes );
	}

	private static Dictionary<Type, HandleConfig> HandleConfigs { get; } = new()
	{
		{ typeof(OutputSignal), new HandleConfig( "Signal", Color.White, HandleShape.Arrow ) },
		{ typeof(Task), new HandleConfig( "Signal", Color.White, HandleShape.Arrow ) },
		{ typeof(GameObject), new HandleConfig( null, Theme.Blue ) },
		{ typeof(GameObjectComponent), new HandleConfig( null, Theme.Green ) },
		{ typeof(float), new HandleConfig( "Float", Color.Parse( "#8ec07c" )!.Value ) },
		{ typeof(int), new HandleConfig( "Int", Color.Parse( "#ce67e0" )!.Value ) },
		{ typeof(bool), new HandleConfig( "Bool", Color.Parse( "#e0d867" )!.Value ) },
		{ typeof(Vector3), new HandleConfig( "Vector3", Color.Parse( "#7177e1" )!.Value ) },
		{ typeof(string), new HandleConfig( "String", Color.Parse( "#c7ae32" )!.Value ) }
	};

	protected override HandleConfig OnGetHandleConfig( Type type )
	{
		if ( HandleConfigs.TryGetValue( type, out var config ) )
		{
			return config;
		}

		if ( type.BaseType != null )
		{
			return OnGetHandleConfig( type.BaseType );
		}

		return base.OnGetHandleConfig( type );
	}
}