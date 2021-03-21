using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Battle {
    public class Pathfinder {
        private List<Tile> _activeTiles = new List<Tile>();
        private List<Tile> _visitedTiles = new List<Tile>();
        private List<Vector3> _walkablePoints;

        private Vector3 _startPoint;
        private Vector3 _targetPoint;

        public Pathfinder(List<Vector3> walkablePoints, Vector3 startPoint, Vector3 targetPoint) {
            _walkablePoints = walkablePoints;
            _startPoint = startPoint;
            _targetPoint = targetPoint;
        }

        public List<Vector3> GetShortestPath() {
            if (_walkablePoints == null) {
                var result = new List<Vector3>();
                result.Add(new Vector3(_targetPoint.x, _startPoint.y, _targetPoint.z));
                result.Add(new Vector3(_targetPoint.x, _targetPoint.y, _targetPoint.z));
                return result;
            }

            var startTile = new Tile(_startPoint, 0, null, _targetPoint);
            _activeTiles.Add(startTile);

            while (_activeTiles.Any()) {
                var checkTile = _activeTiles.OrderBy(x => x.CostDistance).First();

                if (checkTile.Point == _targetPoint) {
                    Debug.Log("We are at the destination!");
                    //We can actually loop through the parents of each tile to find our exact path which we will show shortly. 
                    return GetResult(checkTile);
                }

                _visitedTiles.Add(checkTile);
                _activeTiles.Remove(checkTile);

                var walkableTiles = GetWalkableTiles(checkTile, _targetPoint);

                foreach (var walkableTile in walkableTiles) {
                    //We have already visited this tile so we don't need to do so again!
                    if (_visitedTiles.Any(x => x.Point == walkableTile.Point))
                        continue;

                    //It's already in the active list, but that's OK, maybe this new tile has a better value
                    //(e.g. We might zigzag earlier but this is now straighter). 
                    if (_activeTiles.Any(x => x.Point == walkableTile.Point)) {
                        var existingTile = _activeTiles.First(x => x.Point == walkableTile.Point);
                        if (existingTile.CostDistance > checkTile.CostDistance) {
                            _activeTiles.Remove(existingTile);
                            _activeTiles.Add(walkableTile);
                        }
                    } else {
                        //We've never seen this tile before so add it to the list. 
                        _activeTiles.Add(walkableTile);
                    }
                }
            }

            Console.WriteLine("No Path Found!");
            return null;
        }

        private List<Vector3> GetResult(Tile checkTile) {
            var result = new List<Vector3>();
            var nextTile = checkTile;
            while (nextTile.Parent != null) {
                result.Add(nextTile.Point);
                nextTile = nextTile.Parent;
            } 

            result.Reverse();
            return result;
        }


        private List<Tile> GetWalkableTiles(Tile currentTile, Vector3 targetPos) {
            var result = new List<Tile>();
            var newVector = new Vector3(currentTile.Point.x - 1, currentTile.Point.y, currentTile.Point.z);
            if (_walkablePoints.Contains(newVector)) {
                result.Add(new Tile(newVector,currentTile.Cost+1, currentTile, targetPos));
            }
            newVector = new Vector3(currentTile.Point.x + 1, currentTile.Point.y, currentTile.Point.z);
            if (_walkablePoints.Contains(newVector)) {
                result.Add(new Tile(newVector, currentTile.Cost + 1, currentTile, targetPos));
            }
            newVector = new Vector3(currentTile.Point.x, currentTile.Point.y - 1, currentTile.Point.z);
            if (_walkablePoints.Contains(newVector)) {
                result.Add(new Tile(newVector, currentTile.Cost + 1, currentTile, targetPos));
            }
            newVector = new Vector3(currentTile.Point.x, currentTile.Point.y + 1, currentTile.Point.z);
            if (_walkablePoints.Contains(newVector)) {
                result.Add(new Tile(newVector, currentTile.Cost + 1, currentTile, targetPos));
            }

            return result;
        }
    }

    class Tile {
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

        public void SetDistance(Vector3 target) {
            Distance = Math.Abs(target.x - Point.x) + Math.Abs(target.y - Point.y);
        }

    }
}
