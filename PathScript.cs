using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathScript{

    private class Coord{
        public int x;
        public int z;
        public Coord parent;
        public Coord(int x,int z,Coord parent){
            this.x=x;
            this.z=z;
            this.parent=parent;
        }

    }
    int   [,] ig; //the grid keeps the type of path 0 wall, 1 path, 2 goal, 3 start.
    float [,] h_cost; //distance between this grid and goal. heruistics
    float [,] g_cost; //accumulated minimal travel distance from start. 
    float [,] f_cost; //sum of both, 
    bool  [,] visited; //if visited, don't visit again.
    bool  [,] added; //don't repeatedly add to list if already added.
    Coord [,] parent; //instead of which direction this grid was arrived from.
    List<Coord> opened = new List<Coord>(); //use a list to keep the unsearched coordinates.
    
    Vector3 lastStart,lastGoal,curStart,curGoal;
    float z_grid_size,x_grid_size,z_hmap_ratio,x_hmap_ratio; //this is the resolution of search.
    int z_sections,x_sections;
    int newStartZ, newStartX, newGoalZ, newGoalX;
    int heightMapRes = 513;
    

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
        int [,] model = {{x-1,z-1,14},{x,z-1,10},{x+1,z-1,14},
                         {x-1,z  ,10},           {x+1,z  ,10},
                         {x-1,z+1,14},{x,z+1,10},{x+1,z+1,14}};
        return model;
    }
    //visit each unvisited neighbours, if ig value is not 0, nor visited,
    //given: ig contains all the ints of the maze.
    void solve(int sx, int sz, int gx, int gz){
        bool found = false;
        int A=0,B=0;
        float C=0.0f;
        
        int breaker=0;
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
            
            //how to reduce the chances of putting Coords in hree?

            //print all 
            
            
            
            breaker++;
            if(breaker>1600){
                Debug.Log("limit break@!");
                break;
            }
            /*  [x-1,z-1] [x ,z-1] [x+1, z-1]
                [x-1,z]    start   [x+1, z]
                [x-1,z+1] [x, z+1] [x+1, z+1]
            */
            Coord c= opened[0];
            //if(c.parent!=null)Debug.Log("["+c.x+","+c.z+"]:addedBy["+c.parent.x+","+c.parent.z+"]");
            opened.RemoveAt(0); //c - current - direction could be modelled here.
            if(visited[c.x,c.z])continue; 
            visited[c.x,c.z]=true; //mark visit
            //mark hcosts
            h_cost[c.x,c.z]=(float)Math.Sqrt(Math.Pow(sx-gx,2)+Math.Pow(sz-gz,2));
            //by the time hcost is calcuated, gcode was ready too
            //get fcost
            f_cost[c.x,c.z]=g_cost[c.x,c.z]+ h_cost[c.x,c.z];
            //Debug.Log("fcost:"+f_cost[c.x,c.z]+" H:"+h_cost[c.x,c.z]+"gCost"+g_cost[c.x,c.z]);
            int[,]mdl=GetModel(c.x,c.z);
            //don't add to list if already added.
            for (int i=0;i< mdl.GetLength(0);i++){
                A=mdl[i,0];B=mdl[i,1];C=(float)mdl[i,2]/10.0f;
                if (CheckBounds(A,B) && NotVisited(A,B) && IsNotWall(A,B))  {
                    //mark gcost, if already exist, check if its smaller
                    if (g_cost[A,B]>g_cost[c.x,c.z]+C) g_cost[A,B]=g_cost[c.x,c.z]+C;
                    added[A,B]=true;
                    opened.Add(new Coord(A,B,c));
                }
            }
        }//end while loop  (found || opened.Count>0)
        string ss="";
        for(int i=0;i<ig.GetLength(0);i++){
            for (int j=0;j<ig.GetLength(1);j++){
                //ss+=added[i,j]?1+" ":0+" ";
                ss+=(int)f_cost[i,j]/5+" ";
            }
            ss+="\n";
        }
        Debug.Log(ss);
    }

    
    /*
    Traversal rule: start with z axis because when printing on console, it shows the z (vertical) direction
    at least we don't have to guess x (horizontal position)
    */

    public PathScript(int xr,int zr){ 
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        this.z_sections=zr;
        this.x_sections=xr;
        this.z_grid_size = terrainSize.z/(float)zr;//terrainSize ratio, eg, real size of a grid, not world.
        this.x_grid_size = terrainSize.x/(float)xr;
        this.z_hmap_ratio = heightMapRes/zr;
        this.x_hmap_ratio = heightMapRes/xr;
        ig=null;
        //Debug.Log("TerrainSize:"+terrainSize.z);//100
        h_cost = new float[x_sections,z_sections]; //distance between this grid and goal. heruistics
        g_cost = new float[x_sections,z_sections]; //accumulated minimal travel distance from start. 
        for (int i=0;i<g_cost.GetLength(0);i++)
            for (int j=0;j<g_cost.GetLength(1);j++)
                g_cost[i,j]=(float)Math.Pow(10,10);//g cost is large initially.
        f_cost = new float[x_sections,z_sections]; //sum of both, 
        visited = new bool[x_sections,z_sections]; //if visited, don't visit again.
        added = new bool[x_sections,z_sections]; //if visited, don't visit again.
        parent = new Coord[x_sections,z_sections]; //instead of which direction this grid was arrived from.
    }
    //precondition: when ig is null, call this method first.
    int[,] CreateGrid(Terrain t, GameObject start, GameObject target){
        ig= new int[x_sections,z_sections];
        lastStart = lastGoal = curStart = curGoal = new Vector3();
        for (int j=0;j<z_sections;j++){
            for (int i=0;i<x_sections;i++){
                  float height=t.terrainData.GetHeight((int)(i*x_hmap_ratio),(int)(j*z_hmap_ratio));
                if (height > 0.0f){
                    ig[i,j]=0;
                }else{
                    ig[i,j]=1;
                }
            }
        }
        curStart.x=start.transform.position.x/this.x_grid_size;
        curStart.z=start.transform.position.z/this.z_grid_size;
        curGoal.x =target.transform.position.x/this.x_grid_size;
        curGoal.z =target.transform.position.z/this.z_grid_size;
        ig[(int)curStart.x, (int)curStart.z]=3;
        ig[(int)curGoal.x,  (int)curGoal.z] =2;
        lastStart = curStart;
        lastGoal = curGoal;
        return ig;
        //the ig should be used for asolver to do update and traversals.
    }

    public void FindPath(Terrain t, GameObject start, GameObject target){
        if (start.transform.position.x < t.transform.position.x ||
            start.transform.position.x > t.terrainData.size.x ||
            start.transform.position.z < t.transform.position.z || 
            start.transform.position.z > t.terrainData.size.z)return;
        int[,] ig= CreateGrid(t, start,target);
        //debugging ps line:
        string ps="";
        for(int j=0;j<z_sections;j++){
            for(int i=0;i<x_sections;i++){
                ps+=ig[i,j]+"";
            }
            ps+="\n";
        }
        Debug.Log(ps);
        //once grid has been created, then solve it and provide a pathway.
        solve(  (int)start.transform.position.x/x_sections,
                (int)start.transform.position.z/z_sections,
                (int)target.transform.position.x/x_sections,
                (int)target.transform.position.z/z_sections
        );
    }
    /*
        our coordinates: 
        z: blue axis: depth => i
        x: red axis: width => j
        y: green axis: height 
        grid dimension [i][j] is equivalent to grid[z][x]
    */
    void DrawMark(Vector3 pos, Color c){
        Vector3 spos = pos * this.z_grid_size;
        //pos is our poin of interest on the grid.
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,+0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(+0.25f,0,-0.25f), c);
        Debug.DrawLine(spos, spos+new Vector3(-0.25f,0,+0.25f), c);
    }
    void DrawDiamond(Vector3 pos, Color c){
        Vector3 spos = pos * this.z_grid_size;
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,+0.2f), c);
        Debug.DrawLine(spos+new Vector3(-0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
        Debug.DrawLine(spos+new Vector3(+0.2f,0,0), spos+new Vector3(0,0,-0.2f), c);
    }
    public void DrawGuide(Vector3 pos, Color c){
        Vector3 spos = pos * this.z_grid_size;
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,+0.3f), c);
        Debug.DrawLine(spos+new Vector3(-0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
        Debug.DrawLine(spos+new Vector3(+0.3f,0,0), spos+new Vector3(0,0,-0.3f), c);
    }

    
}