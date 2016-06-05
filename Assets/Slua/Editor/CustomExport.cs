// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SLua
{
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEditor;
	using System.Collections;
    using System.Collections.Generic;
    using System;

	// using DG.Tweening;
	// using DG.Tweening.Core;
	// using DG.Tweening.Plugins.Core.PathCore;
	// using DG.Tweening.Plugins.Options;


    public class CustomExport
    {
        public static void OnGetAssemblyToGenerateExtensionMethod(out List<string> list) {
            list = new List<string> {
                "Assembly-CSharp",
				// "DOTween",
				// "DOTween43",
				// "DOTween46",
				// "DOTween50",
            };
        }

		public static void OnAddCustomAssembly(ref List<string> list)
		{
			// add your custom assembly here
			// you can build a dll for 3rd library like ngui titled assembly name "NGUI", put it in Assets folder
			// add its name into list, slua will generate all exported interface automatically for you

			// list.Add("DOTween");
		}

        public static void OnAddCustomClass(LuaCodeGen.ExportGenericDelegate add)
        {
            add (typeof(System.Func<int>), null);
            add (typeof(System.Action<int, string>), null);
            add (typeof(System.Action<int, Dictionary<int, object>>), null);
            add (typeof(List<int>), "ListInt");
            add (typeof(Dictionary<int, string>), "DictIntStr");
            add (typeof(string), "String");

			add (typeof(Dictionary<string, object>), "DictStrObj");
			add (typeof(List<object>), "ListObj");
            
            // add your custom class here
            // add( type, typename)
            // type is what you want to export
            // typename used for simplify generic type name or rename, like List<int> named to "ListInt", if not a generic type keep typename as null or rename as new type name

			// // DOTween generic
			// add (typeof(TweenerCore<int, int, NoOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreInt");
			// add (typeof(TweenerCore<uint, uint, UintOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreUInt");
			// add (typeof(TweenerCore<long, long, NoOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreLong");
			// add (typeof(TweenerCore<ulong, ulong, NoOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreULong");
			// add (typeof(TweenerCore<float, float, FloatOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreFloat");
			// add (typeof(TweenerCore<double, double, NoOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreDouble");

			// add (typeof(TweenerCore<string, string, StringOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreStr");

			// add (typeof(TweenerCore<Vector2, Vector2, VectorOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreVec2");
			// add (typeof(TweenerCore<Vector3, Vector3, VectorOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreVec3");
			// add (typeof(TweenerCore<Vector4, Vector4, VectorOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreVec4");
			// add (typeof(TweenerCore<Quaternion, Vector3, QuaternionOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreQuat");
			// add (typeof(TweenerCore<Vector3, Vector3[], Vector3ArrayOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreVec3Array");

			// add (typeof(TweenerCore<Color, Color, ColorOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreColor");
			// add (typeof(TweenerCore<Color2, Color2, ColorOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreColor2");

			// add (typeof(TweenerCore<Rect, Rect, RectOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreRect");
			// add (typeof(TweenerCore<RectOffset, RectOffset, NoOptions>), "DG.Tweening.Core.TweenerCore.TweenerCoreRectOffset");
			// add (typeof(TweenerCore<Vector3, Path, PathOptions>), "DG.Tweening.Core.TweenerCore.TweenerCorePath");
        }

        public static HashSet<string> OnAddCustomNamespace()
        {
            return new HashSet<string>
            {
				// "UnityEngine.UI.Extensions",
            };
        }

        // if uselist return a white list, don't check noUseList(black list) again
        public static void OnGetUseList(out List<string> list)
        {
            list = new List<string>
            {
                //"UnityEngine.GameObject",
            };
        }

		// black list if white list not given
		public static void OnGetNoUseList(out List<string> list)
		{
			list = new List<string>
			{      
				"HideInInspector",
				"ExecuteInEditMode",
				"AddComponentMenu",
				"ContextMenu",
				"RequireComponent",
				"DisallowMultipleComponent",
				"SerializeField",
				"AssemblyIsEditorAssembly",
				"Attribute", 
				"Types",
				"UnitySurrogateSelector",
				"TrackedReference",
				"TypeInferenceRules",
				"FFTWindow",
				"RPC",
				"Network",
				"MasterServer",
				"BitStream",
				"HostData",
				"ConnectionTesterStatus",
				"GUI",
				"EventType",
				"EventModifiers",
				"FontStyle",
				"TextAlignment",
				"TextEditor",
				"TextEditorDblClickSnapping",
				"TextGenerator",
				"TextClipping",
				"Gizmos",
				"ADBannerView",
				"ADInterstitialAd",            
				"Android",
				"Tizen",
				"jvalue",
				"iPhone",
				"iOS",
				"CalendarIdentifier",
				"CalendarUnit",
				"CalendarUnit",
				"ClusterInput",
				"FullScreenMovieControlMode",
				"FullScreenMovieScalingMode",
				"Handheld",
				"LocalNotification",
				"NotificationServices",
				"RemoteNotificationType",      
				"RemoteNotification",
				"SamsungTV",
				"TextureCompressionQuality",
				"TouchScreenKeyboardType",
				"TouchScreenKeyboard",
				"MovieTexture",
				"UnityEngineInternal",
				"Terrain",                            
				"Tree",
				"SplatPrototype",
				"DetailPrototype",
				"DetailRenderMode",
				"MeshSubsetCombineUtility",
				"AOT",
				"Social",
				"Enumerator",       
				"SendMouseEvents",               
				"Cursor",
				"Flash",
				"ActionScript",
				"OnRequestRebuild",
				"Ping",
				"ShaderVariantCollection",
				"SimpleJson.Reflection",
				"CoroutineTween",
				"GraphicRebuildTracker",
				"Advertisements",
				"UnityEditor",
				"WSA",
				"EventProvider",
				"Apple",
				"ClusterInput",
			};
		}

        public static List<string> FunctionFilterList = new List<string>()
        {
//            "UIWidget.showHandles",
//            "UIWidget.showHandlesWithMoveTool",
        };

		public static List<string> MemberFilterList = new List<string>
		{
			"AnimationClip.averageDuration",
			"AnimationClip.averageAngularSpeed",
			"AnimationClip.averageSpeed",
			"AnimationClip.apparentSpeed",
			"AnimationClip.isLooping",
			"AnimationClip.isAnimatorMotion",
			"AnimationClip.isHumanMotion",
			"AnimatorOverrideController.PerformOverrideClipListCleanup",
			"Caching.SetNoBackupFlag",
			"Caching.ResetNoBackupFlag",
			"Light.areaSize",
			"Security.GetChainOfTrustValue",
			"Texture2D.alphaIsTransparency",
			"WWW.movie",
			"WebCamTexture.MarkNonReadable",
			"WebCamTexture.isReadable",
			// i don't know why below 2 functions missed in iOS platform
			"*.OnRebuildRequested",
			// il2cpp not exixts
			"Application.ExternalEval",
			"GameObject.networkView",
			"Component.networkView",
			// unity5
			"AnimatorControllerParameter.name",
			"Input.IsJoystickPreconfigured",
			"Resources.LoadAssetAtPath",
#if UNITY_4_6
			"Motion.ValidateIfRetargetable",
			"Motion.averageDuration",
			"Motion.averageAngularSpeed",
			"Motion.averageSpeed",
			"Motion.apparentSpeed",
			"Motion.isLooping",
			"Motion.isAnimatorMotion",
			"Motion.isHumanMotion",
#endif
		};

    }
}