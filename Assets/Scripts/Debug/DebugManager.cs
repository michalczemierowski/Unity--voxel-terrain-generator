using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.DebugUtils
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance;

        [Header("References")]
        [SerializeField] private DebugCommandHandler debugCommandHandler;
        public static DebugCommandHandler CommandHandler => Instance.debugCommandHandler;

        [SerializeField] private TMP_Text debugText;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private TMP_Text biomeInfoText;

        [Header("Settings")]
        [SerializeField] private int maxMessages = 10;
        [SerializeField] private float consoleMessageLifetime = 2;
        [SerializeField] private bool addTimestamp;

        private List<string> content = new List<string>();

        void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }

        void Start()
        {
            InvokeRepeating(nameof(UpdateFPSText), 0.1f, 0.1f);
        }

        public void AddDebugMessage(string msg)
        {
            if (addTimestamp)
            {
                DateTime date = DateTime.Now;
                msg = $"[ {date.ToLongTimeString()}:{date.Millisecond.ToString("000")} ] " + msg;
            }
            if (content.Count >= maxMessages)
                content.RemoveRange(0, maxMessages - content.Count + 1);

            content.Add(msg);

            StopCoroutine(nameof(RemoveConsoleMessagesCoroutine));
            StartCoroutine(nameof(RemoveConsoleMessagesCoroutine));

            UpdateConsoleText();
        }

        private IEnumerator RemoveConsoleMessagesCoroutine()
        {
            while (content.Count > 0)
            {
                yield return new WaitForSeconds(consoleMessageLifetime);

                content.RemoveAt(0);
                UpdateConsoleText();
            }
        }

        private void UpdateConsoleText()
        {
            debugText.text = string.Empty;
            foreach (string line in content)
            {
                debugText.text += line + "\n";
            }
        }

        private void UpdateFPSText()
        {
            int fps = Mathf.RoundToInt(1f / Time.deltaTime);
            fpsText.text = $"{fps} FPS";
        }

        public static void UpdateBiomeInfoText()
        {
            int x = Player.PlayerController.Movement.CurrentPosition.x;
            int z = Player.PlayerController.Movement.CurrentPosition.z;

            float height = World.FastNoise.GetSimplex(x / WorldSettings.Biomes.BiomeSize, z / WorldSettings.Biomes.BiomeSize) + 1;
            float temperature = World.FastNoise.GetSimplex((x / WorldSettings.Biomes.BiomeSize) * 0.1f, (z / WorldSettings.Biomes.BiomeSize) * 0.1f) + 1;
            float moistrue = World.FastNoise.GetSimplex((x / WorldSettings.Biomes.BiomeSize) * 0.25f, (z / WorldSettings.Biomes.BiomeSize) * 0.25f) + 1;

            Instance.biomeInfoText.text = $"height: {height.ToString("0.00")}\ntemperature: {temperature.ToString("0.00")}\nmoistrue: {moistrue.ToString("0.00")}\nbiome: {WorldSettings.Biomes.GetBiome(x, z)}";
        }

        public static void AddDebugMessageStatic(string msg)
        {
            Instance.AddDebugMessage(msg);
        }

        public static void SetPositionText(string text)
        {
            Instance.positionText.text = text;
        }
    }
}
