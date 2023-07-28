using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class LevelEditor : MonoBehaviour
{

    [SerializeField]
    private GameObject Prefab;
    [SerializeField]
    private Transform leftAnchor;
    [SerializeField]
    private Transform rightAnchor;


    private OVRGrabber Lgrab;
    private OVRGrabber Rgrab;

    private OVRInput.Controller controllerType;
    private bool wasTriggerPressed = false;
    private GameObject brush;

    private void Start()
    {
        Lgrab= leftAnchor.transform.parent.GetComponent<OVRGrabber>();
        Rgrab = rightAnchor.transform.parent.GetComponent<OVRGrabber>();
    }

    // Update is called once per frame
    void Update()
    {


        //controls for deleting held object
        OVRGrabbable grabbed = Lgrab.grabbedObject;
        //if trigger while holding 
        if (grabbed != null && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.0f)
        {
            Lgrab.ForceRelease(grabbed);
            Destroy(grabbed.gameObject);
            wasTriggerPressed = true;
            return;

        }
        grabbed = Rgrab.grabbedObject;
        //if trigger while holding 
        if (grabbed != null && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.0f)
        {
            Rgrab.ForceRelease(grabbed);
            Destroy(grabbed.gameObject);
            wasTriggerPressed = true;
            return;

        }

        controllerType = OVRInput.Controller.RTouch;

        ////TEST Control
        //// Get the index trigger input for the specified controller
        //float indexTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerType);
        //// Check if the index trigger is pressed for the specified controller
        //if (indexTriggerValue > 0.0f)
        //{
        //    Debug.Log("Index trigger pressed on " + controllerType.ToString() + " hand.");
        //    SpawnObject(GetWorldspaceControllerTransform(controllerType));
        //}



        //spawn on pull and reset on release

        // Get the index trigger input for the specified controller
        float indexTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerType);

        // Check if the index trigger is pressed for the specified controller
        if (indexTriggerValue > 0.0f && !wasTriggerPressed)
        {
            Debug.Log("Index trigger pressed on " + controllerType.ToString() + " hand.");
            brush = SpawnObject(GetWorldspaceControllerTransform(controllerType));
            brush.name = Prefab.name;
            wasTriggerPressed = true;
        }
        else if (indexTriggerValue <= 0.0f)
        {
            wasTriggerPressed = false;
            brush = null;
        }
        else
        {
            if (brush == null)
                return;
            Transform controllerTransform = GetWorldspaceControllerTransform(controllerType);
            brush.transform.position = controllerTransform.position;
            brush.transform.rotation = controllerTransform.rotation;
        }
    }

    public GameObject SpawnObject(Transform transform)
    {
        return GameObject.Instantiate(Prefab, transform.position, transform.rotation,this.transform);
    }

    private Transform GetWorldspaceControllerTransform(OVRInput.Controller controllerType)
    {

        if (controllerType == OVRInput.Controller.LTouch)
        {
            return leftAnchor;
        }
        else if (controllerType == OVRInput.Controller.RTouch)
        {
            return rightAnchor;
        }

        return null;
    }
    private void SaveLevelData(string fileName)
    {

        Transform[] childTransforms = transform.GetComponentsInChildren<Transform>(true); // Include inactive GameObjects as well
        Scene scene = new Scene();


        foreach (Transform childTransform in childTransforms)
        {
            if (childTransform != transform) // Skip the parent object (current object)
            {
                scene.levelData.Add(JsonUtility.ToJson(UpdateLevelDataFromGameObject(childTransform.gameObject)));
            }
        }
        string jsonData = JsonUtility.ToJson(scene);
        Debug.Log(jsonData);
        File.WriteAllText(fileName, jsonData);
    }

    private Scene LoadLevelData(string fileName)
    {
        if (File.Exists(fileName))
        {
            string jsonData = File.ReadAllText(fileName);
            return JsonUtility.FromJson<Scene>(jsonData);
        }
        else
        {
            Debug.LogWarning("File not found: " + fileName);
            return null;
        }
    }
    public void NewLevel()
    {
        // Destroy all children
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
    public void SaveLevel(string fileName)
    {
        SaveLevelData(fileName);
    }
    public void LoadLevel(string fileName)
    {
        // Call NewLevel to clear the current level before loading the new one
        NewLevel();

        // Load the level data from the JSON file
        Scene scene = LoadLevelData(fileName);

        // Spawn the objects based on the loaded level data
        if (scene.levelData != null)
        {
            SpawnLevelObjects(scene);
        }
    }
private void SpawnLevelObjects(Scene scene)
{
    foreach (string jsonData in scene.levelData)
    {
        LevelData data = JsonUtility.FromJson<LevelData>(jsonData);

        GameObject prefab = Resources.Load<GameObject>("Objects/" + data.resource);
        if (prefab != null)
        {
            GameObject spawnedObject = Instantiate(prefab);

            // Set the position, rotation, and scale of the spawned object's transform
            spawnedObject.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
            spawnedObject.transform.rotation = new Quaternion(data.rotation[0], data.rotation[1], data.rotation[2], data.rotation[3]);
            spawnedObject.transform.localScale = new Vector3(data.scale[0], data.scale[1], data.scale[2]);

            //make object a child
            spawnedObject.transform.parent = this.transform;

            //name object for saving 
            spawnedObject.name = data.resource;
            
        }
        else
        {
            Debug.LogWarning("Prefab not found in Resources folder: " + data.resource);
        }
    }
}

    public LevelData UpdateLevelDataFromGameObject(GameObject gameObject)
    {
        LevelData data = new LevelData();
        Transform transform = gameObject.transform;

        data.resource = gameObject.name; // Set the resource field as the object's name must match the prefab name

        // Get the position, rotation, and scale from the object's transform
        data.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        data.rotation = new float[] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w };
        data.scale = new float[] { transform.localScale.x, transform.localScale.y, transform.localScale.z };
        return data;
    }
}
