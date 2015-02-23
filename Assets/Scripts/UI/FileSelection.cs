using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class FileSelection : MonoBehaviour
{
    public enum Mode
    {
        openFile,
        saveFile,
        openDir
    }

    public delegate void ClickAction();
    public delegate void SelectAction(string value);

    public static event ClickAction OnCancel;
    public static event SelectAction OnFileSelected;

    public GameObject filePrefab;
    public string winStartDir = "C:/";
    public string macStartDir = "";
    public string linuxStartDir = "/";

    private List<FileInfo> fileDisplays;
    private List<string> fileNames;
    private List<string> dirNames;
    private InputField pathField;
    private GameObject filePanel;
    private GameObject itemList;

    private RectTransform rt;
    private float startHeight;

    private string selectedPath
    {
        get { return pathField.text; }
        set { pathField.text = value; }
    }

    void Awake()
    {
        fileDisplays = new List<FileInfo>();
        fileNames = new List<string>();
        dirNames = new List<string>();
        pathField = transform.Find("FilePath").gameObject.GetComponent<InputField>();
        filePanel = transform.Find("FileList").gameObject;
        itemList = filePanel.transform.Find("Items").gameObject;
        rt = (RectTransform)itemList.transform;
        startHeight = rt.rect.height;

        switch(Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                pathField.text = winStartDir; break;
            case RuntimePlatform.WindowsEditor:
                pathField.text = winStartDir; break;
            case RuntimePlatform.OSXPlayer:
                pathField.text = macStartDir; break;
            case RuntimePlatform.OSXEditor:
                pathField.text = macStartDir; break;
            case RuntimePlatform.LinuxPlayer:
                pathField.text = linuxStartDir; break;
        }

        UpdatePathFromInput();
    }

    public void UpdatePathFromInput()
    {
        UpdatePath(pathField.text);
    }

    public void UpdatePath(string newPath)
    {
        if(newPath.Equals("..") && Directory.Exists(selectedPath))
        {
            DirectoryInfo info = Directory.GetParent(selectedPath);
            if(info != null)
            {
                selectedPath = info.FullName;
                UpdateDisplayedDirectory(selectedPath);
            }
        }
        else if(File.Exists(newPath))
        {
            selectedPath = newPath;
            UpdateDisplayedDirectory(Path.GetDirectoryName(selectedPath));
        }
        else
        {
            if(Directory.Exists(newPath))
            {
                selectedPath = newPath;
                UpdateDisplayedDirectory(selectedPath);
            }
        }
    }

    private void ClearFileDisplays()
    {
        foreach(FileInfo display in fileDisplays)
        {
            GameObject.Destroy(display.gameObject);
        }
        fileDisplays.Clear();
    }

    private void UpdateDisplayedDirectory(string directory)
    {
        ClearFileDisplays();
        fileNames.Clear();
        dirNames.Clear();

        float itemHeight = 0f;

        try
        {
            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                fileNames.Add(file);
            }
            foreach (string dir in dirs)
            {
                dirNames.Add(dir);
            }
        }
        catch(System.Exception ex)
        {
            print("Errow while fechting data from directory! " + ex.Message);
        }

        //Add 10 displays
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
                display.text = "..";
            }
            else if (fileCounter <= dirNames.Count)
            {
                display.text = dirNames[fileCounter - 1];
            }
            else
            {
                display.text = fileNames[fileCounter - dirNames.Count - 1];
            }

            itemHeight = display.GetOriginalHeight();
        }

        //Scale background
        rt.offsetMax = new Vector2(rt.offsetMax.x, 0f);
        rt.offsetMin = new Vector2(rt.offsetMin.x, -1f * itemHeight * (dirNames.Count + fileNames.Count + 1) + startHeight);
    }

    public void OkPress()
    {
        if(File.Exists(selectedPath))
        {
            if(OnFileSelected != null)
                OnFileSelected(selectedPath);
            gameObject.SetActive(false);
        }
        else
        {
            selectedPath = "Invalid file!";
        }
    }

    public void CancelPress()
    {
        if(OnCancel != null)
            OnCancel();
        gameObject.SetActive(false);
    }
}
