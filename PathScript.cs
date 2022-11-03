using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Linq;//for array append
public class PathScript{
    List<List<int>>ig;
    Vector3 lastStart,lastGoal,curStart,curGoal;
    float zr,xr,zRes,xRes,zRatio,xRatio; //this is the resolution of search.
    int newStartZ, newStartX, newGoalZ, newGoalX;
    int heightMapRes = 513;
    //there are two type of resolution, one is the heightMap, one is our own grid to world resolution. don't confuse them.
    AStar asv;
    public PathScript(float zr, float xr){ //zr is how many pieces we want to cut our search space.
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        this.zr=zr;
        this.xr=xr;
        this.zRes = terrainSize.z/zr;//this is the size of our grid
        this.xRes = terrainSize.x/xr;
        this.zRatio = heightMapRes/zr; //only used for heightmap detection
        this.xRatio = heightMapRes/xr;
        ig=null;
        asv=null;
        //Debug.Log("TerrainSize:"+terrainSize.z);//100
    }
    //update grid, create most of it if not exist.
    List<List<int>> UpdateGrid(Terrain t, GameObject start, GameObject target){
        if (ig==null){
            ig= new List<List<int>>();
            lastStart = lastGoal = curStart = curGoal = new Vector3();
            //draw a grid around our start object
            for (float i=0.0f;i<xr;i+=1.0f){
                List<int>li = new List<int>();
                for (float j=0.0f;j<zr;j+=1.0f){
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
        }else{
            checkGrid();
            if (ig.Count==0){
                Debug.Log("Error with grid count, should not be zero. Check resolution.");
            }else{
                //Debug.Log(curStart.z +","+ curStart.x);
            }
        }
        curStart.x=start.transform.position.x/this.xRes;
        curStart.z=start.transform.position.z/this.zRes;
        curGoal.x =target.transform.position.x/this.xRes;
        curGoal.z =target.transform.position.z/this.zRes;
        //Debug.Log(curStart.z +","+ curStart.x);
        if(this.asv !=null)this.asv.UpdateStartGoalPosition((int)curStart.z,(int)curStart.x,(int)curGoal.z,(int)curGoal.x);
        lastStart = curStart;
        lastGoal = curGoal;
        ig[(int)curStart.z][(int)curStart.x]=3;
        ig[(int)curGoal.z][(int)curGoal.x]=2;
        return ig;
    }

    public List<List<float>> Solve(Terrain t, GameObject start, GameObject target){
        if (start.transform.position.x < t.transform.position.x ||
            start.transform.position.x > t.terrainData.size.x ||
            start.transform.position.z < t.transform.position.z || 
            start.transform.position.z > t.terrainData.size.z)return null;
        var ig= UpdateGrid(t, start,target);
        if (this.asv ==null){
            this.asv=new AStar(ig, this.zRes);
        }
        //this.asv.check(); //check the current node list.
        return this.asv.solve();
    }
    /*
        our coordinates: 
        z: blue axis: depth => i
        x: red axis: width => j
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
            this.f_cost=(float)Math.Pow(10,10);
            this.parent=null;
        }
        public void AddNode(Node n, float cost, bool isDiag){
            if (n==null) return;
            this.nodelist.Add(n);
            this.costlist.Add(cost);
            this.diaglist.Add(isDiag);
        }
        public float GetFCost(Node target, float zRes){
            this.f_cost=(float)Math.Sqrt(Math.Pow(this.x-target.x ,2) + Math.Pow(this.z-target.z ,2))/zRes + this.g_cost;
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
        public float zRes;
        //here we are changing the constructor of the astar from just a bunch of x and z values 
        // to a 2d array.
        public AStar(List<List<int>> input_grid, float zRes){
            this.zRes = zRes;
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
            float diagCost=1.4f*zRes;
            for (int i=0;i<input_grid.Count;i++){
                for(int j=0;j<input_grid[i].Count;j++){
					if(grid[i][j].type==0)continue;
					if(i-1>=0)grid[i][j].AddNode(grid[i-1][j],zRes,false);
					if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j],zRes,false);
					if(j-1>=0){
						grid[i][j].AddNode(grid[i][j-1],zRes,false);
						if (this.allow_diagonals){
							if(i-1>=0)grid[i][j].AddNode(grid[i-1][j-1],diagCost,true);
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j-1],diagCost,true);
						}
					}
					if(j+1<grid[i].Count){
						grid[i][j].AddNode(grid[i][j+1],zRes,false);
						if (this.allow_diagonals){
							if(i+1<grid.Count)grid[i][j].AddNode(grid[i+1][j+1],diagCost,true);
							if(i-1>grid.Count)grid[i][j].AddNode(grid[i-1][j+1],diagCost,true);
						}
					}
                }
            }
        }

        //we want a method that just update the goal and target position without reconstructing the search nodes.
        public void UpdateStartGoalPosition(int startZ,int startX, int goalZ, int goalX){
            //replace old types as just paths.
            this.start.type=1;
            this.goal.type=1;
            this.start=grid[startZ][startX];
            this.goal=grid[goalZ][goalX];
            this.start.type=3;
            this.goal.type=2;
        }

        public void check(){
            string debug="";
			for (int i=0; i<grid.Count;i++){
				for(int j=0; j<grid[i].Count;j++){
					debug=debug+",["+(grid[i][j].type+"]"+(int)grid[i][j].f_cost);
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
            //Debug.Log("start.parent:"+this.start.parent);
            //Debug.Log(this.start+"==This.Start.Node:"+this.start.x+","+this.start.z);
            //Debug.Log(this.goal+"==This.Goal.Node:"+this.goal.x+","+this.goal.z);
            float MAXCOST=(float)Math.Pow(10,10);
            List<Node> resolved=new List<Node>();
            int countSolveStep=0;//if greater than 100 steps then quit solving.
            Node cheapest_unvisited;
            while (targetNotFound){
                countSolveStep++;
                if(countSolveStep>200)break;
                //Debug.Log("solve loop counter:"+countSolveStep+",last cheapest unvisited:"+cheapest_unvisited);
                float cheapest_cost=MAXCOST;
                cheapest_unvisited=null;
                foreach (Node u in unvisited){
                    if(u.f_cost==MAXCOST){
                        u.GetFCost(this.goal,this.zRes);
                    }
                    if(u.f_cost<cheapest_cost){
                        cheapest_cost=u.f_cost;
                        cheapest_unvisited=u;
                    }
                }
                Debug.Log("After one scan, cheaptest_cost:"+cheapest_unvisited.f_cost+" cheapest_unvisited:"+cheapest_unvisited.x+","+cheapest_unvisited.z+" unvisited count:"+unvisited.Count);
                if(unvisited.Count>0)unvisited.Remove(cheapest_unvisited);
                else {
                    Debug.Log("Cannot solve!");
                    break;
                }
                visited.Add(cheapest_unvisited);
                /*
                foreach(Node m in visited){
                    Debug.Log("Visited:"+m.x+","+m.z);
                }
                */
                if (cheapest_unvisited == this.goal){
                    targetNotFound=false;
                    this.goal.parent=cheapest_unvisited.parent;
                }
                int index=0;
                //string logNodeList="";
                foreach(Node n in cheapest_unvisited.nodelist){
                    //logNodeList +="  ["+n.x+"],["+n.z+"]";
                    if (visited.Contains(n))continue;
                    if (!unvisited.Contains(n)){
                        if (cheapest_unvisited.diaglist[index]==true){
                            n.g_cost=cheapest_unvisited.g_cost+1.4f*zRes;
                        }else{ 
                            n.g_cost=cheapest_unvisited.g_cost+zRes;
                        }
                        n.GetFCost(this.goal,this.zRes);
                        unvisited.Add(n);
                    }
                    n.parent=cheapest_unvisited;
                    index++;
                }
                /*
                Debug.Log("cheapest unvisited neighbours:"+logNodeList);
                string listVisited="";
                foreach(Node n in visited){
                    listVisited+="==["+n.x+"]["+n.z+"]";
                }
                Debug.Log("List of visited:"+listVisited);
                */
            }
            //resolved path:
            resolved.Add(goal);
            Node checker = goal;
            //string debug_path="";
            //int countParentCheck=0;
            while(checker.parent != start){
                //Debug.Log("check parent loop begin."+countParentCheck);
                //if(countParentCheck>200)break;
                //countParentCheck++;
                //debug_path+=checker.name+"->";
                checker = checker.parent;
                resolved.Add(checker);
            }
            //debug_path+=checker.name;
            //Debug.Log(">>>>>>>>>>>path>>>>"+debug_path);
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
            //Debug.Log("<<<<<<<<<<<<path<<<<<<<<<"+debug_path);
            return lout;
        }
    }


    void DrawMark(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRes;
        //pos is our poin of interest on the grid.
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,+0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,+0.25f), c);
    }
    void DrawDiamond(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRes;
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
    }
    public void DrawGuide(Vector3 pos, Color c){
        Vector3 spos = pos * this.zRes;
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
        //Debug.Log(test);
    }

}