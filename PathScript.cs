using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathScript : MonoBehaviour{
    List<List<Color>> a=new List<List<Color>>();
    void Start()    {
        for(int j=0; j<20; j++){
            List<Color> b = new List<Color>();
            for(int i=0; i<4; i++){
                Debug.Log("Hey!");
                b.Add(new Color(i*0.25f,0.05f*j,0,1.0f));
            }
            a.Add(b);
        }
    }
    void Update(){
        for(float g=0.0f; g<100.0f; g=g+5.0f){
            for(float f=0.0f; f<100.0f; f=f+5.0f){
                Debug.DrawLine(new Vector3(f+2,5,g), new Vector3(f,5,g+2), a[(int)g/5][0]);
                Debug.DrawLine(new Vector3(f-2,5,g), new Vector3(f,5,g+2), a[(int)g/5][1]);
                Debug.DrawLine(new Vector3(f-2,5,g), new Vector3(f,5,g-2), a[(int)g/5][2]);
                Debug.DrawLine(new Vector3(f+2,5,g), new Vector3(f,5,g-2), a[(int)g/5][3]);
            }
        }
    }
}
