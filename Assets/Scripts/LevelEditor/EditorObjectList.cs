using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    [System.Serializable]
    public class EditorObjectList : MonoBehaviour
    {
        private const int PAGE_SIZE = 6;

        private List<GameObject> currentCollectionObjects;
        private string currentCollectionName = "";
        private EditorObjectDisplay[] displays;
        private int pageNumber = 0;

        private void Awake()
        {
            displays = new EditorObjectDisplay[6];

            for(int i = 0; i < displays.Length; i++)
            {
                displays[i] = transform.Find("Obj" + (i + 1).ToString()).GetComponent<EditorObjectDisplay>();
            }
        }

        public void ToggleToCollection(string collectionName)
        {
            pageNumber = 0;

            if(currentCollectionName == collectionName)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            currentCollectionName = collectionName;
            currentCollectionObjects = EditorObjects.singletonInstance.GetObjectGroupByName(collectionName);
            UpdateDisplays();
        }

        private void UpdateDisplays()
        {
            for(int i = 0; i < displays.Length; i++)
            {
                int itemPosition = i + pageNumber * PAGE_SIZE;
                if (itemPosition < currentCollectionObjects.Count)
                {
                    // TODO get texture
                    //displays[i].SetObject(currentCollectionObjects[itemPosition], texture);
                }
                else
                {
                    displays[i].SetText("");
                    displays[i].SetImage(null);
                }	
            }
        }

        public void AddPageNumber(int add)
        {
            if(currentCollectionObjects != null)
            {
                pageNumber += add;

                if(pageNumber < 0)
                    pageNumber = 0;

                if(pageNumber * 6 >= currentCollectionObjects.Count)
                {
                    pageNumber -= add;
                }
                else
                {
                    UpdateDisplays();
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if(visible)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                currentCollectionName = "";
            }
        }
    }
}
