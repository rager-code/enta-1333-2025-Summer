using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

    [System.Serializable]

    public class GridNode
    {
        public Vector3 cords;
        public bool explored;
        public bool path;
        public Node connectTo;

        public string Name;
        public Vector3 WorldPosition;
        public bool Walkable;
        public int Weight;

        public TerrainType terrainType;

        
        

    }

  