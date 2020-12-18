using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTG
{
    public class DaylightCycleManager : MonoBehaviour
    {
        [SerializeField] private Light directionalLight;
        [SerializeField] private Light directionalMoonLight;
        [SerializeField] private Transform sunRotationPivot;
        [SerializeField] private Material skyboxDay;
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
            timeOffset = ticksInDay / 2;
        }

        private void OnDisable()
        {
            World.OnTick -= OnTick;
        }

        public void OnTick(int currentTick)
        {
            // range <0; 1>
            float time = (float)(currentTick % (ticksInDay + timeOffset)) / (ticksInDay + timeOffset);

            directionalLight.intensity = sunIntensityCurve.Evaluate(time);
            directionalLight.color = timeColors.Evaluate(time);

            float eulerX = Utils.RoundToDecimalPlace(sunRotationXCurve.Evaluate(time) * 360, 1);
            bool isDay = eulerX > 0 && eulerX < 180;
            sunRotationPivot.eulerAngles = new Vector3(eulerX, 0, 0);
            directionalLight.enabled = isDay;
            directionalMoonLight.enabled = !isDay;
            
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

            RenderSettings.fogDensity = fogDensityCurve.Evaluate(time);
            RenderSettings.fogColor = fogColors.Evaluate(time);

            RenderSettings.ambientSkyColor = timeColors.Evaluate(time);
        }
    }
}
