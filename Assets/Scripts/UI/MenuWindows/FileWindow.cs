using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class FileWindow : DefaultMenuWindow
    {
        public enum Mode
        {
            OPEN_FILE,
            SAVE_FILE,
            OPEN_DIR
        }

        [SerializeField]
        private GameObject filePrefab;

        public Action<string> OkAction { get; set; }

        private List<FileInfo> fileDisplays;
        private InputField pathField;
        private GameObject filePanel;
        private GameObject itemList;

        private RectTransform rt;
        private float startHeight;

        private string SelectedPath
        {
            get { return pathField.text; }
            set { pathField.text = value; }
        }

        private void Awake()
        {
            fileDisplays = new List<FileInfo>();
            pathField = transform.Find("FilePath").gameObject.GetComponent<InputField>();
            filePanel = transform.Find("FileList").gameObject;
            itemList = filePanel.transform.Find("Items").gameObject;
            rt = (RectTransform)itemList.transform;
            startHeight = rt.rect.height;
            DisplayDriveList();
        }

        public void UpdatePath(string newPath)
        {
            if (newPath.Equals("..") && Directory.Exists(SelectedPath))
            {
                // Special case for going one dir up
                DirectoryInfo info = Directory.GetParent(SelectedPath);
                if (info != null)
                {
                    SelectedPath = info.FullName;
                    UpdateDisplayedDirectory(SelectedPath);
                }
                else
                {
                    DisplayDriveList();
                }
            }
            else if (File.Exists(newPath))
            {
                SelectedPath = newPath;
                UpdateDisplayedDirectory(Path.GetDirectoryName(SelectedPath));
            }
            else
            {
                if (Directory.Exists(newPath))
                {
                    SelectedPath = newPath;
                    UpdateDisplayedDirectory(SelectedPath);
                }
            }
        }

        private void UpdatePathFromInput()
        {
            UpdatePath(pathField.text);
        }

        private void ClearFileDisplays()
        {
            foreach (FileInfo display in fileDisplays)
            {
                Destroy(display.gameObject);
            }
            fileDisplays.Clear();
        }

        private void DisplayDriveList()
        {
            UpdateDisplay(Directory.GetLogicalDrives().ToList(), new List<string>());
        }

        private void UpdateDisplayedDirectory(string directory)
        {
            var fileNames = new List<string>();
            var dirNames = new List<string>();

            try
            {
                string[] files = Directory.GetFiles(directory);
                string[] dirs = Directory.GetDirectories(directory);

                fileNames.AddRange(files);
                dirNames.AddRange(dirs);
            }
            catch (IOException)
            {
                // TODO show error dialog or something
            }

            UpdateDisplay(fileNames, dirNames);
        }

        private void UpdateDisplay(List<string> dirNames, List<string> fileNames)
        {
            float itemHeight = 0f;

            ClearFileDisplays();

            for (int fileCounter = 0; fileCounter <= dirNames.Count + fileNames.Count; fileCounter++)
            {
                GameObject parentObj = (GameObject)GameObject.Instantiate(filePrefab);
                parentObj.transform.SetParent(itemList.transform, false);
                FileInfo display = parentObj.GetComponent<FileInfo>();
                display.SetVerticalOffset(fileCounter);
                fileDisplays.Add(display);

                display.Initialize();

                if (fileCounter == 0)
                {
                    display.Text = "..";
                }
                else if (fileCounter <= dirNames.Count)
                {
                    display.Text = dirNames[fileCounter - 1];
                }
                else
                {
                    display.Text = fileNames[fileCounter - dirNames.Count - 1];
                }

                itemHeight = display.GetOriginalHeight();
            }

            //Scale background
            rt.offsetMax = new Vector2(rt.offsetMax.x, 0f);
            rt.offsetMin = new Vector2(rt.offsetMin.x, -1f * itemHeight * (dirNames.Count + fileNames.Count + 1) + startHeight);
        }

        private void OkPress()
        {
            if (File.Exists(SelectedPath))
            {
                if (OkAction != null)
                    OkAction(SelectedPath);
                GameMenu.SingletonInstance.CloseWindow();
            }
            else
            {
                SelectedPath = "Invalid file!";
            }
        }

        private void CancelPress()
        {
            GameMenu.SingletonInstance.CloseWindow();
        }
    }
}