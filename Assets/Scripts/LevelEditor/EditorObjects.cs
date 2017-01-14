using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    public class EditorObjects : MonoBehaviour
    {
        public static EditorObjects singletonInstance;

        public TextAsset objectInfoFile;
        public List<GameObjectGroup> objectGroups;

        private GameObject mySun;
        private GameObject mySelectionPlane;
        private BlockInfo bInfo;

        public float gridScale = 4f;
        public float verticalGridScale = 0.2f;

        private Dictionary<Vector3,GameObject> gridObjects;
        private List<GameObject> nonGridObjects;

        private GameObject startSpawn;

        private void Awake()
        {
            //Make this the public instance of this class
            if(singletonInstance == null)
            {
                singletonInstance = this;
            }

            //Find child objects
            mySun = transform.Find("Sun").gameObject;
            mySelectionPlane = transform.Find("SelectionPlane").gameObject;

            gridObjects = new Dictionary<Vector3,GameObject>();
            nonGridObjects = new List<GameObject>();
            bInfo = new BlockInfo(objectInfoFile);
        }

        public List<GameObject> GetObjectGroupByName(string name)
        {
            foreach(GameObjectGroup group in objectGroups)
            {
                if(group.name.Equals(name))
                {
                    return group.objects;
                }
            }

            throw new System.Exception("Object group with name " + name + " not found!");
        }

        public GameObject GetPrefabByName(string name)
        {
            foreach(GameObjectGroup group in objectGroups)
            {
                foreach(GameObject go in group.objects)
                {
                    if(go.name.Equals(name))
                    {
                        return go;
                    }
                }
            }

            throw new System.Exception("Prefab with name " + name + " not found!");
        }

        public Vector3[] GetObjectExtentsByName(string blockName)
        {
            return bInfo.GetBlockExtents(blockName);
        }

        public Vector3 GetObjectRotationByName(string blockName)
        {
            return GetPrefabByName(blockName).transform.rotation.eulerAngles;
        }

        public Vector3 GetObjectOffsetByName(string blockName)
        {
            return GetPrefabByName(blockName).transform.position;
        }

        public Vector3 GetObjectScaleByName(string blockName)
        {
            return GetPrefabByName(blockName).transform.localScale;
        }

        private Vector3 StringToVector(string input)
        {
            string[] inputValues = input.Split(',');
            float[] parsedValues = new float[3];
            for(int i = 0; i < 3; i++)
            {
                float val;
                if (float.TryParse(inputValues[i], out val))
                {
                    parsedValues[i] = val;
                }
                else
                    throw new System.FormatException("Couldn't parse input.");
            }

            return new Vector3(parsedValues[0], parsedValues[1], parsedValues[2]);
        }

        public GameObject GetStartSpawn()
        {
            return startSpawn;
        }

        private void SpecialObjectCheck(GameObject obj)
        {
            switch(obj.name)
            {
                case "Start":
                    startSpawn = obj;
                    break;
            }
        }

        private void RemoveSpecialObjectCheck(GameObject obj)
        {
            switch(obj.name)
            {
                case "Start":
                    startSpawn = null;
                    break;
            }
        }

        public void AddNonGridObject(GameObject obj)
        {
            nonGridObjects.Add(obj);
            SpecialObjectCheck(obj);
        }

        public void RemoveNonGridObject(GameObject obj)
        {
            nonGridObjects.Remove(obj);
            RemoveSpecialObjectCheck(obj);
        }

        public void AddObjectToGrid(GameObject obj)
        {
            Vector3 pos = RoundVectorToGrid(obj.transform.position, gridScale, verticalGridScale);

            foreach(Vector3 addPos in GetObjectExtentsByName(obj.name))
            {
                gridObjects.Add(pos + RoundVectorToGrid(addPos, gridScale, verticalGridScale), obj);
            }

            SpecialObjectCheck(obj);
        }

        public void RemoveObjectFromGrid(GameObject obj)
        {
            List<Vector3> removeList = new List<Vector3>();

            foreach(KeyValuePair<Vector3,GameObject> pair in gridObjects)
            {
                if(pair.Value == obj)
                {
                    removeList.Add(pair.Key);
                }
            }

            while(removeList.Count > 0)
            {
                gridObjects.Remove(removeList[0]);
                removeList.RemoveAt(0);
            }

            RemoveSpecialObjectCheck(obj);
        }

        public bool IsGridPositionFree(Vector3 position)
        {
            return !gridObjects.ContainsKey(RoundVectorToGrid(position, gridScale, verticalGridScale));
        }

        public GameObject GetObjectAtGridPosition(Vector3 position)
        {
            return gridObjects[RoundVectorToGrid(position, gridScale, verticalGridScale)];
        }

        public static Vector3 RoundVectorToGrid(Vector3 input, float grid, float vGrid)
        {
            return new Vector3(RoundFloat(input.x, grid), RoundFloat(input.y, vGrid), RoundFloat(input.z, grid));
        }

        private static float RoundFloat(float input, float grid)
        {
            float a = input / grid;
            a += 0.5f;
            a = Mathf.Floor(a);
            a *= grid;
            return a;
        }

        //Saves the current level to a xml file
        public void SaveLevelToFile(string path)
        {
            List<ObjectData> allObj = new List<ObjectData>();
            foreach(GameObject obj in GetAllObjects())
            {
                ObjectData oData = new ObjectData();
                oData.name = obj.name;
                oData.position = obj.transform.localPosition;
                oData.eulerRotation = obj.transform.localRotation.eulerAngles;
                oData.scale = obj.transform.localScale;
                allObj.Add(oData);
            }
            LevelData data = new LevelData();
            data.levelObjects = allObj;
            data.WriteToFile(path);
        }

        public LevelData LoadLevelFromFile(string path)
        {
            return LevelData.CreateFromFile(path);
        }

        //Rotate the sun (the directional light)
        public void SetSunRotation(Quaternion rot)
        {
            mySun.transform.rotation = rot;
        }

        //Move selection plane (currently just up/down)
        public void AddSelectionPlanePosition(Vector3 addVector)
        {
            mySelectionPlane.transform.position += addVector;
        }

        public List<GameObject> GetAllObjects()
        {
            List<GameObject> objs = nonGridObjects;
            foreach(GameObject obj in gridObjects.Values)
            {
                //Dont include duplicates created by extents
                if(!objs.Contains(obj))
                    objs.Add(obj);
            }
            return objs;
        }

        public void DestroyAllObjects()
        {
            foreach(GameObject obj in gridObjects.Values)
            {
                Destroy(obj);
            }
            gridObjects.Clear();

            foreach(GameObject obj in nonGridObjects)
            {
                Destroy(obj);
            }
            nonGridObjects.Clear();
        }

        public void SetSelectionPlaneVisibility(bool value)
        {
            mySelectionPlane.SetActive(value);
        }
    }
}