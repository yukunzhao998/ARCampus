using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Unity.Barracuda;

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
    public GameObject cube;

    [SerializeField]
    public ARSessionOrigin arSessionOrigin;

    [SerializeField]
    private NNModel poseNetModel;
    
    private Model runtimeModel;
    private IWorker worker;
    private string translateVecLayer;
    private string rotateVecLayer;

    //private Vector3 origin_position = new Vector3(0.0f, 0.0f, 0.0f);
    //private Quaternion origin_rotation = Quaternion.Euler(0,0,0);

    void Start()
    {
        button.onClick.AddListener(TaskOnClick);
        cube.SetActive(false);
        runtimeModel = ModelLoader.Load(poseNetModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        //outputLayer = runtimeModel.outputs[runtimeModel.outputs.Count - 1];
        translateVecLayer = runtimeModel.outputs[0];
        rotateVecLayer = runtimeModel.outputs[1];

        log.text = "Intermediate variables: "+ "\n";
    }

    void Update()
    {


    }

    void TaskOnClick()
    {
        cube.SetActive(true);

        //input frame of the camera, and prepocessing of image
        //frameInput = 
        Texture2D tex = (Texture2D)Resources.Load("testImage");
        tex = Preprocess.ScaleTexture(tex, 224, 224);
        
        //convert type 'UnityEngine.Texture2D' to 'float[]'
        var encoder = new TextureAsTensorData(tex);

        using Tensor inputTensor = new Tensor(1, 224, 224, 3, encoder);
        //inputTensor[0] = tex;
        worker.Execute(inputTensor);
        Tensor translateTensor = worker.PeekOutput(translateVecLayer);
        Tensor rotateTensor = worker.PeekOutput(rotateVecLayer);

        log.text += "Translation Vector: "+ "\n";
        log.text += "("+ translateTensor[0].ToString() +", "+ translateTensor[1].ToString()+ ", " + translateTensor[2].ToString()+ ")"+ "\n";
        log.text += "Rotation Vector: "+ "\n";
        log.text += "("+ rotateTensor[0].ToString()+ ", "+ rotateTensor[1].ToString()+ ", "+ rotateTensor[2].ToString()+ ", "+ rotateTensor[3].ToString()+ ")"+ "\n";

    }

    public void OnDestroy()
    {
        worker?.Dispose();
    }
}
