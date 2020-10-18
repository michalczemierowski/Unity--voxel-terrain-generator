﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
#if UNITY_EDITOR
        public static bool gameCorrectlyLoaded;
#endif
        public bool limitFPS;
        public int targetFPS;

        public int renderDistance = 8;
        public int maxChunksToBuildAtOnce = 1000;

        void Awake()
        {
            // Singleton
            if (Instance)
            {
                Destroy(this);
                return;
            }
            else
                Instance = this;

            DontDestroyOnLoad(this);

            if (limitFPS)
                Application.targetFrameRate = targetFPS;

            SaveDefaultSettings();

#if UNITY_EDITOR
            gameCorrectlyLoaded = true;
#endif
        }

        private void SaveDefaultSettings()
        {
            Settings.SetSetting(SettingsType.RENDER_DISTANCE, renderDistance);
            Settings.SetSetting(SettingsType.MAX_CHUNKS_TO_BUILD_AT_ONCE, maxChunksToBuildAtOnce);
        }
    }
}