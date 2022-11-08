using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathScript{

    private class Coord{
        int x;
        int z;
        public Coord(int x,int z){
            this.x=x;
            this.z=z;
        }
    }
    int   [,] ig; //the grid keeps the type of path 0 wall, 1 path, 2 goal, 3 start.
    float [,] h_cost; //distance between this grid and goal. heruistics
    float [,] g_cost; //accumulated minimal travel distance from start. 
    float [,] f_cost; //sum of both, 
    bool  [,] visited; //if visited, don't visit again.
    Coord [,] parent; //instead of which direction this grid was arrived from.
    List<Coord> opened; //use a list to keep the unsearched coordinates.
    
    //visit each unvisited neighbours, if ig value is not 0, nor visited,
    void solve(int sx, int sz, int gx, int gz){
        bool found = false;
        while(! found){
            /*  [x-1,z-1] [x ,z-1] [x+1, z-1]
                [x-1,z]    start   [x+1, z]
                [x+1,z+1] [x, z+1] [x+1, z+1]
            */

            if (ig[sx,sz]==0){
                //when our bot is in the wall, stop searching.
                found = true;
                Debug.Log("no solve.");
            }
            
            if (ig[sx,sz]==2){
                found = true;
                Debug.Log("target arrived");
            }

            

        }
    }

    Vector3 lastStart,lastGoal,curStart,curGoal;
    float z_grid_size,x_grid_size,z_hmap_ratio,x_hmap_ratio; //this is the resolution of search.
    int z_sections,x_sections;
    int newStartZ, newStartX, newGoalZ, newGoalX;
    int heightMapRes = 513;
    
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
        //TODO
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