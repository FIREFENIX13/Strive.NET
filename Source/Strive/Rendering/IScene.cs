using System;
using System.Threading;

using Strive.Rendering.Models;
using Strive.Rendering.Textures;
using Strive.Math3D;

namespace Strive.Rendering
{

	/// <summary>
	/// Represent a Scene.  A scene contains everything that will be rendered.
	/// </summary>
	public interface IScene
	{
		#region "Methods"
		void DropAll();
		void SetSky( string name, ITexture texture );
		void SetLighting( short level );
		void SetFog( float level );
		void DrawText( Vector2D location, string message );
		/// <remarks>Renders the scene into video memory.</remarks>
		void Render();

		/// <remarks>Displays the rendered screen.</remarks>
		void Display();

		int RayCollision(
			Vector3D start_point, Vector3D end_point, int collision_type
		);

		#endregion
 
		#region "Properties"
		/// <summary>
		/// Indiactes whether the scene is being rendered
		/// </summary>
		bool IsRendering { get; }

		/// <summary>
		/// Model collection
		/// </summary>
		IModelCollection Models { get; }

		/// <summary>
		/// Returns the View of the current scene.
		/// </summary>
		Cameras.ICameraCollection Views { get; }

		/// <summary>
		/// Returns the default view
		/// </summary>
		Cameras.ICamera View	{ get; }
		

		#endregion

	}
}
