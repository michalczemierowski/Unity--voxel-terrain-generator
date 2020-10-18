using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.DebugUtils
{
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance;

        [SerializeField] private TMP_Text debugText;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text fpsText;
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
            if(addTimestamp)
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
            while(content.Count > 0)
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
