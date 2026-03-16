using Remedy.Framework;
//using SaintsField;
using UnityEngine;

namespace Remedy.Cameras
{
    [ExecuteAlways]
    public class CameraManager : Singleton<CameraManager>
    {
        //[Dropdown("GetOperators")]
        public static CameraOperator CurrentOperator;
        public Transform _cameraTransform;

        private Camera _camera;
        public static Camera Camera => Instance._camera ??= CameraTransform.GetCachedComponent<Camera>();
        public static Transform CameraTransform => Instance._cameraTransform ?? null;

        private CameraOperator[] _operators;

        private float _currentTransitionTime = 0f;
        private float _referenceCamTransitionTime = 0f;
        private float _referenceCamTransitionDuration = 0f;
        private bool _wasUsingReferenceCam = false;
/*
        public DropdownList<CameraOperator> GetOperators()
        {
            var list = new DropdownList<CameraOperator>();
            var ops = FindObjectsByType<CameraOperator>(FindObjectsSortMode.None);

            foreach (var item in ops)
            {
                list.Add(item.name + "_" + item.GetType().Name, item);
            }

            return list;
        }*/

        void Start()
        {
            _operators = FindObjectsByType<CameraOperator>(FindObjectsSortMode.None);

            if(Camera != null)
                CameraSubsystem.SetMainCamera(Camera);
        }

        private void Update()
        {
            if (CurrentOperator == null) return;

            if (_currentTransitionTime < CurrentOperator.Properties.TransitionTime)
            {
                _currentTransitionTime += Time.deltaTime;

                _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, CurrentOperator.Camera.position, _currentTransitionTime / CurrentOperator.Properties.TransitionTime);
                _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, CurrentOperator.Camera.rotation, _currentTransitionTime / CurrentOperator.Properties.TransitionTime);
            }
            else
            {
                var currentVolume = CurrentOperator.CurrentVolume;
                if (CurrentOperator.UseVolumes && currentVolume != null && currentVolume.UseReferenceCam && currentVolume.ReferenceCamera != null)
                {
                    var refEuler = currentVolume.ReferenceCamera.eulerAngles;
                    var refPos = currentVolume.ReferenceCamera.position;

                    var goalEuler = refEuler * currentVolume.ReferenceCamAngleMask + _cameraTransform.eulerAngles * !currentVolume.ReferenceCamAngleMask;
                    var goalPos = refPos * currentVolume.ReferenceCamPositionMask + _cameraTransform.position * !currentVolume.ReferenceCamPositionMask;

                    _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, Quaternion.Euler(goalEuler), currentVolume.ReferenceCameraSpeed * Time.deltaTime);
                    _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, goalPos, currentVolume.ReferenceCameraSpeed * Time.deltaTime);

                    CurrentOperator.ForceAxis(_cameraTransform.eulerAngles.y, 0);

                    _wasUsingReferenceCam = true;
                    _referenceCamTransitionTime = 0f;
                    _referenceCamTransitionDuration = currentVolume.ReferenceCameraSpeed;
                }
                else
                {
                    if (_wasUsingReferenceCam)
                    {
                        if (_referenceCamTransitionTime < _referenceCamTransitionDuration)
                        {
                            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, CurrentOperator.Camera.position, _referenceCamTransitionTime / _referenceCamTransitionDuration);
                            _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, CurrentOperator.Camera.rotation, _referenceCamTransitionTime / _referenceCamTransitionDuration);
                            _referenceCamTransitionTime += Time.deltaTime;
                        }
                        else
                        {
                            _wasUsingReferenceCam = false;
                        }
                    }
                    else
                    {
                        _cameraTransform.position = CurrentOperator.Camera.position;
                        _cameraTransform.rotation = CurrentOperator.Camera.rotation;
                    }
                }
            }


            foreach (var op in _operators)
            {
                if (op == CurrentOperator) continue;

                op.Root.position = CurrentOperator.Root.position;
                op.Root.rotation = CurrentOperator.Root.rotation;
                op.ForceAxis(CurrentOperator._actualXAxis, CurrentOperator._actualYAxis);
            }

        }

        public static void SetCameraOperator(CameraOperator op)
        {
            if (Instance._cameraTransform == null) return;

            if (CurrentOperator == null)
            {
                Instance._cameraTransform.position = op.Camera.position;
                Instance._cameraTransform.rotation = op.Camera.rotation;
            }

            CurrentOperator = op;
            Instance._currentTransitionTime = 0f;
        }
    }
}