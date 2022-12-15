﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UIExtensions
{
    [Serializable]
    public class UIEffectSnapshotRequest
    {
        /// <summary>
        /// Effect mode.
        /// </summary>
        public enum EffectMode
        {
            None = 0,
            Grayscale = 1,
            Sepia = 2,
            Nega = 3,
            Pixel = 4,
        }

        /// <summary>
        /// Color effect mode.
        /// </summary>
        public enum ColorMode
        {
            Multiply = 0,
            Fill = 1,
            Add = 2,
            Subtract = 3,
        }

        /// <summary>
        /// Blur effect mode.
        /// </summary>
        public enum BlurMode
        {
            None = 0,
            FastBlur = 1,
            MediumBlur = 2,
            DetailBlur = 3,
        }

        /// <summary>
        /// Down sampling rate.
        /// </summary>
        public enum DownSamplingRate
        {
            None = 0,
            x1 = 1,
            x2 = 2,
            x4 = 4,
            x8 = 8,
        }


        [Tooltip("Effect mode.")] [SerializeField]
        EffectMode m_EffectMode = EffectMode.None;

        [Tooltip("Effect factor between 0(no effect) and 1(complete effect).")] [SerializeField] [Range(0, 1)]
        float m_EffectFactor = 1;

        [Tooltip("Color effect mode.")] [SerializeField]
        ColorMode m_ColorMode = ColorMode.Multiply;

        [Tooltip("Color effect factor between 0(no effect) and 1(complete effect).")] [SerializeField] [Range(0, 1)]
        float m_ColorFactor = 1;

        [Tooltip("Color for the color effect.")] [SerializeField]
        Color m_EffectColor = Color.white;

        [Tooltip("Blur effect mode.")] [SerializeField]
        BlurMode m_BlurMode = BlurMode.FastBlur;

        [Tooltip("How far is the blurring from the graphic.")] [SerializeField] [Range(0, 1)]
        float m_BlurFactor = 1f;

        [Tooltip("Blur iterations.")] [SerializeField] [Range(1, 8)]
        int m_BlurIterations = 2;

        [Tooltip("Down sampling rate of the generated RenderTexture.")] [SerializeField]
        DownSamplingRate m_DownSamplingRate = DownSamplingRate.x2;

        [Tooltip("Down sampling rate of reduction buffer to apply effect.")] [SerializeField]
        DownSamplingRate m_ReductionRate = DownSamplingRate.x2;

        [Tooltip("FilterMode for capturing.")] [SerializeField]
        FilterMode m_FilterMode = FilterMode.Bilinear;

        [Tooltip("Custom materials for post effect.")] [SerializeField]
        Material[] m_CustomMaterials = new Material[0];


        /// <summary>
        /// Effect mode.
        /// </summary>
        public EffectMode effectMode
        {
            get { return m_EffectMode; }
            set { m_EffectMode = value; }
        }

        /// <summary>
        /// Effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float effectFactor
        {
            get { return m_EffectFactor; }
            set { m_EffectFactor = Mathf.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// Color effect mode.
        /// </summary>
        public ColorMode colorMode
        {
            get { return m_ColorMode; }
            set { m_ColorMode = value; }
        }

        /// <summary>
        /// Color effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float colorFactor
        {
            get { return m_ColorFactor; }
            set { m_ColorFactor = Mathf.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// Color for the color effect.
        /// </summary>
        public Color effectColor
        {
            get { return m_EffectColor; }
            set { m_EffectColor = value; }
        }

        /// <summary>
        /// Blur effect mode.
        /// </summary>
        public BlurMode blurMode
        {
            get { return m_BlurMode; }
            set { m_BlurMode = value; }
        }

        /// <summary>
        /// How far is the blurring from the graphic.
        /// </summary>
        public float blurFactor
        {
            get { return m_BlurFactor; }
            set { m_BlurFactor = Mathf.Clamp(value, 0, 4); }
        }

        /// <summary>
        /// Blur iterations.
        /// </summary>
        public int blurIterations
        {
            get { return m_BlurIterations; }
            set { m_BlurIterations = Mathf.Clamp(value, 1, 8); }
        }

        /// <summary>
        /// Down sampling rate of the generated RenderTexture.
        /// </summary>
        public DownSamplingRate downSamplingRate
        {
            get { return m_DownSamplingRate; }
            set { m_DownSamplingRate = value; }
        }

        /// <summary>
        /// Down sampling rate of reduction buffer to apply effect.
        /// </summary>
        public DownSamplingRate reductionRate
        {
            get { return m_ReductionRate; }
            set { m_ReductionRate = value; }
        }

        /// <summary>
        /// FilterMode for capturing.
        /// </summary>
        public FilterMode filterMode
        {
            get { return m_FilterMode; }
            set { m_FilterMode = value; }
        }

        /// <summary>
        /// Custom materials for post effect.
        /// </summary>
        public Material[] customMaterials
        {
            get { return m_CustomMaterials; }
            set { m_CustomMaterials = value; }
        }

        /// <summary>
        /// Captured texture.
        /// </summary>
        public RenderTexture renderTexture { get; internal set; }

        internal bool globalMode { get; set; }
        internal CommandBuffer commandBuffer { get; set; }
        internal Action<UIEffectSnapshotRequest> postAction { get; set; }

        public int materialHash
        {
            get { return ((int) m_EffectMode << 8) + ((int) m_ColorMode << 4) + ((int) m_BlurMode << 0); }
        }
    }
}
