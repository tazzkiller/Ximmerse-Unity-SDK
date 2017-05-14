//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Ximmerse {

	public class PostBuildProcessor{

		internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
		{
			if (Directory.Exists(dstPath))
				Directory.Delete(dstPath);
			if (File.Exists(dstPath))
				File.Delete(dstPath);

			Directory.CreateDirectory(dstPath);

			foreach (var file in Directory.GetFiles(srcPath))
				File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

			foreach (var dir in Directory.GetDirectories(srcPath))
				CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
		}

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {

			if (buildTarget == BuildTarget.iOS) {
				string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

				PBXProject proj = new PBXProject();
				proj.ReadFromString(File.ReadAllText(projPath));

				string target = proj.TargetGuidByName("Unity-iPhone");

				// Set a custom link flag
				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-framework CoreBluetooth");
				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-framework IOKit");

				File.WriteAllText(projPath, proj.WriteToString());
				Debug.Log("added liner flag");
			}
		}
	}
}
#endif