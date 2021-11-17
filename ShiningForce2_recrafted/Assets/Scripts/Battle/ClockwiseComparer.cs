using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

namespace Assets.Scripts.Battle {   
    public class ClockwiseComparer : IComparer<Vector3> {
        private Vector2 m_Origin;
        private DirectionType _direction;

        public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }


        public ClockwiseComparer(Vector3 origin, DirectionType direction) {
            m_Origin = origin;
            _direction = direction;
        }



        public int Compare(Vector3 first, Vector3 second) {
            return IsClockwise(first, second, m_Origin, _direction);
        }

        // Returns 1 if first comes before second in clockwise order.
        // Returns -1 if second comes before first.
        // Returns 0 if the points are identical.
        public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin, DirectionType direction) {
            if (first == second)
                return 0;

            Vector3 firstOffset = origin - first;
            Vector3 secondOffset = origin - second;

            firstOffset.x *= -1;
            firstOffset.y *= -1;
            secondOffset.x *= -1;
            secondOffset.y *= -1;

            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);

            if (direction == DirectionType.down) {
                if (angle1 < angle2)
                    return -1;

                if (angle1 > angle2)
                    return 1;
            }

            if(direction == DirectionType.right) {
                var new1 = angle1 >= Mathf.PI / 2 ? angle1 + Mathf.PI / 2 - (2 * Mathf.PI) : angle1 + Mathf.PI / 2;
                var new2 = angle2 >= Mathf.PI / 2 ? angle2 + Mathf.PI / 2 - (2 * Mathf.PI) : angle2 + Mathf.PI / 2;
                if (new1 < new2)
                    return -1;

                if (new1 > new2)
                    return 1;
            }

            if (direction == DirectionType.up) {
                var new1 = angle1 >= 0 ? angle1 - Mathf.PI : angle1 + Mathf.PI;
                var new2 = angle2 >= 0 ? angle2 - Mathf.PI : angle2 + Mathf.PI;
                if (new1 < new2)
                    return -1;

                if (new1 > new2)
                    return 1;

                if (angle1 < angle2)
                    return -1;

                if (angle1 > angle2)
                    return 1;
            }

            if (direction == DirectionType.left) {
                var new1 = angle1 >= -Mathf.PI / 2 ? angle1 - Mathf.PI / 2  : angle1 - Mathf.PI / 2 + (2 * Mathf.PI);
                var new2 = angle2 >= -Mathf.PI / 2 ? angle2 - Mathf.PI / 2 : angle2 - Mathf.PI / 2 + (2 * Mathf.PI);
                if (new1 < new2)
                    return -1;

                if (new1 > new2)
                    return 1;
            }


            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
        }
    }
}