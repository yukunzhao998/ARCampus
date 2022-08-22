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

    //private Vector3 origin_position = new Vector3(0.0f, 0.0f, 0.0f);
    //private Quaternion origin_rotation = Quaternion.Euler(0,0,0);

    void Start()
    {
        button.onClick.AddListener(TaskOnClick);
        tiger.SetActive(false);

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

    void setObjectCoord(float xwx, float xwy, float xwz, float qx, float qy, float qz, float qw)
    {
        _camera.transform.position = new Vector3(xwx, xwy, xwz);
        float[,] xwOrigin = new float[,] {{xwx}, {xwy}, {xwz}};
        Quaternion camQuat = new Quaternion(qx, qy, qz, qw);
        camQuat = normalize(camQuat);
        float[,] camRotationMatrix = CoordTransform.getRotationMatrix(camQuat);
        float[,] translationVector = CoordTransform.MultiplyMatrix(CoordTransform.NegativeMatrix(camRotationMatrix), xwOrigin);
        //Debug.Log(translationVector[0,0].ToString()+"\n"+translationVector[1,0].ToString()+"\n"+translationVector[2,0].ToString()+"\n");
        //Debug.Log(camQuat.x.ToString()+"\n"+camQuat.y.ToString()+"\n"+camQuat.z.ToString()+"\n"+camQuat.w.ToString());

        //1. set right (red)
        float[,] xcRight = new float[,] {{1},{0},{0}};
        float[,] xwRight = CoordTransform.MultiplyMatrix(CoordTransform.Transpose(camRotationMatrix), CoordTransform.MinusMatrix(xcRight, translationVector));
        float[,] vectorRight = CoordTransform.MinusMatrix(xwRight, xwOrigin);
        Vector3 rightDirection = new Vector3(vectorRight[0,0], vectorRight[1,0], vectorRight[2,0]);
        //2. set forward (blue)
        float[,] xcForward = new float[,] {{0},{0},{1}};
        float[,] xwForward = CoordTransform.MultiplyMatrix(CoordTransform.Transpose(camRotationMatrix), CoordTransform.MinusMatrix(xcForward, translationVector));
        float[,] vectorForward = CoordTransform.MinusMatrix(xwForward, xwOrigin);
        Vector3 forwardDirection = new Vector3(vectorForward[0,0], vectorForward[1,0], vectorForward[2,0]);

        Vector3 upDirection = -1*(Vector3.Cross(forwardDirection, rightDirection));
        Quaternion orientation = Quaternion.LookRotation(forwardDirection, upDirection);
        _camera.transform.rotation = orientation;
    }
    
    void setObjectCoord(GameObject gameobject,float tx, float ty, float tz, float qx, float qy, float qz, float qw)
    {
        tx = 5*tx;
        ty = 5*ty;
        tz = 5*tz;
        float[,] translationVector = new float[,] {{tx}, {ty}, {tz}};
        Quaternion camQuat = new Quaternion(qx, qy, qz, qw);
        float[,] camRotationMatrix = CoordTransform.getRotationMatrix(camQuat);
        float[,] xcOrigin = new float[,] {{0}, {0}, {0}};
        float[,] xwOrigin = CoordTransform.MultiplyMatrix(CoordTransform.Transpose(camRotationMatrix), CoordTransform.MinusMatrix(xcOrigin, translationVector));
        gameobject.transform.position = new Vector3(xwOrigin[0,0], xwOrigin[1,0], xwOrigin[2,0]);
        //1. set right (red)
        float[,] xcRight = new float[,] {{1},{0},{0}};
        float[,] xwRight = CoordTransform.MultiplyMatrix(CoordTransform.Transpose(camRotationMatrix), CoordTransform.MinusMatrix(xcRight, translationVector));
        float[,] vectorRight = CoordTransform.MinusMatrix(xwRight, xwOrigin);
        Vector3 rightDirection = new Vector3(vectorRight[0,0], vectorRight[1,0], vectorRight[2,0]);
        //2. set forward (blue)
        float[,] xcForward = new float[,] {{0},{0},{1}};
        float[,] xwForward = CoordTransform.MultiplyMatrix(CoordTransform.Transpose(camRotationMatrix), CoordTransform.MinusMatrix(xcForward, translationVector));
        float[,] vectorForward = CoordTransform.MinusMatrix(xwForward, xwOrigin);
        Vector3 forwardDirection = new Vector3(vectorForward[0,0], vectorForward[1,0], vectorForward[2,0]);

        Vector3 upDirection = -1*(Vector3.Cross(forwardDirection, rightDirection));
        Quaternion orientation = Quaternion.LookRotation(forwardDirection, upDirection);
        gameobject.transform.rotation = orientation;
    }
}
