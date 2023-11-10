using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Terrain;

[Title( "Terrain" )]
[Category( "Rendering" )]
[Icon( "terrain" )]
public class TerrainComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	SceneObject _sceneObject;
	public SceneObject SceneObject => _sceneObject;

	/// <summary>
	/// This needs to be a material that uses the terrain shader.
	/// </summary>
	[Property] public Material Material { get; set; }

	[Property] public Texture HeightMap { get; set; }
	[Property] public Texture SplatMap0 { get; set; }
	[Property] public Texture SplatMap1 { get; set; }

	[Property] public float MaxHeightInInches { get; set; } = 40000.0f;
	[Property] public float TerrainResolutionInInches { get; set; } = 39.0f;

	[Property] public int ClipMapLodLevels { get; set; } = 7;
	[Property] public int ClipMapLodExtentTexels { get; set; } = 128;

	[Property] public DebugViewEnum DebugView { get; set; } = DebugViewEnum.None;


	/// <summary>
	/// Hack to sample 16 bit depth from RB rust heightmap
	/// </summary>
	[Property] public bool IsRustHeightmap { get; set; }

	Model _model;

	int vertexCount = 0;
	int indexCount = 0;

	public override void OnEnabled()
	{
		Assert.True( _sceneObject == null );
		Assert.NotNull( Scene );

		{
			var clipmapMesh = GeometryClipmap.GenerateMesh_DiamondSquare( ClipMapLodLevels, ClipMapLodExtentTexels, Material );
			_model = Model.Builder.AddMesh( clipmapMesh ).Create();

			vertexCount = clipmapMesh.VertexCount;
			indexCount = clipmapMesh.IndexCount;
		}

		_sceneObject = new SceneObject( Scene.SceneWorld, _model, Transform.World );
		_sceneObject.Tags.SetFrom( GameObject.Tags );
		_sceneObject.Batchable = false;
	}

	public override void OnDisabled()
	{
		_sceneObject?.Delete();
		_sceneObject = null;
	}

	protected override void OnPreRender()
	{
		if ( !_sceneObject.IsValid() )
			return;

		_sceneObject.Transform = Transform.World;
		_sceneObject.Attributes.Set( "Heightmap", HeightMap );
		_sceneObject.Attributes.Set( "SplatMap0", SplatMap0 );
		_sceneObject.Attributes.Set( "SplatMap1", SplatMap1 );
		_sceneObject.Attributes.Set( "HeightScale", MaxHeightInInches );
		_sceneObject.Attributes.Set( "TerrainResolution", TerrainResolutionInInches );
		_sceneObject.Attributes.Set( "DebugView", (int)DebugView );
		_sceneObject.Attributes.Set( "IsRustHeightmap", IsRustHeightmap );
	}

	public override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Black;
		Gizmo.Draw.ScreenText( $"Terrain Size: {HeightMap.Width * TerrainResolutionInInches} x {HeightMap.Height * TerrainResolutionInInches} ( {(HeightMap.Width * TerrainResolutionInInches).InchToMeter()}m² )", Vector2.One * 16, size: 16, flags: TextFlag.Left );
		Gizmo.Draw.ScreenText( $"Clipmap Lod Levels: {ClipMapLodLevels} covering {ClipMapLodExtentTexels} texels", Vector2.One * 16 + Vector2.Up * 24, size: 16, flags: TextFlag.Left );
		Gizmo.Draw.ScreenText( $"Clipmap Mesh: {vertexCount.KiloFormat()} verticies {(indexCount / 3).KiloFormat()} triangles", Vector2.One * 16 + Vector2.Up * 48, size: 16, flags: TextFlag.Left );
	}

	public enum DebugViewEnum
	{
		None = 0,
		Normal = 1,
		NormalZ = 2,
		Height = 3,
		LOD = 4,
		Splat = 5,
	}

	public BBox Bounds
	{
		get
		{
			if ( _sceneObject is not null )
			{
				return _sceneObject.Bounds;
			}

			return new BBox( Transform.Position, 16 );
		}
	}
}
