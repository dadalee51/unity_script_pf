using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Linq;//for array append
public class PathScript : MonoBehaviour{
    public Terrain t;
    public GameObject target;
    bool doSolve;
    List<List<float>>ans = new List<List<float>>();
    void Start(){
        doSolve=true;
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
    void DrawGuide(Vector3 pos, Color c){
        Debug.DrawLine(pos+new Vector3(+0.3f,0,0), pos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(pos+new Vector3(-0.3f,0,0), pos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(pos+new Vector3(-0.3f,0,0), pos+new Vector3(0,0,-0.3f), c);
        Debug.DrawLine(pos+new Vector3(+0.3f,0,0), pos+new Vector3(0,0,-0.3f), c);
    }
    void Update(){
        Vector3 azPos = gameObject.transform.position;
        List<List<int>> ig= new List<List<int>>();
        
        //draw a grid around our agentzero
        for (float i=0.0f;i<100.0f;i+=1.0f){
            List<int>li = new List<int>();
            for (float j=0.0f;j<100.0f;j+=1.0f){
                int type=-1;
                float height=t.terrainData.GetHeight((int)(i*5.13f),(int)(j*5.13f));
                if (height > 0.0f){
                    //DrawMark(new Vector3(i,0,j), Color.black);
                    type=0;
                }else{
                    //DrawDiamond(new Vector3(i,0,j), Color.red);
                    type=1;
                }
                if (transform.position.z > i && transform.position.z < i+10 && 
                    transform.position.x > j && transform.position.x < j+10){
                    type=3;
                }
                if (target.transform.position.z > i && target.transform.position.z < i+10 && 
                    target.transform.position.x > j && target.transform.position.x < j+10){
                    type=2;
                }
                li.Add(type);
            }
            ig.Add(li);
        }
        if(doSolve){
            AStar astar=new AStar(ig);
            astar.check();
            ans = astar.solve();
            doSolve=false;
        }
        if (ans!=null){
            foreach(List<float> a in ans){
                DrawGuide(new Vector3(a[0],5.0f,a[1]),Color.black);
            }
        }
    }
    /*
        our coordinates: 
        x: red axis: width
        z: blue axis: depth
        y: green axis: height
    */

    public class Node{
        public List<Node> nodelist;
        public List<float> costlist;
        public float x;
        public float z;
        public float f_cost;
        public float g_cost;
        public string name;
        public int type; //the type of this node, 0=wall, 1=path, 2=goal, 3=start
        public Node parent;
        public Node(){
            this.nodelist = new List<Node>();
            this.costlist=new List<float>();
            this.x=0.0f;
            this.z=0.0f;
            this.f_cost=-1.0f;
            this.parent=null;
        }
        public void AddNode(Node n, float cost){
            if (n==null) return;
            this.nodelist.Add(n);
            this.costlist.Add(cost);
        }
        public float GetFCost(Node target){
            this.f_cost=(float)Math.Sqrt(Math.Pow(this.x-target.x ,2) + Math.Pow(this.z-target.z ,2)) + this.g_cost;
            return this.f_cost;
        }
    }
    //add a Astar class which keeps a internal grid for solving the path.
    public class AStar{
        public bool allow_diagonals = true;
        public bool debug;
        public Node start;
        public Node goal;
        public List<List<Node>> grid;
        //here we are changing the constructor of the astar from just a bunch of x and z values 
        // to a 2d array.
        public AStar(List<List<int>> input_grid){
            grid = new List<List<Node>>();
            //create the grid from the width and height, mark the start and goal position, then solve.
            for (int i=0;i<input_grid.Count;i++){
                List<Node> ln = new List<Node>();
                for(int j=0;j<input_grid[i].Count;j++){
                    Node n = new Node();
                    n.x=(float)j;
                    n.z=(float)i;
                    n.name="["+i+"]["+j+"]"; //name was required to show the path in debug.
                    ln.Add(n);
                    if (input_grid[i][j]==3){
                        this.start=n;
                        n.type=3;
                    }else if(input_grid[i][j]==2){
                        this.goal=n;
                        n.type=2;
                    }else if(input_grid[i][j]==1){
                        n.type=1;//a pathway.
                    }else if(input_grid[i][j]==0){
                        n.type=0;//a wall.
                    }
                }
                grid.Add(ln);
            }
            //compose nodelist for every node.
            for (int i=0;i<input_grid.Count;i++){
                for(int j=0;j<input_grid[i].Count;j++){
					if(grid[i][j].type==0)continue;
					if(i-1>=0)grid[i][j].AddNode(grid[i-1][j],10);
					if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j],10);
					if(j-1>=0){
						grid[i][j].AddNode(grid[i][j-1],10);
						if (this.allow_diagonals){
							if(i-1>=0)grid[i][j].AddNode(grid[i-1][j-1],14);
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j-1],14);
						}
					}
					if(j+1<grid[i].Count){
						grid[i][j].AddNode(grid[i][j+1],10);
						if (this.allow_diagonals){
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j+1],14);
							if(i-1>grid.Count)grid[i][j].AddNode(grid[i-1][j+1],14);
						}
					}
                }
            }
        }
        public void check(){
            string debug="";
			for (int i=0; i<grid.Count;i++){
				for(int j=0; j<grid[i].Count;j++){
					debug=debug+","+(grid[i][j].type);
				}
                debug=debug+"\n";
			}
            Debug.Log(debug);
		}

        public List<List<float>> solve(){
            List<Node> unvisited=new List<Node>();
            List<Node> visited = new List<Node>();
            bool targetNotFound=true;
            unvisited.Add(this.start);
            float MAXCOST=(float)Math.Pow(10,10);
            List<Node> resolved=new List<Node>();
            while (targetNotFound){
                float cheapest_cost=MAXCOST;
                Node cheapest_unvisited=null;
                foreach (Node u in unvisited){
                    if(u.f_cost==MAXCOST){
                        u.GetFCost(this.goal);
                    }
                    if(u.f_cost<cheapest_cost){
                        cheapest_cost=u.f_cost;
                        cheapest_unvisited=u;
                    }
                }
                if(unvisited.Count>0)unvisited.Remove(cheapest_unvisited);
                else {
                    Debug.Log("Cannot solve!");
                    break;
                }
                visited.Add(cheapest_unvisited);
                if (cheapest_unvisited == this.goal){
                    targetNotFound=false;
                    this.goal.parent=cheapest_unvisited.parent;
                }
                foreach(Node n in cheapest_unvisited.nodelist){
                    if (visited.Contains(n))continue;
                    if (!unvisited.Contains(n)){
                        n.g_cost=cheapest_unvisited.g_cost+1;
                        n.GetFCost(goal);
                        unvisited.Add(n);
                    }
                    n.parent=cheapest_unvisited;
                }
            }
            //resolved path:
            resolved.Add(goal);
            Node checker = goal;
            string debug_path="";
            while(checker.parent != null){
                debug_path+=checker.name+"->";
                checker = checker.parent;
                resolved.Add(checker);
            }
            debug_path+=checker.name;
            Debug.Log(debug_path);
            resolved.Reverse();
            debug_path="";
            List<List<float>>lout=new List<List<float>>();
            foreach(Node r in resolved){
                debug_path+=r.name+"->";
                List<float>lo=new List<float>();
                lo.Add(r.z);
                lo.Add(r.x);
                lout.Add(lo);
            }
            Debug.Log(debug_path);
            return lout;
        }
    }
}
