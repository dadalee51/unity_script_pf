using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Linq;//for array append
public class PathScript{
    List<List<int>>ig;
    Vector3 lastStart,lastGoal,curStart,curGoal;
    float zRes,xRes,zRatio,xRatio; //this is the resolution of search.
    public PathScript(float zr, float xr){
        this.zRes = zr;
        this.xRes = xr;
        this.zRatio = 513/zr;
        this.xRatio = 513/xr;
        Debug.Log(this.zRatio);
        ig=null;
    }
    void DrawMark(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRatio;
        //pos is our poin of interest on the grid.
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,+0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,+0.25f), c);
    }
    void DrawDiamond(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRatio;
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
    }
    public void DrawGuide(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRatio;
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
    }

    public void checkGrid(){
        string test="";
        foreach (var i in this.ig){
            foreach (var j in i){
                test+=j;
            }
            test+="\n";
        }
        Debug.Log(test);
    }

    //update grid, create most of it if not exist.
    List<List<int>> UpdateGrid(Terrain t, GameObject start, GameObject target){
        if (ig!=null){
            //checkGrid();
            if (ig.Count==0){
                Debug.Log("Error with grid count, should not be zero. Check resolution.");
            }
            //if this is a Update call, ig will not be null, so we simply update the last position and erase start and goal position 
            // to avoid multiple start and goal positions.
            //then just update the ig[j][i] for (start or goal).transform.position.z and x
            //if (lastStart != null && (int)lastStart.z < ig.Count && (int)lastStart.x < ig[0].Count){
            /*Debug.Log(ig.Count);
            Debug.Log("lastStartz and x:"+(int)lastStart.z + ":" +(int)lastStart.x);
            Debug.Log("curStartz and x:"+(int)curStart.z + ":" +(int)curStart.x);
            Debug.Log("lastGoal z and x:"+(int)lastGoal.z + ":" +(int)lastGoal.x);
            Debug.Log("curGoalz and x:"+(int)curGoal.z + ":" +(int)curGoal.x);
            */
            ig[(int)lastStart.z][(int)lastStart.x]=1;
            ig[(int)lastGoal.z][(int)lastGoal.x]=1;
            //}
            curStart.x=start.transform.position.x/this.xRatio;
            curStart.z=start.transform.position.z/this.zRatio;
            curGoal.x =target.transform.position.x/this.xRatio;
            curGoal.z =target.transform.position.z/this.zRatio;
            lastStart = curStart;
            lastGoal = curGoal;
            ig[(int)curStart.z][(int)curStart.x]=3;
            ig[(int)curGoal.z][(int)curGoal.x]=2;
            return ig;
        }
        ig= new List<List<int>>();
        lastStart = lastGoal = curStart = curGoal = new Vector3();
        //draw a grid around our start object
        for (float i=0.0f;i<xRes;i+=1.0f){
            List<int>li = new List<int>();
            for (float j=0.0f;j<zRes;j+=1.0f){
                int type=-1;
                float height=t.terrainData.GetHeight((int)(j*zRatio),(int)(i*xRatio));
                if (height > 0.0f){
                    type=0;
                }else{
                    type=1;
                }
                li.Add(type);
            }
            ig.Add(li);
        }
        curStart.x=start.transform.position.x/this.xRatio;
        curStart.z=start.transform.position.z/this.zRatio;
        curGoal.x =target.transform.position.x/this.xRatio;
        curGoal.z =target.transform.position.z/this.zRatio;
        lastStart = curStart;
        lastGoal = curGoal;
        ig[(int)curStart.z][(int)curStart.x]=3;
        ig[(int)curGoal.z][(int)curGoal.x]=2;
        return ig;
    }

    public List<List<float>> Solve(Terrain t, GameObject start, GameObject target){
        var ig= UpdateGrid(t, start,target);
        AStar astar=new AStar(ig);
        //astar.check();
        var ans = astar.solve();
        return ans;
    }
    /*
        our coordinates: 
        x: red axis: width => j
        z: blue axis: depth => i
        y: green axis: height 
        grid dimension [i][j] is equivalent to grid[z][x]
    */

    public class Node{
        public List<Node> nodelist;
        public List<float> costlist;
        public List<bool> diaglist;
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
            this.diaglist=new List<bool>();//add information about whether it is a diagonal node to the orgiinal node.
            this.x=0.0f;
            this.z=0.0f;
            this.f_cost=-1.0f;
            this.parent=null;
        }
        public void AddNode(Node n, float cost, bool isDiag){
            if (n==null) return;
            this.nodelist.Add(n);
            this.costlist.Add(cost);
            this.diaglist.Add(isDiag);
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
                    n.z=(float)i;
                    n.x=(float)j;
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
					if(i-1>=0)grid[i][j].AddNode(grid[i-1][j],10,false);
					if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j],10,false);
					if(j-1>=0){
						grid[i][j].AddNode(grid[i][j-1],10,false);
						if (this.allow_diagonals){
							if(i-1>=0)grid[i][j].AddNode(grid[i-1][j-1],14,true);
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j-1],14,true);
						}
					}
					if(j+1<grid[i].Count){
						grid[i][j].AddNode(grid[i][j+1],10,false);
						if (this.allow_diagonals){
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j+1],14,true);
							if(i-1>grid.Count)grid[i][j].AddNode(grid[i-1][j+1],14,true);
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
                int index=0;
                foreach(Node n in cheapest_unvisited.nodelist){
                    if (visited.Contains(n))continue;
                    if (!unvisited.Contains(n)){
                        if (cheapest_unvisited.diaglist[index]){
                            n.g_cost=cheapest_unvisited.g_cost+14;
                        }else{ 
                            n.g_cost=cheapest_unvisited.g_cost+10;
                        }
                        n.GetFCost(this.goal);
                        unvisited.Add(n);
                    }
                    n.parent=cheapest_unvisited;
                    index++;
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
            //Debug.Log(debug_path);
            resolved.Reverse();
            //debug_path="";
            List<List<float>>lout=new List<List<float>>();
            foreach(Node r in resolved){
                //debug_path+=r.name+"->";
                List<float>lo=new List<float>();
                lo.Add(r.x);
                lo.Add(r.z);
                lout.Add(lo);
            }
            //Debug.Log(debug_path);
            return lout;
        }
    }
}
