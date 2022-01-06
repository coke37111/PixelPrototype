using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox
{
    public class SandboxCameraController : MonoBehaviour
    {
        [Header("- For Designer")]
        public float rotateSpeed = 10.0f;
        public float zoomSpeed = 10.0f;
        public float moveSpeed = 10.0f;

        [Header("- For Player")]
        public float dist = 10.0f;
        public float height = 5.0f;
        public float smoothRotate = 5.0f;

        private Camera mainCamera;
        private SandboxManager sbManager;
        private Transform target;

        private bool isInitialized = false;

        #region UNITY

        void Start()
        {
            mainCamera = GetComponent<Camera>();
        }

        void Update()
        {
            if (!isInitialized)
                return;

            if(sbManager.GetPlayerType() == SandboxManager.PLAYER_TYPE.Designer)
            {
                Zoom();
                Rotate();
                Move();
                ShowCube();
            }
        }

        private void LateUpdate()
        {
            if (!isInitialized)
                return;

            if(sbManager.GetPlayerType() == SandboxManager.PLAYER_TYPE.Player)
            {
                FollowTarget();
            }
        }

        #endregion

        public void Init(SandboxManager sbManager)
        {
            mainCamera = GetComponent<Camera>();
            this.sbManager = sbManager;

            isInitialized = true;
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private void ShowCube()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.tag == "Cube")
                    sbManager.ShowCube(hit.transform, hit.normal);
            }
        }

        private void Zoom()
        {
            float distance = Input.GetAxis("Mouse ScrollWheel") * -1 * zoomSpeed;
            if (distance != 0)
            {
                mainCamera.fieldOfView += distance;
            }
        }

        private void Rotate()
        {
            if(Input.GetMouseButton(1))
            {
                Vector3 rot = transform.rotation.eulerAngles; // 현재 카메라의 각도를 Vector3로 반환
                rot.y += Input.GetAxis("Mouse X") * rotateSpeed; // 마우스 X 위치 * 회전 스피드
                rot.x += -1 * Input.GetAxis("Mouse Y") * rotateSpeed; // 마우스 Y 위치 * 회전 스피드
                Quaternion q = Quaternion.Euler(rot); // Quaternion으로 변환
                transform.rotation = Quaternion.Slerp(transform.rotation, q, 2f); // 자연스럽게 회전
            }
        }

        private void Move()
        {
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                dir += Vector3.forward;
            }else if(Input.GetKey(KeyCode.S))
            {
                dir += Vector3.back;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dir += Vector3.left;
            }else if (Input.GetKey(KeyCode.D))
            {
                dir += Vector3.right;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                dir += Vector3.up;
            }else if (Input.GetKey(KeyCode.X))
            {
                dir += Vector3.down;
            }

            transform.Translate(dir * moveSpeed * Time.deltaTime);
        }

        private void FollowTarget()
        {
            if (target == null)
            {
                return;
            }

            float curYAngle = Mathf.LerpAngle(transform.eulerAngles.y, target.eulerAngles.y, smoothRotate * Time.deltaTime);

            Quaternion rot = Quaternion.Euler(0, curYAngle, 0);

            transform.position = target.position - (rot * Vector3.forward * dist) + Vector3.up * height;

            transform.LookAt(target);
        }
    }
}