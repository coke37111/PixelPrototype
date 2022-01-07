using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox
{
    public class SandboxCameraController : MonoBehaviour
    {
        // TODO : 키보드에 의한 카메라 이동일 시 사용
        private float moveSpeed = 10.0f;

        public float rotSpeed = 100.0f;           // 회전속도
        public float zoomSpeed = 10.0f;

        [SerializeField]
        private GameObject editTarget = null;       // 타겟이 될 게임오브젝트

        [Header("- For Play"), Space(10)]
        public float dist = 10.0f;
        public float height = 5.0f;
        public float smoothRotate = 5.0f;

        private Camera mainCamera;
        private SandboxManager sbManager;
        private Transform playTarget;

        private float rotationX = 0.0f;         // X축 회전값
        private float rotationY = 0.0f;         // Y축 회전값

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

            Zoom();
            if (sbManager.GetPlayerType() == SandboxManager.PLAYER_TYPE.Designer)
            {
                Rotate(editTarget.transform);
                //Move();
                ShowCube();
            }
            else
            {
                // TODO : 유닛의 이동이 카메라의 회전에 따라 변하지 않는다, 수정필요
                //Rotate(target);
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
            this.playTarget = target;
        }

        private void ShowCube()
        {
            sbManager.ActiveShowCube(false);

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.tag == "Cube")
                {
                    sbManager.ShowCube(hit.transform, hit.normal);
                    sbManager.ActiveShowCube(true);
                }
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

        private void Rotate(Transform target)
        {
            //transform.LookAt(Vector3.zero);

            // 마우스가 눌러지면,
            if (Input.GetMouseButton(1))
            {
                // 마우스 변화량을 얻고, 그 값에 델타타임과 속도를 곱해서 회전값 구하기
                rotationX = Input.GetAxis("Mouse X") * Time.deltaTime * rotSpeed;
                rotationY = Input.GetAxis("Mouse Y") * Time.deltaTime * rotSpeed;

                // 각 축으로 회전
                // Y축은 마우스를 내릴때 카메라는 올라가야 하므로 반대로 적용
                transform.RotateAround(target.position, Vector3.right, -rotationY);
                transform.RotateAround(target.position, Vector3.up, rotationX);

                // 회전후 타겟 바라보기
                transform.LookAt(target);
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
            if (playTarget == null)
            {
                return;
            }

            float curYAngle = Mathf.LerpAngle(transform.eulerAngles.y, playTarget.eulerAngles.y, smoothRotate * Time.deltaTime);

            Quaternion rot = Quaternion.Euler(0, curYAngle, 0);

            transform.position = playTarget.position - (rot * Vector3.forward * dist) + Vector3.up * height;

            transform.LookAt(playTarget);
        }

        public void LookTarget()
        {
            if (playTarget == null)
                return;

            transform.LookAt(playTarget);
        }
    }
}