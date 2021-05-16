using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;

namespace SKReleaseNotes
{
	class Program
	{
		static void Main(string[] args)
		{
			// Initialize StereoKit
			SKSettings settings = new SKSettings
			{
				appName           = "SKReleaseNotes031",
				assetsFolder      = "Assets",
				displayPreference = DisplayMode.MixedReality,
			};
			if (!SK.Initialize(settings))
				Environment.Exit(1);

			// Some tools we'll use, these are pulled from the StereoKit repo
			SK.AddStepper<AvatarSkeleton>();
			//SK.AddStepper(new RenderCamera(new Pose(0,1,0.7f, Quat.Identity), 512, 512));

			// Create a list of scenes for the application, and init the 
			// first one before we begin.
			int          sceneActive = 0;
			List<IScene> scenes = new List<IScene>(new IScene[]{ 
				new SceneWelcome(),
				new SceneControllers(),
				new SceneLighting(),
				new SceneRenderToTex(),
				new SceneUI(),
				new SceneUIBox(),
				new SceneMicrophone(),
				new SceneWand(),
				new SceneSoundInst(),
				new SceneThanks(),
			});
			scenes[sceneActive].Init();

			// Set up some nice lighting, and a background.
			Renderer.SkyTex   = Tex.FromCubemapEquirectangular("adams_place_bridge_1k.hdr", out SphericalHarmonics lighting);
			Renderer.SkyLight = lighting;

			// Set up the scene switching platform assets!
			Model platformModel = Model.FromFile("ButtonPlatform.glb");
			Mesh     button      = platformModel.GetMesh(0);
			Material buttonMat   = platformModel.GetMaterial(0);
			Mesh     platform    = platformModel.GetMesh(1);
			Material platformMat = platformModel.GetMaterial(1);
			Mesh     backplate   = Mesh.GenerateRoundedCube(new Vec3(1.4f, 0.6f, 0.05f), 0.025f);

			// Adjust the camera a bit, so the user isn't standing in the
			// middle of the button
			Renderer.CameraRoot = SK.ActiveDisplayMode == DisplayMode.Flatscreen 
				? Matrix.T(0,1.4f, 0.3f)
				: Matrix.T(0,0,0.2f);


			// Core application loop
			while (SK.Step(() =>
			{
				Hierarchy.Push(World.BoundsPose.ToMatrix());

				// The platform floor
				platform.Draw(platformMat, Matrix.Identity);

				// Draw a backplate to create some contrast with the scene text
				backplate.Draw(Default.Material, Matrix.T(0,1.45f,-0.77f), new Color(0.25f, 0.25f, 0.25f));

				// Show the big red button, and make it interactive
				Pose pose = new Pose(V.XYZ(0, 1.01f, 0), Quat.FromAngles(90, 0, 0));
				UI.WindowBegin("RedButton", ref pose, UIWin.Empty );
				UI.ButtonBehavior(button.Bounds.dimensions.XZ.XY0/2, button.Bounds.dimensions.XZ, "RedButton", out float finger, out BtnState state, out BtnState focus);
				UI.WindowEnd();
				button.Draw(buttonMat, Matrix.T(0, 1-(0.01f-finger), 0));

				Hierarchy.Pop();

				// Advance the scene if the button has been pressed!
				if (state.IsJustActive()) 
				{
					scenes[sceneActive].Shutdown();
					sceneActive = (sceneActive + 1) % scenes.Count;
					scenes[sceneActive].Init();
				}
				scenes[sceneActive].Step();
			}));
			SK.Shutdown();
		}
	}
}
