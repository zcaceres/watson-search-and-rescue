using UnityEngine;
using System.Collections;


namespace Suimono.Core
{

	public class SuimonoModuleLib : MonoBehaviour {

		public Texture2D texNormalC;
		public Texture2D texNormalT;
		public Texture2D texNormalR;

		public Texture2D texFoam;
		public Texture2D texRampWave;
		public Texture2D texRampDepth;
		public Texture2D texRampBlur;
		public Texture2D texRampFoam;
		public Texture2D texWave;
		public Cubemap texCube1;
		public Texture2D texBlank;
		public Texture2D texMask;

		public Texture2D texDrops;

		public Material materialSurface;
		public Material materialSurfaceScale;
		public Material materialSurfaceShadow;

		public GameObject surfaceObject;
		public GameObject moduleObject;

		public Mesh[] meshLevel;
		public Shader[] shaderRepository;
		public TextAsset[] presetRepository;

	}
}
