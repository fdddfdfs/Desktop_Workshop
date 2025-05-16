using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ResourcesLoader
{
    public static T InstantiateLoadComponent<T>(string resourceName) where T : MonoBehaviour
    {
        GameObject loadedObject = LoadObject(resourceName);
        loadedObject = Object.Instantiate(loadedObject);

        return GetComponent<T>(loadedObject);
    }

    public static T InstantiateLoadedComponent<T>(GameObject loadedObject) where T : MonoBehaviour
    {
        loadedObject = Object.Instantiate(loadedObject);

        return GetComponent<T>(loadedObject);
    }
    
    public static GameObject LoadObject(string resourceName)
    {
        var loadedObject = Resources.Load(resourceName) as GameObject;
        if (!loadedObject)
        {
            throw new Exception($"Resources folder does not contain {resourceName}");
        }

        return loadedObject;
    }
    
    public static T LoadMenuView<T>(Transform parent, string menuViewResourceName) where T: MonoBehaviour
    {
        var view = InstantiateLoadComponent<T>(menuViewResourceName);
        view.transform.SetParent(parent, false);

        return view;
    }
    
    private static T GetComponent<T>(GameObject loadedObject) where T: MonoBehaviour
    {
        var component = loadedObject.GetComponent<T>();

        if (!component)
        {
            throw new Exception(
                $"{loadedObject.name} resource should contain {typeof(T).FullName} component");
        }

        return component;
    }
}