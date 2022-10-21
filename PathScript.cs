using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Linq;//for array append
public class PathScript : MonoBehaviour{
    public Terrain t;
    void Start(){
        
    }
    void DrawMark(Vector3 pos, Color c){
        //pos is our poin of interest on the grid.
        Debug.DrawLine(pos, pos+new Vector3(+0.25f,0,+0.25f), c);
        Debug.DrawLine(pos, pos+new Vector3(-0.25f,0,-0.25f), c);
        Debug.DrawLine(pos, pos+new Vector3(+0.25f,0,-0.25f), c);
        Debug.DrawLine(pos, pos+new Vector3(-0.25f,0,+0.25f), c);
    }
    void DrawDiamond(Vector3 pos, Color c){
        Debug.DrawLine(pos+new Vector3(+0.2f,0,0), pos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(pos+new Vector3(-0.2f,0,0), pos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(pos+new Vector3(-0.2f,0,0), pos+new Vector3(0,0,-0.2f), c);
        Debug.DrawLine(pos+new Vector3(+0.2f,0,0), pos+new Vector3(0,0,-0.2f), c);
    }
    void Update(){
        
        Vector3 azPos = gameObject.transform.position;
        //draw a grid around our agentzero
        for (float i=0.0f;i<100.0f;i+=1.0f){
            for (float j=0.0f;j<100.0f;j+=1.0f){
                float height=t.terrainData.GetHeight((int)(i*5.13f),(int)(j*5.13f));
                if (height > 0.0f){
                    DrawMark(new Vector3(i,0,j), Color.black);
                }else{
                    DrawDiamond(new Vector3(i,0,j), Color.red);
                }

            }
        }
        
    }

    public class Node{
        public List<Node> nodelist;
        public List<float> costlist;
        public float x;
        public float y;
        public float f_cost;
        public Node parent;
        public Node(){
            this.nodelist = new List<Node>();
            this.costlist=new List<float>();
            this.x=0.0f;
            this.y=0.0f;
            this.f_cost=-1.0f;
            this.parent=null;
        }
        public void AddNode(Node n, float cost){
            if (n==null) return;
            this.nodelist.Add(n);
            this.costlist.Add(cost);
        }
    }
}
