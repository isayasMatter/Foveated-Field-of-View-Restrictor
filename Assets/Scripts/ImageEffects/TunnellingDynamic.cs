using UnityEngine;
using System.Collections;

namespace Sigtrap.ImageEffects {
	[RequireComponent(typeof(Camera))]
	public class TunnellingDynamic : MonoBehaviour {
		#region Public Fields
		[Tooltip("Remove for plain black effect.")]
		public Cubemap skybox;

		[Header("Angular Velocity")]
		/// <summary>
		/// Angular velocity calculated for this Transform. DO NOT USE HMD!
		/// </summary>
		[Tooltip("Angular velocity calculated for this Transform.\nDO NOT USE HMD!")]
		public Transform refTransform;

		/// <summary>
		/// Below this angular velocity, effect will not kick in. Degrees per second
		/// </summary>
		[Tooltip("Below this angular velocity, effect will not kick in.\nDegrees per second")]
		public float minAngVel = 0f;

		/// <summary>
		/// At/above this angular velocity, effect will be maxed out. Degrees per second
		/// </summary>
		[Tooltip("At/above this angular velocity, effect will be maxed out.\nDegrees per second")]
		public float maxAngVel = 180f;

		/// <summary>
		/// Below this speed, effect will not kick in.
		/// </summary>
		[Tooltip("Below this speed, effect will not kick in.")]
		public float minSpeed = 0f;

		/// <summary>
		/// At/above this speed, effect will be maxed out.
		/// </summary>
		[Tooltip("At/above this speed, effect will be maxed out.\nSet negative for no effect.")]
		public float maxSpeed = -1f;

		[Header("Effect Settings")]
		/// <summary>
		/// Screen coverage at max angular velocity.
		/// </summary>
		[Range(0f,1f)][Tooltip("Screen coverage at max angular velocity.\n(1-this) is radius of visible area at max effect (screen space).")]
		public float maxEffect = 0.75f;

		/// <summary>
		/// Feather around cut-off as fraction of screen.
		/// </summary>
		[Range(0f, 0.5f)][Tooltip("Feather around cut-off as fraction of screen.")]
		public float feather = 0.1f;

		/// <summary>
		/// Tunneling mask offset from center of screen.
		/// </summary>
		[Range(0f, 1f)][Tooltip("Tunneling mask offset x from center of screen.")]
		public static float offsetx = 0.5f;

		[Range(0f, 1f)][Tooltip("Tunneling mask offset y from center of screen.")]
		public static float offsety = 0.5f;

		/// <summary>
		/// Smooth out radius over time. 0 for no smoothing.
		/// </summary>
		[Tooltip("Smooth out radius over time. 0 for no smoothing.")]
		public float smoothTime = 0.15f;
		#endregion

		#region Smoothing
		private float _avSlew;
		private float _av;
		#endregion

		#region Shader property IDs
		private int _propAV;
		private int _propFeather;
		private int _propOffsetX;
		private int _propOffsetY;
		#endregion

		#region Eye matrices
		Matrix4x4[] _eyeToWorld = new Matrix4x4[2];
		Matrix4x4[] _eyeProjection = new Matrix4x4[2];
		#endregion

		#region Misc Fields
		private Vector3 _lastFwd;
		private Vector3 _lastPos;
		private Material _m;
		private Camera _cam;
		#endregion

		#region Messages
		void Awake () {
			_m = new Material(Shader.Find("Hidden/TunnellingDynamic"));

			if (refTransform == null){
				refTransform = transform;
			}

			_propAV = Shader.PropertyToID("_AV");
			_propFeather = Shader.PropertyToID("_Feather");
			_propOffsetX = Shader.PropertyToID("_OffsetX");
			_propOffsetY = Shader.PropertyToID("_OffsetY");

			_cam = GetComponent<Camera>();
		}

		void Update(){
			Vector3 fwd = refTransform.forward;
			float av = Vector3.Angle(_lastFwd, fwd) / Time.deltaTime;
			av = (av - minAngVel) / (maxAngVel - minAngVel);

			Vector3 pos = refTransform.position;

			if (maxSpeed > 0) {
				float speed = (pos - _lastPos).magnitude / Time.deltaTime;
				speed = (speed - minSpeed) / (maxSpeed - minSpeed);
				//speed = 4.2f;
				if (speed > av) {
					av = speed;
				}
			}

			av = Mathf.Clamp01(av) * maxEffect;

			_av = Mathf.SmoothDamp(_av, av, ref _avSlew, smoothTime);

			offsetx = (offsetx/2.0f) + 0.5f;
			offsety = (offsety/2.0f) + 0.5f;

			float _offset_x = Mathf.Clamp(offsetx, 0.5f,0.6f);
			float _offset_y = Mathf.Clamp(offsety, 0.4f,0.6f);

			_m.SetFloat(_propAV, _av);
			_m.SetFloat(_propFeather, feather);
			_m.SetFloat(_propOffsetX, _offset_x);
			_m.SetFloat(_propOffsetY, _offset_y);

			//Debug.Log("Shader offset properties: " + _offset_x + ", " + _offset_y);

			_lastFwd = fwd;
			_lastPos = pos;
		}

		void OnPreRender(){
			// Update eye matrices
			Matrix4x4 local;
			#if UNITY_2017_2_OR_NEWER
			if (UnityEngine.XR.XRSettings.enabled) {
			#else
			if (UnityEngine.VR.VRSettings.enabled) {
			#endif
				local = _cam.transform.parent.worldToLocalMatrix;
			} else {
				local = Matrix4x4.identity;
			}

			_eyeProjection[0] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			_eyeProjection[1] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			_eyeProjection[0] = GL.GetGPUProjectionMatrix(_eyeProjection[0], true).inverse;
			_eyeProjection[1] = GL.GetGPUProjectionMatrix(_eyeProjection[1], true).inverse;
			
			_eyeProjection[0][1, 1] *= -1f;
			_eyeProjection[1][1, 1] *= -1f;

			// Hard-code far clip
			_eyeProjection[0][3, 3] = 0.001f;
			_eyeProjection[1][3, 3] = 0.001f;

			_eyeToWorld[0] = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			_eyeToWorld[1] = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

			_eyeToWorld[0] = local * _eyeToWorld[0].inverse;
			_eyeToWorld[1] = local * _eyeToWorld[1].inverse;

			_m.SetMatrixArray("_EyeProjection", _eyeProjection);
			_m.SetMatrixArray("_EyeToWorld", _eyeToWorld);

			// Update skybox
			// if (skybox){
			// 	_m.SetTexture("_Skybox", skybox);
			// 	_m.EnableKeyword("TUNNEL_SKYBOX");
			// } else {
			// 	_m.DisableKeyword("TUNNEL_SKYBOX");
			// }
			_m.DisableKeyword("TUNNEL_SKYBOX");
		}

		void OnRenderImage(RenderTexture src, RenderTexture dest){
			Graphics.Blit(src, dest, _m);
			
		}

		void OnDestroy(){
			Destroy(_m);
		}
		#endregion
	}
}
