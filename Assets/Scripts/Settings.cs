using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTG.DebugUtils;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    /// <summary>
    /// Settings manager class
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Dictionary containing value type for each setting
        /// ! remember to add every SettingsType here !
        /// </summary>
        /// <value></value>
        private static Dictionary<SettingsType, SettingsValueType> settingTypes = new Dictionary<SettingsType, SettingsValueType>
        {
            { SettingsType.RENDER_DISTANCE, SettingsValueType.INT },
            { SettingsType.MAX_CHUNKS_TO_BUILD_AT_ONCE, SettingsValueType.INT }
        };

        /// <summary>
        /// Save setting value
        /// </summary>
        /// <param name="settingsType">type of setting</param>
        /// <param name="value">value you want to save</param>
        public static void SetSetting(SettingsType settingsType, dynamic value)
        {
            if (settingTypes.TryGetValue(settingsType, out SettingsValueType valueType))
            {
                switch (valueType)
                {
                    case SettingsValueType.FLOAT:
                        if (value is float || value is int)
                        {
                            PlayerPrefs.SetFloat(settingsType.ToString(), (float)value);
                        }
                        // TODO: log error
                        break;
                    case SettingsValueType.INT:
                        if (value is int || value is float)
                        {
                            PlayerPrefs.SetInt(settingsType.ToString(), (int)value);
                        }
                        // TODO: log error
                        break;
                    case SettingsValueType.STRING:
                        if (value is string)
                        {
                            PlayerPrefs.SetString(settingsType.ToString(), (string)value);
                        }
                        // TODO: log error
                        break;
                }
            }
        }

        /// <summary>
        /// Get setting value
        /// </summary>
        /// <param name="settingsType">type of setting</param>
        /// <param name="resultValueType">setting value type</param>
        /// <returns>dynamic variable containing setting value</returns>
        public static dynamic GetSetting(SettingsType settingsType, out Type resultValueType)
        {
            if (settingTypes.TryGetValue(settingsType, out SettingsValueType valueType))
            {
                switch (valueType)
                {
                    case SettingsValueType.FLOAT:
                        resultValueType = typeof(float);
                        return PlayerPrefs.GetFloat(settingsType.ToString());
                    case SettingsValueType.INT:
                        resultValueType = typeof(int);
                        return PlayerPrefs.GetInt(settingsType.ToString());
                    case SettingsValueType.STRING:
                        resultValueType = typeof(string);
                        return PlayerPrefs.GetString(settingsType.ToString());
                }
            }

            resultValueType = null;
            return null;
        }

        /// <summary>
        /// Get setting value
        /// </summary>
        /// <param name="settingsType">type of setting</param>
        /// <returns>dynamic variable containing setting value</returns>
        public static dynamic GetSetting(SettingsType settingsType)
        {
            if (settingTypes.TryGetValue(settingsType, out SettingsValueType valueType))
            {
                switch (valueType)
                {
                    case SettingsValueType.FLOAT:
                        return PlayerPrefs.GetFloat(settingsType.ToString());
                    case SettingsValueType.INT:
                        return PlayerPrefs.GetInt(settingsType.ToString());
                    case SettingsValueType.STRING:
                        return PlayerPrefs.GetString(settingsType.ToString());
                }
            }

            return null;
        }

        public static SettingsValueType GetSettingsValueType(SettingsType settingsType)
        {
            if (settingTypes.TryGetValue(settingsType, out SettingsValueType valueType))
                return valueType;

            Debug.LogError($"There is no SettingsValueType defined for SettingsType {settingsType}!\n returning string type");
            return SettingsValueType.STRING;
        }
    }

    public enum SettingsValueType : byte
    {
        FLOAT,
        INT,
        STRING
    }

    public enum SettingsType : byte
    {
        RENDER_DISTANCE,
        MAX_CHUNKS_TO_BUILD_AT_ONCE
    }
}