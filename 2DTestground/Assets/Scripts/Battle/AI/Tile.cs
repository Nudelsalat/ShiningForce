using System;
using UnityEngine;

namespace Assets.Scripts.Battle.AI {

    public class Tile {
        public Vector3 Point;
        public float Cost;
        public float Distance;
        public float CostDistance => Cost + Distance;
        public Tile Parent;

        public Tile(Vector3 point, float cost, Tile parent, Vector3 target) {
            Point = point;
            Cost = cost;
            Parent = parent;
            SetDistance(target);
        }

        public Tile(Vector3 point, float cost, Tile parent) {
            Point = point;
            Cost = cost;
            Parent = parent;
            Distance = 0;
        }

        public void SetDistance(Vector3 target) {
            Distance = Math.Abs(target.x - Point.x) + Math.Abs(target.y - Point.y);
        }
    }
}
