using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Coord{
        public int x;
        public int z;
        public Coord parent;
        public Coord next;
        public Coord(int x,int z,Coord parent){
            this.x=x;
            this.z=z;
            this.parent=parent;
            this.next=null;
        }
    }
public class PathScript{
    float MAP_MAX = (float)Math.Pow(10,10);
    int   [,] ig; //the grid keeps the type of path 0 wall, 1 path, 2 goal, 3 start.
    float [,] h_cost; //distance between this grid and goal. heruistics
    float [,] g_cost; //accumulated minimal travel distance from start. 
    float [,] f_cost; //sum of both, 
    bool  [,] visited; //if visited, don't visit again.
    bool  [,] added; //don't repeatedly add to list if already added.
    Coord [,] parent; //instead of which direction this grid was arrived from.
    List<Coord> opened = new List<Coord>(); //use a list to keep the unsearched coordinates.
    List<Coord> closed = new List<Coord>(); //this list keeps all the minimum tree. back traverse using parent will guarantee the shortest path.
    Vector3 lastStart,lastGoal,curStart,curGoal;
    float z_grid_size,x_grid_size,z_hmap_ratio,x_hmap_ratio; //this is the resolution of search.
    int z_sections,x_sections;
    int newStartZ, newStartX, newGoalZ, newGoalX;
    int heightMapRes = 513; //please make sure dimension of terrain is set to 100x100x100
    /*Constructor*/
    public PathScript(int xr,int zr){ 
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        this.z_sections=zr;
        this.x_sections=xr;
        this.z_grid_size = terrainSize.z/(float)zr;//terrainSize ratio, eg, real size of a grid, not world.
        this.x_grid_size = terrainSize.x/(float)xr;
        this.z_hmap_ratio = heightMapRes/zr;
        this.x_hmap_ratio = heightMapRes/xr;
        ig=null;
        h_cost = new float[x_sections,z_sections]; //distance between this grid and goal. heruistics
        g_cost = new float[x_sections,z_sections]; //accumulated minimal travel distance from start. 
        f_cost = new float[x_sections,z_sections]; //sum of all cost
        for (int i=0;i<g_cost.GetLength(0);i++){
            for (int j=0;j<g_cost.GetLength(1);j++){
                g_cost[i,j]=MAP_MAX;//cost is large initially. //this is to avoid search goes off the limit.
                f_cost[i,j]=MAP_MAX;//cost is large initially. //this is to avoid search goes off the limit.
            }
        }
        visited = new bool[x_sections,z_sections]; //if visited, don't visit again.
        added = new bool[x_sections,z_sections]; //if visited, don't visit again.
        parent = new Coord[x_sections,z_sections]; //instead of which direction this grid was arrived from.
    }
    bool NotVisited(int x, int z){
        return !visited[x,z] && !added[x,z];
    }
    bool IsNotWall(int x, int z){
        return ig[x,z]!=0;
    }
    bool CheckBounds(int x, int z){
        return (x>=0 && x<ig.GetLength(0) && z>=0 && z<ig.GetLength(1));
    }
    int[,]GetModel(int x,int z){
        int [,] model = {{x-1,z-1,1414},{x,z-1,1000},{x+1,z-1,1414},
                         {x-1,z  ,1000},             {x+1,z  ,1000},
                         {x-1,z+1,1414},{x,z+1,1000},{x+1,z+1,1414}};
        return model;
    }
    int GetLowestFCostFromOpened(List<Coord>o){
        float minF=Single.MaxValue; //incredibly large to start
        int pull=-1;
        for (int i=0;i<o.Count;i++){
            if(minF > f_cost[o[i].x, o[i].z]){
                minF = f_cost[o[i].x, o[i].z];
                pull=i;
            }
        }
        return pull;
    }
    /*
     * CreateGrid based on the height of the map, at this moment everything above 0 is a wall.
     * please add your threashold here.
    */
    int[,] CreateGrid(Terrain t, GameObject start, GameObject target){
        ig= new int[x_sections,z_sections];
        lastStart = lastGoal = curStart = curGoal = new Vector3();
        for (int j=0;j<z_sections-1;j++){
            for (int i=0;i<x_sections-1;i++){
                  float height=t.terrainData.GetHeight((int)(i*x_hmap_ratio),(int)(j*z_hmap_ratio));
                if (height > 0.0f){
                    ig[i,j]=0;
                }else{
                    ig[i,j]=1;
                }
            }
        }
        curStart.x=start.transform.position.x/this.x_grid_size; //centred
        curStart.z=start.transform.position.z/this.z_grid_size; //centred
        curGoal.x =target.transform.position.x/this.x_grid_size; //centred;
        curGoal.z =target.transform.position.z/this.z_grid_size; //centred;
        try{
        ig[(int)curStart.x, (int)curStart.z]=3;
        ig[(int)curGoal.x,  (int)curGoal.z] =2;
        lastStart = curStart;
        lastGoal = curGoal;
        }catch(IndexOutOfRangeException e){
            Debug.Log("Robot or Goal out of bounds.");
        }
        return ig;
    }
    public Coord FindPath(Terrain t, GameObject start, GameObject target){
        if (start.transform.position.x < t.transform.position.x ||
            start.transform.position.x > t.terrainData.size.x ||
            start.transform.position.z < t.transform.position.z || 
            start.transform.position.z > t.terrainData.size.z)return null;
        int[,] ig= CreateGrid(t, start,target);
        //once grid has been created, then solve it and provide a pathway.
        Coord cr = solve(  (int)(start.transform.position.x/ x_grid_size),
                (int)(start.transform.position.z/ z_grid_size),
                (int)(target.transform.position.x/ x_grid_size),
                (int)(target.transform.position.z/ z_grid_size)
        );
        return cr;
    }

    Coord solve(int sx, int sz, int gx, int gz){
        bool found = false;
        int A=0,B=0;
        float C=0.0f;
        opened.Add(new Coord(sx,sz,null)); //start search from sxsz.
        if (ig[sx,sz]==0){
            //when our bot is in the wall, stop searching.
            found = true;
            Debug.Log("no solve.");
        }
        if (ig[sx,sz]==2){
            found = true;
            Debug.Log("target arrived");
        }
        g_cost[sx,sz]=0; //required to start.
        while(! found && opened.Count>0){
            /*  [x-1,z-1] [x ,z-1] [x+1, z-1]
                [x-1,z]    start   [x+1, z]
                [x-1,z+1] [x, z+1] [x+1, z+1]
            */
            int pull=GetLowestFCostFromOpened(opened);//get smallest of all in opened.
            Coord c= opened[pull];
            opened.RemoveAt(pull); 
            if(visited[c.x,c.z])continue; 
            //check if target found.
            if (ig[c.x,c.z]==2){
               found = true;
               break;
            }
            visited[c.x,c.z]=true; //mark visit
            int[,]mdl=GetModel(c.x,c.z);
            for (int i=0;i< mdl.GetLength(0);i++){//go through neightbours
                A=mdl[i,0];B=mdl[i,1];C=(float)mdl[i,2]/1000.0f;//be accurate as possible.
                if (CheckBounds(A,B) && NotVisited(A,B) && IsNotWall(A,B))  {
                    if(g_cost[c.x,c.z]+C<g_cost[A,B])g_cost[A,B]=g_cost[c.x,c.z]+C;
                    if(h_cost[A,B]!=0.0f)h_cost[A,B]=(float)Math.Sqrt(Math.Pow((A-gx),2)+Math.Pow((B-gz),2));
                    if(f_cost[A,B]==MAP_MAX)f_cost[A,B]=g_cost[A,B]+ h_cost[A,B];
                    parent[A,B]=c;
                    added[A,B]=true;
                    opened.Add(new Coord(A,B,c));
                }else if(CheckBounds(A,B) && IsNotWall(A,B)){//just update gcost, otherwise gcost is not correct.
                    if(g_cost[c.x,c.z]+C<g_cost[A,B])g_cost[A,B]=g_cost[c.x,c.z]+C;
                }
            }
        }//end while loop  (found || opened.Count>0)
        Coord ba=parent[gx,gz];
        Coord bb=new Coord(0,0,null);
        //string ps="";
        while(ba != null){
            DrawGuide(new Vector3(ba.x,4.0f,ba.z),Color.blue);
            bb=ba;
            ba=parent[ba.x,ba.z];
            //if(ba!=null)ps+="["+ba.x+"],["+ba.z+"]";
            if(ba!=null)ba.next=bb;
        }
        //Debug.Log(ps);
        return bb; //because ba is now null(parent of start)
    }

    public void DrawGuide(Vector3 pos, Color c){
        Vector3 spos = pos * this.z_grid_size;
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
    }
}