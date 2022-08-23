using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Unity.Barracuda;
using UnityEditor;

public class Display : MonoBehaviour
{
    const int IMAGE_SIZE = 224;

    [SerializeField]
    public Text log;

    [SerializeField]
    public Button button;

    [SerializeField]
    public Camera _camera;

    [SerializeField]
    public GameObject tiger;

    [SerializeField]
    public ARSessionOrigin arSessionOrigin;

    [SerializeField]
    private NNModel poseNetModel;
    
    private Model runtimeModel;
    private IWorker worker;
    private string translateVecLayer;
    private string rotateVecLayer;

    void Start()
    {
        button.onClick.AddListener(TaskOnClick);
        tiger.SetActive(false);

        runtimeModel = ModelLoader.Load(poseNetModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        translateVecLayer = runtimeModel.outputs[0];
        rotateVecLayer = runtimeModel.outputs[1];

        log.text = "Intermediate variables: "+ "\n";
    }

    void Update()
    {

    }

    void TaskOnClick()
    {
        tiger.SetActive(true);
        //set the display object in world space
        setObjectCoord(tiger, -7.9372005743993155f, -0.018808338696225977f, -0.755158005882741f, 0.9957989566896625f, -0.0873020608514319f, 0.025496715402473905f, -0.010616286100041216f);
       
        //input frame of the camera, and prepocessing of image
        //frameInput = 

        Texture2D tex = (Texture2D)Resources.Load("pytorch_resized");
        EditorUtility.CompressTexture(tex, TextureFormat.RGBA32, TextureCompressionQuality.Best);
        //tex.Compress(true);
        using Tensor inputTensor = new Tensor(tex, 3);
        worker.Execute(inputTensor);
        Tensor translateTensor = worker.PeekOutput(translateVecLayer);
        Tensor rotateTensor = worker.PeekOutput(rotateVecLayer);

        //set the camera in world spacce
        setObjectCoord(translateTensor[0], translateTensor[1], translateTensor[2],rotateTensor[0], rotateTensor[1], rotateTensor[2], rotateTensor[3]);

        //UI output
        log.text += "Translation Vector: "+ "\n";
        log.text += "("+ translateTensor[0].ToString() +", "+ translateTensor[1].ToString()+ ", " + translateTensor[2].ToString()+ ")"+ "\n";
        log.text += "Rotation Vector: "+ "\n";
        log.text += "("+ rotateTensor[0].ToString()+ ", "+ rotateTensor[1].ToString()+ ", "+ rotateTensor[2].ToString()+ ", "+ rotateTensor[3].ToString()+ ")"+ "\n";

    }

    void OnDestroy()
    {
        worker?.Dispose();
    }

    Quaternion normalize(Quaternion quat){
        float twoNorm = (float) Math.Sqrt(quat.w*quat.w+quat.x*quat.x+quat.y*quat.y+quat.z*quat.z);
        Quaternion result = new Quaternion(quat.x/twoNorm, quat.y/twoNorm, quat.z/twoNorm, quat.w/twoNorm);
        return result;
    }

    void setObjectCoord(float xwx, float xwy, float xwz, float qw, float qx, float qy, float qz)
    {
        Vector3 objPosition = new Vector3(xwx, -xwy, xwz);
        _camera.transform.position = objPosition;
        Quaternion orientation = new Quaternion(qx, -qy, qz, qw);
        orientation = normalize(orientation);
        _camera.transform.rotation = orientation;
    }

    void setObjectCoord(GameObject gameObject, float xwx, float xwy, float xwz, float qw, float qx, float qy, float qz)
    {
        Vector3 objPosition = new Vector3(xwx, -xwy, xwz);
        gameObject.transform.position = objPosition;
        Quaternion orientation = new Quaternion(qx, -qy, qz, qw);
        gameObject.transform.rotation = orientation;
    }

}
