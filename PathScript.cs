using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Linq;//for array append
public class PathScript : MonoBehaviour{
    public Terrain t;
    void Start(){
        AStar astar=new AStar(10,10,0,0,9,9);
		astar.check();
        astar.solve();
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
        public AStar(int width, int depth, int start_x, int start_z, int goal_x, int goal_z){
            grid = new List<List<Node>>();
            //create the grid from the width and height, mark the start and goal position, then solve.
            for (int i=0;i<depth;i++){
                List<Node> ln = new List<Node>();
                for(int j=0;j<width;j++){
                    Node n = new Node();
                    n.x=(float)j;
                    n.z=(float)i;
                    n.name="["+i+"]["+j+"]";
                    ln.Add(n);
                    if (start_x==j && start_z== i){
                        this.start=n;
                        n.type=3;
                    }else if(goal_x==j && goal_z==i){
                        this.goal=n;
                        n.type=2;
                    }else{
                        n.type=1;//by default is a path.
                    }
                }
                grid.Add(ln);
            }
            //compose nodelist for every node.
            for (int i=0;i<depth;i++){
                for(int j=0;j<width;j++){
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

        public void solve(){
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
            foreach(Node r in resolved){
                debug_path+=r.name+"->";
            }
            Debug.Log(debug_path);
        }
    }


}
