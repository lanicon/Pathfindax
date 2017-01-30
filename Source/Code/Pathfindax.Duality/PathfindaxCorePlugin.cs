﻿using System;
using System.Linq;
using Duality;
using Duality.Resources;
using Pathfindax.Duality.Components;

namespace Pathfindax.Duality
{
	/// <summary>
	/// Defines a Duality core plugin.
	/// </summary>
	public class PathfindaxCorePlugin : CorePlugin
	{
		private static IPathfinderComponent[] _pathfinderComponents;

		public PathfindaxCorePlugin()
		{
			Scene.Leaving += Scene_Leaving;
		}

		/// <summary>
		/// Gets the <see cref="IPathfinderComponent"/> with the same id. If the <paramref name="id"/> parameter is null then the first pathfinder will be returned.
		/// If there are multiple pathfinders and no id was supplied this will throw a exception.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		internal static IPathfinderComponent GetPathfinder(string id = null)
		{
			if (_pathfinderComponents == null)
			{
				_pathfinderComponents = Scene.Current.FindComponents<IPathfinderComponent>().ToArray();
			}

			if (string.IsNullOrEmpty(id))
			{
				try
				{
					return _pathfinderComponents.SingleOrDefault();
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("If there are multiple pathfinders you have to supply a unique id", ex);
				}
			}
			else
			{
				try
				{
					return _pathfinderComponents.SingleOrDefault(x => x.PathfinderId == id);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("2 pathfinders found with the same id", ex);
				}
			}
		}

		protected override void OnBeforeUpdate()
		{
			base.OnBeforeUpdate();
			if (_pathfinderComponents != null)
			{
				foreach (var pathfinderComponent in _pathfinderComponents)
				{
					pathfinderComponent.ProcessPaths();
				}
			}
		}

		protected override void OnDisposePlugin()
		{
			Scene.Leaving -= Scene_Leaving;
			base.OnDisposePlugin();			
		}

		private static void Scene_Leaving(object sender, EventArgs e)
		{
			_pathfinderComponents = null;
		}
	}
}