using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTG
{
    public class DaylightCycleManager : MonoBehaviour
    {
        [Tooltip("Daytime directional light (will be disabled during night)")]
        [SerializeField] private Light directionalLight;
        [Tooltip("Nighttime directional light (will be disabled during day)")]
        [SerializeField] private Light directionalMoonLight;
        [Tooltip("Sun and Moon parent which will be rotated during cycle")]
        [SerializeField] private Transform sunRotationPivot;
        [Tooltip("Skybox that will be active during day")]
        [SerializeField] private Material skyboxDay;
        [Tooltip("Skybox that will be active during night")]
        [SerializeField] private Material skyboxNight;

        [SerializeField] private AnimationCurve sunIntensityCurve;
        [SerializeField] private AnimationCurve sunRotationXCurve;
        [SerializeField] private int ticksInDay = 32000;
        [SerializeField] private Gradient timeColors;
        [SerializeField] private Gradient fogColors;
        [SerializeField] private AnimationCurve fogDensityCurve;

        /// <summary>
        /// offset to start game at day
        /// </summary>
        private float timeOffset;

        private void OnEnable()
        {
            World.OnTick += OnTick;
            timeOffset = ticksInDay / 4;
        }

        private void OnDisable()
        {
            World.OnTick -= OnTick;
        }

        public void OnTick(int currentTick)
        {
            // range <0; 1>
            float time = (currentTick + timeOffset) % (ticksInDay) / ticksInDay;

            // apply sun color and intesity
            directionalLight.intensity = sunIntensityCurve.Evaluate(time);
            directionalLight.color = timeColors.Evaluate(time);

            float eulerX = Utils.RoundToDecimalPlace(sunRotationXCurve.Evaluate(time) * 360, 1);
            bool isDay = eulerX > 0 && eulerX < 180;
            // rotate sun and moon
            sunRotationPivot.eulerAngles = new Vector3(eulerX, 0, 0);
            directionalLight.enabled = isDay;
            directionalMoonLight.enabled = !isDay;
            
            // TODO: lerp between procedural skybox
            if(isDay)
            {
                RenderSettings.sun = directionalLight;
                RenderSettings.skybox = skyboxDay;
            }
            else
            {
                RenderSettings.sun = directionalMoonLight;
                RenderSettings.skybox = skyboxNight;
            }

            // apply fog settings
            RenderSettings.fogDensity = fogDensityCurve.Evaluate(time);
            RenderSettings.fogColor = fogColors.Evaluate(time);

            // apply sky color
            RenderSettings.ambientSkyColor = timeColors.Evaluate(time);
        }
    }
}
