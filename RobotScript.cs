using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class RobotScript : MonoBehaviour{
   WheelCollider wcA,wcB,wcC;
   GameObject robot;
   Vector3 locked;
   List<List<float>> searchPath;
   PathScript ps;
   int PathCount=0;
   void Start(){
       
       robot = new GameObject(name="robot");
       robot.AddComponent<Rigidbody>();
       robot.GetComponent<Rigidbody>().mass=2;
       GameObject robotBody=Instantiate(Resources.Load("RobotBodyv3",typeof(GameObject))) as GameObject;
       robotBody.transform.rotation*=Quaternion.Euler(-90,180,0);
       robotBody.transform.position=new Vector3(0,0.25f,0);
       robotBody.transform.localScale=new Vector3(18,18,18);
       AddChild(robot, robotBody);
       GameObject wheelGroup = new GameObject(name="wheelGroup");
       AddChild(robot,wheelGroup);
       //First wheel
       GameObject wheelA = new GameObject(name="wheelA");
       AddChild(wheelGroup,wheelA);
       wheelA.AddComponent<Rigidbody>();
       GameObject wheelJoint = new GameObject(name="wheelJoint");
       AddChild(wheelA,wheelJoint);
       wcA = wheelJoint.AddComponent<WheelCollider>();
       wcA.radius = 1;
       WheelFrictionCurve a = wcA.sidewaysFriction;
       a.stiffness=0.5f;
       wcA.sidewaysFriction=a; //assign the variable back to property
       WheelFrictionCurve f = wcA.forwardFriction;
       f.stiffness=3;
       wcA.forwardFriction=f; //assign the variable back to property
       //Instead of a Spherical tyre, we use the wheel we created from Fusion.
       GameObject omniWheel=Instantiate(Resources.Load("mtrAv3",typeof(GameObject))) as GameObject;
       omniWheel.transform.localRotation*=Quaternion.Euler(0,0,90);
      
       omniWheel.name="omniWheel";
       omniWheel.AddComponent<MeshRenderer>();
       omniWheel.GetComponent<Renderer>().material.color=Color.red;
       omniWheel.transform.localScale = new Vector3(20,20,20);
       AddChild(wheelJoint,omniWheel);
      
       //Cloning wheelB from wheelA
       GameObject wheelB = Object.Instantiate(wheelA);
       wheelB.name="wheelB";
       AddChild(wheelGroup,wheelB);
       wcB = wheelB.transform.GetChild(0).GetComponent<WheelCollider>();
       GameObject.Find("/robot/wheelGroup/wheelB/wheelJoint/omniWheel").GetComponent<Renderer>().material.color=Color.blue;
       //Cloning wheelC from wheelA
       GameObject wheelC = Object.Instantiate(wheelA);
       wheelC.name="wheelC";
       AddChild(wheelGroup,wheelC);
       wcC = wheelC.transform.GetChild(0).GetComponent<WheelCollider>();
       GameObject.Find("/robot/wheelGroup/wheelC/wheelJoint/omniWheel").GetComponent<Renderer>().material.color=Color.green;
       wcA.suspensionDistance=wcB.suspensionDistance=wcC.suspensionDistance=0.01f;
       wcA.forceAppPointDistance=wcB.forceAppPointDistance=wcC.forceAppPointDistance=0;
       JointSpring jsa=wcA.suspensionSpring,jsb=wcB.suspensionSpring,jsc=wcC.suspensionSpring;
       jsa.spring=jsb.spring=jsc.spring=0;
       jsa.damper=jsb.damper=jsc.damper=0;
       jsa.targetPosition=jsb.targetPosition=jsc.targetPosition=0;
       wcA.suspensionSpring=jsa;
       wcB.suspensionSpring=jsb;
       wcC.suspensionSpring=jsc;
       wcA.mass=wcB.mass=wcC.mass=1.5f;
 
 
       wheelA.transform.rotation*=Quaternion.Euler(0,0,0);
       wheelA.transform.localPosition+=new Vector3(2,0,0);
       robot.AddComponent<FixedJoint>().connectedBody=wheelA.GetComponent<Rigidbody>();
 
       wheelB.transform.rotation*=Quaternion.Euler(0,120,0);
       wheelB.transform.localPosition+=new Vector3(-1f,0,-1.732f);
       robot.AddComponent<FixedJoint>().connectedBody=wheelB.GetComponent<Rigidbody>();
 
       wheelC.transform.rotation*=Quaternion.Euler(0,240,0);
       wheelC.transform.localPosition+=new Vector3(-1f,0,1.732f);
       robot.AddComponent<FixedJoint>().connectedBody=wheelC.GetComponent<Rigidbody>();
 
       //place robot somewhere on the plane create by other process
       robot.transform.position=new Vector3(50,0.25f,50);
       locked=robot.transform.rotation.eulerAngles;

       ps = new PathScript(100,100);
       Terrain t = Terrain.activeTerrain;
       GameObject target = GameObject.Find("GoldenEgg");
       searchPath=ps.Solve(t,robot,target);
       

   }
 
   void Update(){
        PathCount++;
        if(PathCount%100==0){
            PathCount=0;
            Terrain t = Terrain.activeTerrain;
            GameObject target = GameObject.Find("GoldenEgg");
            searchPath=ps.Solve(t,robot,target);
        }
        if(searchPath!=null){
            foreach(List<float> a in searchPath){
                ps.DrawGuide(new Vector3(a[0],0.0f,a[1]),Color.black);
            }
        }

       float incremental=10;
       float maxBrake=Mathf.Pow(10,8);
       if (Input.GetKey("e")){
           wcA.brakeTorque=0;
           wcA.motorTorque+=incremental;
       }
       if (Input.GetKey("w")){
           wcC.brakeTorque=0;
           wcC.motorTorque+=incremental;
       }
       if (Input.GetKey("q")){
           wcB.brakeTorque=0;
           wcB.motorTorque+=incremental;
       }
       if (Input.GetKeyDown("space")){
               wcA.brakeTorque=wcB.brakeTorque=wcC.brakeTorque=maxBrake;
               wcA.motorTorque=wcB.motorTorque=wcC.motorTorque=0;
           //Debug.LogFormat("A:{0},B:{1},C{2}",wcA.motorTorque,wcB.motorTorque,wcC.motorTorque);
       }
       if (Input.GetKey("d")){
           wcA.brakeTorque=0;
           wcA.motorTorque-=incremental;
       }
       if (Input.GetKey("s")){
           wcC.brakeTorque=0;
           wcC.motorTorque-=incremental;
       }
       if (Input.GetKey("a")){
           wcB.brakeTorque=0;
           wcB.motorTorque-=incremental;
       }
       if (Input.GetKey("escape"))
       {
           Application.Quit();
       }
       
       //update omniWheelA rotation and wheelcollider rpm.
       GameObject.Find("/robot/wheelGroup/wheelA/wheelJoint/omniWheel").transform.rotation*=Quaternion.Euler(0,wcA.rpm/60*360,0);
       GameObject.Find("/robot/wheelGroup/wheelB/wheelJoint/omniWheel").transform.rotation*=Quaternion.Euler(0,wcB.rpm/60*360,0);
       GameObject.Find("/robot/wheelGroup/wheelC/wheelJoint/omniWheel").transform.rotation*=Quaternion.Euler(0,wcC.rpm/60*360,0);
 
    }
 
    void FixedUpdate(){
        transform.LookAt(robot.transform);
        transform.position=robot.transform.position+new Vector3(0,1,-10);

        //raycast to detect blockage
        RaycastHit hit;
        if(Physics.Raycast(robot.transform.position,robot.transform.forward,out hit,100)){
            if(hit.collider.gameObject.name=="Terrain"){
                Debug.DrawLine(robot.transform.position+new Vector3(0,1.53f,0),hit.point,Color.green);
                DrawLine(robot.transform.position+new Vector3(0,1.53f,0),hit.point,Color.green);
            }
        }
    }

    void LateUpdate(){
        //keep rotation of robot only for z, we don't want robot to flip.
        robot.transform.rotation = Quaternion.Euler(locked.x, robot.transform.rotation.eulerAngles.y ,locked.z);
    }

   void AddChild(GameObject parent, GameObject child){
       child.transform.SetParent(parent.transform);
   }

   

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material (Shader.Find ("Sprites/Default"));
        lr.startColor=color;
        lr.endColor=color;
        lr.startWidth=0.1f;
        lr.endWidth=0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }


}
 
 

