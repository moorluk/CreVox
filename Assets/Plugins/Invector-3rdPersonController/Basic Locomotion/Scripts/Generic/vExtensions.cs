using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Invector
{
    public static class vExtensions
    {
        /// <summary>
        /// Check if Transfom is children
        /// </summary>
        /// <param name="me"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool isChild(this Transform me, Transform target)
        {
            if (!target) return false;
            var objName = target.gameObject.name;
            var obj = me.FindChildByNameRecursive(objName);
            if (obj == null) return false;
            else return obj.Equals(target);
        }

        public static Transform FindChildByNameRecursive(this Transform me, string name)
        {
            if (me.name == name)
                return me;
            for (int i = 0; i < me.childCount; i++)
            {
                var child = me.GetChild(i);
                var found = child.FindChildByNameRecursive(name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static T[]  Append<T>(this T[] arrayInitial, T[] arrayToAppend) 
        {
            if (arrayToAppend == null)
            {
                throw new ArgumentNullException("The appended object cannot be null");
            }
            if ((arrayInitial is string) || (arrayToAppend is string))
            {
                throw new ArgumentException("The argument must be an enumerable");
            }
            T[] ret = new T[arrayInitial.Length + arrayToAppend.Length];
            arrayInitial.CopyTo(ret, 0);
            arrayToAppend.CopyTo(ret, arrayInitial.Length);

            return ret;
        }
        
        /// <summary>
        /// Normalized the angle. between -180 and 180 degrees
        /// </summary>
        /// <param Name="eulerAngle">Euler angle.</param>
        public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
        {
            var delta = eulerAngle;

            if (delta.x > 180) delta.x -= 360;
            else if (delta.x < -180) delta.x += 360;

            if (delta.y > 180) delta.y -= 360;
            else if (delta.y < -180) delta.y += 360;

            if (delta.z > 180) delta.z -= 360;
            else if (delta.z < -180) delta.z += 360;

            return new Vector3(delta.x, delta.y, delta.z);//round values to angle;
        }

        public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
        {
            return otherVector - vector;
        }

        public static void SetActiveChildren(this GameObject gameObjet, bool value)
        {
            foreach (Transform child in gameObjet.transform)
                child.gameObject.SetActive(value);
        }

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            do
            {
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
            } while (angle < -360 || angle > 360);

            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// Lerp between CameraStates
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="time"></param>
        public static void Slerp(this vThirdPersonCameraState to, vThirdPersonCameraState from, float time)
        {
            to.Name = from.Name;
	        to.forward = Mathf.Lerp(to.forward, from.forward, time);
            to.right = Mathf.Lerp(to.right, from.right, time);
            to.defaultDistance = Mathf.Lerp(to.defaultDistance, from.defaultDistance, time);
            to.maxDistance = Mathf.Lerp(to.maxDistance, from.maxDistance, time);
            to.minDistance = Mathf.Lerp(to.minDistance, from.minDistance, time);
            to.height = Mathf.Lerp(to.height, from.height, time);
            to.fixedAngle = Vector2.Lerp(to.fixedAngle, from.fixedAngle, time);            
            to.smoothFollow = Mathf.Lerp(to.smoothFollow, from.smoothFollow, time);
            to.xMouseSensitivity = Mathf.Lerp(to.xMouseSensitivity, from.xMouseSensitivity, time);
            to.yMouseSensitivity = Mathf.Lerp(to.yMouseSensitivity, from.yMouseSensitivity, time);
            to.yMinLimit = Mathf.Lerp(to.yMinLimit, from.yMinLimit, time);
            to.yMaxLimit = Mathf.Lerp(to.yMaxLimit, from.yMaxLimit, time);
            to.xMinLimit = Mathf.Lerp(to.xMinLimit, from.xMinLimit, time);
            to.xMaxLimit = Mathf.Lerp(to.xMaxLimit, from.xMaxLimit, time);
            to.rotationOffSet = Vector3.Lerp(to.rotationOffSet, from.rotationOffSet, time);
            to.cullingHeight = Mathf.Lerp(to.cullingHeight, from.cullingHeight, time);
            to.cullingMinDist = Mathf.Lerp(to.cullingMinDist, from.cullingMinDist, time);
            to.cameraMode = from.cameraMode;
            to.useZoom = from.useZoom;
            to.lookPoints = from.lookPoints;
            to.fov = Mathf.Lerp(to.fov, from.fov, time);
        }

        /// <summary>
        /// Copy of CameraStates
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        public static void CopyState(this vThirdPersonCameraState to, vThirdPersonCameraState from)
        {
            to.Name = from.Name;
            to.forward = from.forward;
            to.right = from.right;
            to.defaultDistance = from.defaultDistance;
            to.maxDistance = from.maxDistance;
            to.minDistance = from.minDistance;
            to.height = from.height;
            to.fixedAngle = from.fixedAngle;
            to.lookPoints = from.lookPoints;            
            to.smoothFollow = from.smoothFollow;
            to.xMouseSensitivity =from.xMouseSensitivity;
            to.yMouseSensitivity =from.yMouseSensitivity;
            to.yMinLimit = from.yMinLimit;
            to.yMaxLimit = from.yMaxLimit;
            to.xMinLimit = from.xMinLimit;
            to.xMaxLimit = from.xMaxLimit;
            to.rotationOffSet = from.rotationOffSet;
            to.cullingHeight = from.cullingHeight;
            to.cullingMinDist = from.cullingMinDist;
            to.cameraMode = from.cameraMode;
            to.useZoom = from.useZoom;
            to.fov = from.fov;
        }

        public static List<T> vToList<T>(this T[] array)
        {
            List<T> list = new List<T>();
            if (array == null || array.Length == 0) return list;
            for (int i = 0; i < array.Length; i++)
            {
                list.Add(array[i]);
            }
            return list;
        }

        public static T[] vToArray<T>(this List<T> list)
        {
            T[] array = new T[list.Count];
            if (list == null || list.Count == 0) return array;
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i];
            }
            return array;
        }

        public static ClipPlanePoints NearClipPlanePoints(this Camera camera, Vector3 pos, float clipPlaneMargin)
        {
            var clipPlanePoints = new ClipPlanePoints();

            var transform = camera.transform;
            var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
            var aspect = camera.aspect;
            var distance = camera.nearClipPlane;
            var height = distance * Mathf.Tan(halfFOV);
            var width = height * aspect;
            height *= 1 + clipPlaneMargin;
            width *= 1 + clipPlaneMargin;
            clipPlanePoints.LowerRight = pos + transform.right * width;
            clipPlanePoints.LowerRight -= transform.up * height;
            clipPlanePoints.LowerRight += transform.forward * distance;

            clipPlanePoints.LowerLeft = pos - transform.right * width;
            clipPlanePoints.LowerLeft -= transform.up * height;
            clipPlanePoints.LowerLeft += transform.forward * distance;

            clipPlanePoints.UpperRight = pos + transform.right * width;
            clipPlanePoints.UpperRight += transform.up * height;
            clipPlanePoints.UpperRight += transform.forward * distance;

            clipPlanePoints.UpperLeft = pos - transform.right * width;
            clipPlanePoints.UpperLeft += transform.up * height;
            clipPlanePoints.UpperLeft += transform.forward * distance;

            return clipPlanePoints;
        }

        public static HitBarPoints GetBoundPoint(this BoxCollider boxCollider,Transform torso, LayerMask mask)
        {
            HitBarPoints bp = new HitBarPoints();
            var boxPoint = boxCollider.GetBoxPoint();
            Ray toTop = new Ray(boxPoint.top, boxPoint.top- torso.position);
            Ray toCenter = new Ray(torso.position, boxPoint.center - torso.position);
            Ray toBottom = new Ray(torso.position, boxPoint.bottom - torso.position);
            Debug.DrawRay(toTop.origin, toTop.direction, Color.red, 2);
            Debug.DrawRay(toCenter.origin, toCenter.direction, Color.green, 2);
            Debug.DrawRay(toBottom.origin, toBottom.direction, Color.blue, 2);
            RaycastHit hit;
            var dist = Vector3.Distance(torso.position, boxPoint.top);
            if (Physics.Raycast(toTop,out hit, dist, mask))
            {
                bp |= HitBarPoints.Top;
                Debug.Log(hit.transform.name);
            }
            dist = Vector3.Distance(torso.position, boxPoint.center);
            if (Physics.Raycast(toCenter, out hit, dist,mask))
            {
                bp |= HitBarPoints.Center;
                Debug.Log(hit.transform.name);
            }
            dist = Vector3.Distance(torso.position, boxPoint.bottom);
            if (Physics.Raycast(toBottom, out hit, dist, mask))
            {
                bp |= HitBarPoints.Bottom;
                Debug.Log(hit.transform.name);
            }

            return bp;
        }

        public static BoxPoint GetBoxPoint(this BoxCollider boxCollider)
        {
            BoxPoint bp = new BoxPoint();
            bp.center =  boxCollider.transform.TransformPoint(boxCollider.center)  ;          
            var height = boxCollider.transform.lossyScale.y * boxCollider.size.y;          
            var ray = new Ray(bp.center, boxCollider.transform.up);
           
            bp.top =    ray.GetPoint((height * 0.5f));
            bp.bottom = ray.GetPoint(-(height * 0.5f));
           
            return bp;
        }

        public static Vector3 BoxSize(this BoxCollider boxCollider)
        {
            var length = boxCollider.transform.lossyScale.x * boxCollider.size.x;
            var width = boxCollider.transform.lossyScale.z * boxCollider.size.z;
            var height = boxCollider.transform.lossyScale.y * boxCollider.size.y;
            return  new Vector3(length, height, width);
        }  

        public static bool Contains(this Enum keys, Enum flag)
        {
            if (keys.GetType() != flag.GetType())
                throw new ArgumentException("Type Mismatch");
            return (Convert.ToUInt64(keys) & Convert.ToUInt64(flag)) != 0;
        }       
    }

    public struct BoxPoint
    {
        public Vector3 top;
        public Vector3 center;
        public Vector3 bottom;
       
    }

    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }

    [Flags]
    public enum HitBarPoints
    {
        None = 0, 
        Top = 1, 
        Center = 2,
        Bottom = 4
    }
}
