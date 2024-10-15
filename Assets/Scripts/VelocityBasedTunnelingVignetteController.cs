using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Provides methods for <see cref="ITunnelingVignetteProvider"/> components to control the tunneling vignette material.
    /// </summary>
    [AddComponentMenu("XR/Locomotion/Velocity-Based Tunneling Vignette Controller", 11)]
    public class VelocityBasedTunnelingVignetteController : MonoBehaviour
    {
        public static readonly VignetteParameters defaultEffect = new VignetteParameters
        {
            apertureSize = 0.7f,
            featheringEffect = 0.2f,
            easeInTime = 0.3f,
            easeOutTime = 0.3f,
            easeInTimeLock = false,
            easeOutDelayTime = 0f,
            vignetteColor = Color.black,
            vignetteColorBlend = Color.black,
            apertureVerticalPosition = 0f
        };

        public static readonly VignetteParameters noEffect = new VignetteParameters
        {
            apertureSize = 1f,
            featheringEffect = 0f,
            easeInTime = 0f,
            easeOutTime = 0f,
            easeInTimeLock = false,
            easeOutDelayTime = 0f,
            vignetteColor = Color.black,
            vignetteColorBlend = Color.black,
            apertureVerticalPosition = 0f
        };

        const string k_DefaultShader = "VR/TunnelingVignette";

        static class ShaderPropertyLookup
        {
            public static readonly int apertureSize = Shader.PropertyToID("_ApertureSize");
            public static readonly int featheringEffect = Shader.PropertyToID("_FeatheringEffect");
            public static readonly int vignetteColor = Shader.PropertyToID("_VignetteColor");
            public static readonly int vignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");
        }
        

        // Velocity-based tunneling vignette

        [SerializeField]
        CharacterController m_CharacterController;

        Vector3 m_PreviousPosition = Vector3.zero;

        [SerializeField]
        float m_VelocityThreshold = 0.1f;


        // Vignette parameters

        [SerializeField]
        VignetteParameters m_DefaultParameters = new VignetteParameters();

        /// <summary>
        /// The default <see cref="VignetteParameters"/> of this <see cref="TunnelingVignetteController"/>.
        /// </summary>
        public VignetteParameters defaultParameters
        {
            get => m_DefaultParameters;
            set => m_DefaultParameters = value;
        }

        [SerializeField]
        VignetteParameters m_CurrentParameters = new VignetteParameters();

        /// <summary>
        /// (Read Only) The current <see cref="VignetteParameters"/> that is controlling the tunneling vignette material.
        /// </summary>
        public VignetteParameters currentParameters => m_CurrentParameters;

        // Vignette state (Replaces individual provider states)
        EaseState easeState;
        float dynamicApertureSize = 1f;
        bool easeInLockEnded = false;
        float dynamicEaseOutDelayTime = 0f;


        // Rendering
        MeshRenderer m_MeshRender;
        MeshFilter m_MeshFilter;
        Material m_SharedMaterial;
        MaterialPropertyBlock m_VignettePropertyBlock;

        public void BeginTunnelingVignette()
        {
            easeState = EaseState.EasingIn;
        }

        public void EndTunnelingVignette()
        {
            easeState = currentParameters.easeInTimeLock
                ? EaseState.EasingInHoldBeforeEasingOut
                : currentParameters.easeOutDelayTime > 0f
                    ? EaseState.EasingOutDelay
                    : EaseState.EasingOut;
        }

        /// <summary>
        /// (Editor Only) Previews a vignette effect in Editor with the given <see cref="VignetteParameters"/>.
        /// </summary>
        /// <param name="previewParameters">The <see cref="VignetteParameters"/> to preview in Editor.</param>
        [Conditional("UNITY_EDITOR")]
        internal void PreviewInEditor(VignetteParameters previewParameters)
        {
            // Avoid previewing when inspecting the prefab asset, which may cause the editor constantly refreshing.
            // Only preview it when it is in the scene or in the prefab window.
            if (!Application.isPlaying && gameObject.activeInHierarchy)
                UpdateTunnelingVignette(previewParameters);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
#if UNITY_EDITOR
            UnityEditor.SceneVisibilityManager.instance.DisablePicking(gameObject, false);
#endif
            m_CurrentParameters.CopyFrom(noEffect);
            UpdateTunnelingVignette(noEffect);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected virtual void Reset()
        {
            m_DefaultParameters.CopyFrom(defaultEffect);
            m_CurrentParameters.CopyFrom(noEffect);
            UpdateTunnelingVignette(m_DefaultParameters);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            // Check if the CharacterController is moving.
            if (m_CharacterController.velocity.magnitude > m_VelocityThreshold)
            {
                BeginTunnelingVignette();
            }
            else
            {
                EndTunnelingVignette();
            }

            // Also detect if the CharacterController is no longer moving.
            if (m_PreviousPosition == m_CharacterController.transform.position)
            {
                EndTunnelingVignette();
            }
            m_PreviousPosition = m_CharacterController.transform.position;


            // Max aperture size for no effect
            const float apertureSizeMax = 1f;

            // Compute dynamic parameter values for all providers and update their records.
            var parameters = defaultParameters;
            var currentSize = dynamicApertureSize;

            switch (easeState)
            {
                case EaseState.NotEasing:
                {
                    dynamicApertureSize = apertureSizeMax;
                    dynamicEaseOutDelayTime = 0f;
                    easeInLockEnded = false;

                    break;
                }

                case EaseState.EasingIn:
                {
                    var desiredEaseInTime = Mathf.Max(parameters.easeInTime, 0f);
                    var desiredEaseInSize = parameters.apertureSize;
                    easeInLockEnded = false;

                    if (desiredEaseInTime > 0f && currentSize > desiredEaseInSize)
                    {
                        var updatedSize = currentSize + (desiredEaseInSize - apertureSizeMax) / desiredEaseInTime * Time.unscaledDeltaTime;
                        dynamicApertureSize = updatedSize < desiredEaseInSize ? desiredEaseInSize : updatedSize;
                    }
                    else
                    {
                        dynamicApertureSize = desiredEaseInSize;
                    }

                    break;
                }

                case EaseState.EasingInHoldBeforeEasingOut:
                {
                    if (!easeInLockEnded)
                    {
                        var desiredEaseInTime = Mathf.Max(parameters.easeInTime, 0f);
                        var desiredEaseInSize = parameters.apertureSize;

                        if (desiredEaseInTime > 0f && currentSize > desiredEaseInSize)
                        {
                            var updatedSize = currentSize + (desiredEaseInSize - apertureSizeMax) / desiredEaseInTime * Time.unscaledDeltaTime;
                            dynamicApertureSize = updatedSize < desiredEaseInSize ? desiredEaseInSize : updatedSize;
                        }
                        else
                        {
                            easeInLockEnded = true;
                            if (parameters.easeOutDelayTime > 0f &&
                                dynamicEaseOutDelayTime < parameters.easeOutDelayTime)
                            {
                                easeState = EaseState.EasingOutDelay;
                                goto case EaseState.EasingOutDelay;
                            }

                            easeState = EaseState.EasingOut;
                            goto case EaseState.EasingOut;
                        }
                    }
                    else
                    {
                        if (parameters.easeOutDelayTime > 0f)
                        {
                            easeState = EaseState.EasingOutDelay;
                            goto case EaseState.EasingOutDelay;
                        }

                        easeState = EaseState.EasingOutDelay;
                        goto case EaseState.EasingOut;
                    }

                    break;
                }

                case EaseState.EasingOutDelay:
                {
                    var currentDelayTime = dynamicEaseOutDelayTime;
                    var desiredEaseOutDelayTime = Mathf.Max(parameters.easeOutDelayTime, 0f);

                    if (desiredEaseOutDelayTime > 0f && currentDelayTime < desiredEaseOutDelayTime)
                    {
                        currentDelayTime += Time.unscaledDeltaTime;

                        dynamicEaseOutDelayTime = currentDelayTime > desiredEaseOutDelayTime
                            ? desiredEaseOutDelayTime
                            : currentDelayTime;
                    }

                    if (dynamicEaseOutDelayTime >= desiredEaseOutDelayTime)
                    {
                        easeState = EaseState.EasingOut;
                        goto case EaseState.EasingOut;
                    }

                    break;
                }

                case EaseState.EasingOut:
                {
                    var desiredEaseOutTime = Mathf.Max(parameters.easeOutTime, 0f);
                    var startSize = parameters.apertureSize;

                    if (desiredEaseOutTime > 0f && currentSize < apertureSizeMax)
                    {
                        var updatedSize = currentSize + (apertureSizeMax - startSize) / desiredEaseOutTime * Time.unscaledDeltaTime;
                        dynamicApertureSize = updatedSize > apertureSizeMax ? apertureSizeMax : updatedSize;
                    }
                    else
                    {
                        dynamicApertureSize = apertureSizeMax;
                    }

                    if (dynamicApertureSize >= apertureSizeMax)
                        easeState = EaseState.NotEasing;

                    break;
                }

                default:
                {
                    Assert.IsTrue(false, $"Unhandled {nameof(EaseState)}={easeState}");
                    break;
                }
            }
            
            m_CurrentParameters.CopyFrom(defaultParameters);
            m_CurrentParameters.apertureSize = dynamicApertureSize;

            // Update the visuals of the tunneling vignette.
            UpdateTunnelingVignette(m_CurrentParameters);
        }

        /// <summary>
        /// Updates the tunneling vignette with the vignette parameters.
        /// </summary>
        /// <param name="parameters">The <see cref="VignetteParameters"/> uses to update the material values.</param>
        /// <remarks>
        /// Use this method with caution when other <see cref="ITunnelingVignetteProvider"/> instances are updating the material simultaneously.
        /// Calling this method will automatically try to set up the material and its renderer for the <see cref="TunnelingVignetteController"/> if it is not set up already.
        /// </remarks>
        void UpdateTunnelingVignette(VignetteParameters parameters)
        {
            if (parameters == null)
                parameters = m_DefaultParameters;

            if (TrySetUpMaterial())
            {
                m_MeshRender.GetPropertyBlock(m_VignettePropertyBlock);
                m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.apertureSize, parameters.apertureSize);
                m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.featheringEffect, parameters.featheringEffect);
                m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColor, parameters.vignetteColor);
                m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColorBlend, parameters.vignetteColorBlend);
                m_MeshRender.SetPropertyBlock(m_VignettePropertyBlock);
            }

            // Update the Transform y-position to match apertureVerticalPosition
            var thisTransform = transform;
            var localPosition = thisTransform.localPosition;
            if (!Mathf.Approximately(localPosition.y, parameters.apertureVerticalPosition))
            {
                localPosition.y = parameters.apertureVerticalPosition;
                thisTransform.localPosition = localPosition;
            }
        }

        bool TrySetUpMaterial()
        {
            if (m_MeshRender == null)
                m_MeshRender = GetComponent<MeshRenderer>();
            if (m_MeshRender == null)
                m_MeshRender = gameObject.AddComponent<MeshRenderer>();

            if (m_VignettePropertyBlock == null)
                m_VignettePropertyBlock = new MaterialPropertyBlock();

            if (m_MeshFilter == null)
                m_MeshFilter = GetComponent<MeshFilter>();
            if (m_MeshFilter == null)
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();

            if (m_MeshFilter.sharedMesh == null)
            {
                Debug.LogWarning("The default mesh for the TunnelingVignetteController is not set. " +
                    "Make sure to import it from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                return false;
            }

            if (m_MeshRender.sharedMaterial == null)
            {
                var defaultShader = Shader.Find(k_DefaultShader);
                if (defaultShader == null)
                {
                    Debug.LogWarning("The default material for the TunnelingVignetteController is not set, and the default Shader: " + k_DefaultShader
                        + " cannot be found. Make sure they are imported from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                    return false;
                }

                Debug.LogWarning("The default material for the TunnelingVignetteController is not set. " +
                    "Make sure it is imported from the Tunneling Vignette Sample of XR Interaction Toolkit. + " +
                    "Try creating a material using the default Shader: " + k_DefaultShader, this);

                m_SharedMaterial = new Material(defaultShader)
                {
                    name = "DefaultTunnelingVignette",
                };
                m_MeshRender.sharedMaterial = m_SharedMaterial;
            }
            else
            {
                m_SharedMaterial = m_MeshRender.sharedMaterial;
            }

            return true;
        }
    }
}
