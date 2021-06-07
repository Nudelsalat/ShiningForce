using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.GameData.Characters;
using UnityEngine;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Battle.AI {
    public class Pathfinder {
        private readonly List<Tile> _activeTiles = new List<Tile>();
        private readonly List<Tile> _visitedTiles = new List<Tile>();
        private readonly List<Vector3> _walkablePoints;

        private readonly Vector3 _startPoint;
        private readonly Vector3 _targetPoint;
        private readonly LayerMask _targetLayerMask;

        private readonly bool _calculateMovementCost;
        private readonly bool _searchForLayerMask;

        private readonly MovementGrid _movementGrid;
        private readonly EnumMovementType _movementType;
        //only to randomize the movement of flying units a bit... otherwise you could "kite" those units that only move 2 steps
        private int _increment = UnityEngine.Random.Range(0, 4);

        private readonly Vector3[] _vector3Directions = new Vector3[4]
        {
            new Vector3(0, 1),
            new Vector3(1, 0),
            new Vector3(0, -1),
            new Vector3(-1, 0)
        };

        public Pathfinder(List<Vector3> walkablePoints, Vector3 startPoint, Vector3 targetPoint) {
            _walkablePoints = walkablePoints;
            _startPoint = startPoint;
            _targetPoint = targetPoint;
            _calculateMovementCost = false;
            _searchForLayerMask = false;
        }

        public Pathfinder(List<Vector3> walkablePoints, Vector3 startPoint, Vector3 targetPoint, 
            MovementGrid movementGrid, EnumMovementType movementType) {
            _walkablePoints = walkablePoints;
            _startPoint = startPoint;
            _targetPoint = targetPoint;
            _movementGrid = movementGrid;
            _movementType = movementType;
            _calculateMovementCost = true;
            _searchForLayerMask = false;
        }

        public Pathfinder(List<Vector3> walkablePoints, Vector3 startPoint, LayerMask targetLayerMask,
            MovementGrid movementGrid, EnumMovementType movementType) {
            _walkablePoints = walkablePoints;
            _startPoint = startPoint;
            _targetLayerMask = targetLayerMask;
            _movementGrid = movementGrid;
            _movementType = movementType;
            _calculateMovementCost = true;
            _searchForLayerMask = true;
        }

        public List<Vector3> GetShortestPath() {
            if (_walkablePoints == null) {
                var result = new List<Vector3>();
                result.Add(new Vector3(_targetPoint.x, _startPoint.y, _targetPoint.z));
                result.Add(new Vector3(_targetPoint.x, _targetPoint.y, _targetPoint.z));
                return result;
            }

            var startTile = _searchForLayerMask ? new Tile(_startPoint, 0, null) 
                                                : new Tile(_startPoint, 0, null, _targetPoint);
            _activeTiles.Add(startTile);

            while (_activeTiles.Any()) {
                var checkTile = _activeTiles.OrderBy(x => x.CostDistance).First();

                if(CheckIfFinished(checkTile)) {
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

        private bool CheckIfFinished(Tile checkTile) {
            if (_searchForLayerMask) {
                if (_movementGrid.IsOccupiedByOpponent(checkTile.Point.x, checkTile.Point.y, _targetLayerMask)) {
                    Debug.Log("We are at the destination!");
                    //We can actually loop through the parents of each tile to find our exact path which we will show shortly. 
                    return true;
                }
                else {
                    return false;
                }
            } else if (checkTile.Point == _targetPoint) {
                Debug.Log("We are at the destination!");
                //We can actually loop through the parents of each tile to find our exact path which we will show shortly. 
                return true;
            }
            return false;
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
            _increment++;
            for (int i = 0; i < 4; i++) {
                var newVector = currentTile.Point + _vector3Directions[(i+_increment) %4];
                if (_walkablePoints.Contains(newVector)) {
                    var cost = 1f;
                    if (_calculateMovementCost) {
                        cost = _movementGrid.GetMovementCost(newVector.x, newVector.y, _movementType);
                    }
                    result.Add(_searchForLayerMask
                        ? new Tile(newVector, currentTile.Cost + cost, currentTile)
                        : new Tile(newVector, currentTile.Cost + cost, currentTile, targetPos));
                }
            }
            return result;
        }
    }
}
