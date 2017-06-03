using UnityEngine;
using System.Collections;

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_6
	using UnityEngine.Rendering;
#endif



public enum suiCamToolType{
	transparent,transparentCaustic,wakeEffects,normals,depthMask,localReflection,
	underwaterMask,underwater,shorelineObject,shorelineCapture
};
	

public enum suiCamToolRender{
	automatic,deferredShading,deferredLighting,forward
};

public enum suiCamHdrMode{
	off,on,automatic
};



namespace Suimono.Core
{


	[ExecuteInEditMode]
	public class cameraTools : MonoBehaviour {

		//Public Variables
		public suiCamToolType cameraType;
		public suiCamToolRender renderType = suiCamToolRender.automatic;
		public suiCamHdrMode hdrMode = suiCamHdrMode.off;

		public int resolution = 256;
		public float cameraOffset = 0f;
		public RenderTexture renderTexDiff;
		public Shader renderShader;
		public bool executeInEditMode = false;
		public bool isUnderwater = false;

		[HideInInspector] public Renderer surfaceRenderer;
		[HideInInspector] public Renderer scaleRenderer;
		[HideInInspector] public float reflectionDistance = 200.0f;
		[HideInInspector] public int setLayers;


		//Private Variables
		private RenderingPath usePath;
		private Suimono.Core.SuimonoModule suimonoModuleObject;
		private Camera cam;
		private Camera copyCam;
		private int currResolution = 256;

		//Collect variables for reflection
		private float clipPlaneOffset = 0.07f;

		//Collect variables for GC
		private Vector3 pos;
		private Vector3 normal;
		private float d;
		private Vector4 reflectionPlane;
		private Matrix4x4 reflection;
		private Vector3 oldpos;
		private Vector3 newpos;
		private Vector4 clipPlane;
		private Matrix4x4 projection;
		private Vector3 euler;
		private Matrix4x4 scaleOffset;
		private Vector3 scale;
		private Matrix4x4 mtx;
		private Vector3 offsetPos;
		private Matrix4x4 m;
		private Vector3 cpos;
		private Vector3 cnormal;
		private Matrix4x4 proj;
		private Vector4 q;
		private Vector4 c;
		private float hasStarted = 0f;



		void Start () {

			//Get object references
			if (GameObject.Find("SUIMONO_Module") != null){
				suimonoModuleObject = GameObject.Find("SUIMONO_Module").gameObject.GetComponent<Suimono.Core.SuimonoModule>();
			}

			if (cameraType != suiCamToolType.localReflection){
				if (transform.parent != null) surfaceRenderer = transform.parent.gameObject.GetComponent<Renderer>();
			} else {
				if (transform.parent != null){
					surfaceRenderer = transform.parent.Find("Suimono_Object").gameObject.GetComponent<Renderer>();
					scaleRenderer = transform.parent.Find("Suimono_ObjectScale").gameObject.GetComponent<Renderer>();
				}
			}

			cam = gameObject.GetComponent<Camera>() as Camera;
			if (suimonoModuleObject != null && suimonoModuleObject.setCamera != null){
				copyCam = suimonoModuleObject.setCamera.GetComponent<Camera>();
			}

			UpdateRenderTex();
			CameraUpdate();
		}
		



		void OnPreRender(){
			if (Application.isPlaying && cameraType == suiCamToolType.localReflection){
				GL.invertCulling = true;
			}
		}

		void OnPostRender(){
			if (Application.isPlaying){
				GL.invertCulling = false;
			}
		}

		void Update(){

			//update shoreline camera during edit mode
			if (!Application.isPlaying && executeInEditMode){
				CameraUpdate();
			}

			//set layermasks
			if (cam != null){
				if (cameraType == suiCamToolType.shorelineCapture){
					cam.cullingMask = 1 << suimonoModuleObject.layerDepthNum;
				}
			}
		}

		void LateUpdate(){
			if (Application.isPlaying){
				if (cameraType == suiCamToolType.shorelineObject){
					if (hasStarted == 0f && Time.time > 0.2f){
						CameraUpdate();
						hasStarted = 1f;
					}
				} else {
					CameraUpdate();
				}
			}
		}




		void CameraRender(){

			//Setup Camera Matrices
			if (cameraType == suiCamToolType.localReflection){
				ReflectionPreRender();
			}

			//RENDER CAMERA
			cam.targetTexture = renderTexDiff;
			if (Application.isPlaying && cameraType == suiCamToolType.shorelineObject){
				cam.enabled = false;
				cam.Render();
			} else {
				cam.enabled = true;
			}

		    //Reset Camera Properties
			if (cameraType == suiCamToolType.localReflection) ReflectionPostRender();
		}





		public void CameraUpdate() {

			if (suimonoModuleObject != null){
				if (suimonoModuleObject.setCameraComponent != null){
					copyCam = suimonoModuleObject.setCameraComponent;
				}
			}

			if (copyCam != null && cam != null){

				//set camera settings
				if (cameraType != suiCamToolType.shorelineObject){
					cam.transform.position = copyCam.transform.position;
					cam.transform.rotation = copyCam.transform.rotation;
					cam.projectionMatrix = copyCam.projectionMatrix;;
					cam.fieldOfView = copyCam.fieldOfView;
				}

				//re-project camera for screen-space effects
				if (cameraOffset != 0.0f){
					cam.transform.Translate(Vector3.forward * cameraOffset);
				}

					//select rendering path
					if (renderType == suiCamToolRender.automatic){
						usePath = copyCam.actualRenderingPath;

						//specific settings for transparent camera
						if (cameraType == suiCamToolType.transparent){
							if (copyCam.renderingPath == RenderingPath.Forward){
								usePath = RenderingPath.DeferredLighting;
							} else {
								usePath = copyCam.renderingPath;
							}
						}
					
					} else if (renderType == suiCamToolRender.deferredShading){
						usePath = RenderingPath.DeferredShading;

					} else if (renderType == suiCamToolRender.deferredLighting){
						usePath = RenderingPath.DeferredLighting;

					} else if (renderType == suiCamToolRender.forward){
						usePath = RenderingPath.Forward;
					}

					//set effect rendering path
					cam.renderingPath = usePath;


				if (renderTexDiff != null){

					//update texture resolution
					if (resolution != currResolution){
						currResolution = resolution;
						UpdateRenderTex();
					}

					//render custom normal effects shader
					if (cameraType == suiCamToolType.normals){
						if (suimonoModuleObject.enableAdvancedDistort){
							cam.hdr = false;
							cam.SetReplacementShader(renderShader,"RenderType");
							CameraRender();
						} else {
							renderTexDiff = null;
						}

					//render customwake effects shader
					} else if (cameraType == suiCamToolType.wakeEffects){
						if (suimonoModuleObject.enableAdvancedDistort){
							cam.SetReplacementShader(renderShader,"RenderType");
							CameraRender();
						} else {
							renderTexDiff = null;
						}

					//render transparency effects
					} else if (cameraType == suiCamToolType.transparent){
						if (suimonoModuleObject.enableTransparency){
							CameraRender();
						} else {
							renderTexDiff = null;
						}

					//render caustics effects
					} else if (cameraType == suiCamToolType.transparentCaustic){
						if (suimonoModuleObject.enableCaustics){
							CameraRender();
						} else {
							renderTexDiff = null;
						}

					} else {
						CameraRender();
					}
					


					//pass texture to shader
					if (cameraType == suiCamToolType.transparent){
						Shader.SetGlobalTexture("_suimono_TransTex",renderTexDiff);
						if (!suimonoModuleObject.enableCausticsBlending) Shader.SetGlobalTexture("_suimono_CausticTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.transparentCaustic){
						if (suimonoModuleObject.enableCausticsBlending) Shader.SetGlobalTexture("_suimono_CausticTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.wakeEffects){
						Shader.SetGlobalTexture("_suimono_WakeTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.normals){
						Shader.SetGlobalTexture("_suimono_NormalsTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.depthMask){
						Shader.SetGlobalTexture("_suimono_depthMaskTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.underwaterMask){
						Shader.SetGlobalTexture("_suimono_underwaterMaskTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.underwater){
						Shader.SetGlobalTexture("_suimono_underwaterTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.localReflection){
						if (surfaceRenderer != null) surfaceRenderer.sharedMaterial.SetTexture("_ReflectionTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.shorelineObject){
						if (surfaceRenderer != null) surfaceRenderer.sharedMaterial.SetTexture("_MainTex",renderTexDiff);
					}
					if (cameraType == suiCamToolType.shorelineCapture){
						Shader.SetGlobalTexture("_suimono_shorelineTex",renderTexDiff);
					}

				} else {
					UpdateRenderTex();
				}
				
			}

		}



		void UpdateRenderTex(){

			if (resolution < 4) resolution = 4;
				
			if (renderTexDiff != null){
				if (cam != null) cam.targetTexture = null;
				DestroyImmediate(renderTexDiff);
			}
			//renderTexDiff = new RenderTexture(resolution,resolution,24,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);
			//renderTexDiff = new RenderTexture(resolution,resolution,24,RenderTextureFormat.DefaultHDR,RenderTextureReadWrite.Linear);
			renderTexDiff = new RenderTexture(resolution,resolution,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);

			#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_6
				renderTexDiff.dimension = TextureDimension.Tex2D;
			#else
				renderTexDiff.isCubemap = false;
			#endif

			renderTexDiff.autoGenerateMips = false;
			renderTexDiff.anisoLevel = 1;
			renderTexDiff.filterMode = FilterMode.Trilinear;
			renderTexDiff.wrapMode = TextureWrapMode.Clamp;
		}



		void ReflectionPreRender(){

		    // find out the reflection plane: position and normal in world space
		    pos = transform.parent.position;

		    if (isUnderwater){
		    	normal = -transform.parent.transform.up; //underwater
			} else {
				normal = transform.parent.transform.up; //above water
			}

		    //set camera properties
		    cam.CopyFrom(copyCam);

		    //turn hdr off
		    if (hdrMode == suiCamHdrMode.off){
		    	cam.hdr = false;
		    } else if (hdrMode == suiCamHdrMode.on){
		    	cam.hdr = true;
		    }



			if (isUnderwater){
		    	cam.farClipPlane = 3;
		    	cam.clearFlags = CameraClearFlags.Color;
		    	cam.depthTextureMode = DepthTextureMode.Depth;
			} else {
				//cam.farClipPlane = reflectionDistance;
				//cam.clearFlags = CameraClearFlags.Skybox;
			}

			//render transparency effects
			if (cameraType == suiCamToolType.localReflection){
				if (renderShader != null){
					cam.SetReplacementShader(renderShader,null);

				}
			}


			cam.cullingMask = setLayers;

		    // Render reflection
		    // Reflect camera around reflection plane
		    d = -Vector3.Dot (normal, pos) - clipPlaneOffset;
		    reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
		 
		    reflection  = Matrix4x4.zero;
			reflection = Set_CalculateReflectionMatrix (reflectionPlane);

		    oldpos = copyCam.transform.position;
		    newpos = reflection.MultiplyPoint( oldpos );
		    cam.worldToCameraMatrix = copyCam.worldToCameraMatrix * reflection;
		 
		    // Setup oblique projection matrix so that near plane is our reflection
		    // plane. This way we clip everything below/above it for free.
		    clipPlane = Set_CameraSpacePlane(cam, pos, normal, 1f);
		    projection = copyCam.projectionMatrix;
		    projection = Set_CalculateObliqueMatrix (clipPlane);
		    cam.projectionMatrix = projection;

			GL.invertCulling = true;

			cam.transform.position = newpos;
		    euler = copyCam.transform.eulerAngles;
		    cam.transform.eulerAngles = new Vector3(0f, euler.y, euler.z);
		}


		void ReflectionPostRender(){
		    cam.transform.position = oldpos;
			GL.invertCulling = false;
		    scaleOffset = Matrix4x4.TRS(new Vector3(0.5f,0.5f,0.5f), Quaternion.identity, new Vector3(0.5f,0.5f,0.5f) );
		    scale = transform.lossyScale;
		    mtx = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1.0f/scale.x, -1.0f/scale.y, 1.0f/scale.z) );
		    mtx = scaleOffset * copyCam.projectionMatrix * copyCam.worldToCameraMatrix * mtx;
		}


		public float Set_sgn(float a){
		    if (a > 0.0f) return 1.0f;
		    if (a < 0.0f) return -1.0f;
		    return 0.0f;
		}


		public Vector4 Set_CameraSpacePlane (Camera cm, Vector3 pos, Vector3 normal, float sideSign) {
		    offsetPos = pos + normal * (clipPlaneOffset);
		    m = cm.worldToCameraMatrix;
		    cpos = m.MultiplyPoint( offsetPos );
		    cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		    return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
		}


		public Matrix4x4 Set_CalculateObliqueMatrix (Vector4 clipPlane) {
			proj = copyCam.projectionMatrix;
		    q = proj.inverse * new Vector4(Set_sgn(clipPlane.x),Set_sgn(clipPlane.y),1f,1f);
		    c = clipPlane * (2f / (Vector4.Dot (clipPlane, q)));
		    proj[2] = c.x - proj[3];
		    proj[6] = c.y - proj[7];
		    proj[10] = c.z - proj[11];
		    proj[14] = c.w - proj[15];
		    return proj;
		}


		public Matrix4x4 Set_CalculateReflectionMatrix (Vector4 plane) {
			
			var reflectionMat = Matrix4x4.zero;
		    
		    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

		    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);

		    reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		    reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		    reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		    reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

		    reflectionMat.m30 = 0F;
		    reflectionMat.m31 = 0F;
		    reflectionMat.m32 = 0F;
		    reflectionMat.m33 = 1F;

		    return reflectionMat;
		}



	}
}