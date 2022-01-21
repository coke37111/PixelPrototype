using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Player : MonoBehaviour
    {
        private Transform trSpine;
        private Animator skelAnim;
        private Rigidbody2D rBody;
        private Transform effectContainerL;
        private Transform effectContainerR;
        private CollisionEventListener collEventListener;
        private SkeletonMecanim skelMecanim;
        private BoxCollider2D collBody;
        private PlayerAttackCollision atkColl;

        private GameObject effL;
        private GameObject effR;
        private bool isLeft;

        public float jumpPower = 100f;
        public float speed = 5f;
        public float knockbackPower = 100f;
        public float atk = 5f;

        private bool isAttacked;

        public float attackedDelay = 3f;
        private float curAttackedDelay;

        public LayerMask groundLayer;
        private bool isGround;

        private bool canJump;

        // Use this for initialization
        void Start()
        {
            skelMecanim = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelMecanim.transform;
            skelAnim = skelMecanim.GetComponent<Animator>();
            rBody = GetComponent<Rigidbody2D>();
            effectContainerL = transform.Find("Effect/L");
            effectContainerR = transform.Find("Effect/R");
            collEventListener = GetComponentInChildren<CollisionEventListener>();
            collBody = transform.Find("Collider/Body").GetComponent<BoxCollider2D>();
            atkColl = GetComponentInChildren<PlayerAttackCollision>();

            collEventListener.RegisterListner("AttackBy", AttackBy);
            atkColl.SetParent(this);

            isLeft = trSpine.localScale.x < 0f;
            isAttacked = false;
            isGround = true;

            canJump = false;
        }

        // Update is called once per frame
        void Update()
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                canJump = true;
            }
            CheckGround();

            Attack();

            if (isAttacked)
            {
                if (curAttackedDelay >= attackedDelay)
                {
                    curAttackedDelay -= attackedDelay;
                    isAttacked = false;

                    foreach (Slot slot in skelMecanim.skeleton.Slots)
                    {
                        slot.A = 1f;
                    }
                }
                else
                {
                    curAttackedDelay += Time.deltaTime;
                }
            }
        }

        private void FixedUpdate()
        {
            if (canJump)
            {
                canJump = false;
                Jump();
            }
        }

        private void Move()
        {
            float axisX = Input.GetAxis("Horizontal");

            transform.Translate(Vector3.right * axisX * speed * Time.deltaTime);

            skelAnim.SetBool("isMove", axisX != 0f);

            Vector3 trScale = trSpine.localScale;
            if(axisX > 0f)
            {
                trScale.x = -Mathf.Abs(trScale.x);
                isLeft = false;
            }else if(axisX < 0f)
            {
                trScale.x = Mathf.Abs(trScale.x);
                isLeft = true;
            }
            trSpine.localScale = trScale;
        }

        private void Jump()
        {
            if (!isGround)
                return;

            rBody.AddForce(Vector3.up * jumpPower);
            isGround = false;

            skelAnim.SetTrigger("isJump");
            skelAnim.SetBool("isGround", isGround);
        }

        private void CheckGround()
        {
            if (isGround)
                return;

            Vector3 rayOrg = transform.position +
                new Vector3(collBody.offset.x, collBody.offset.y, 0f);
            Vector3 rayDir = Vector3.down;
            float rayDist = .45f;
            Debug.DrawRay(rayOrg, rayDir * rayDist, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(rayOrg, rayDir, rayDist, groundLayer);
            if (hit)
            {
                if (rBody.velocity.y <= 0)
                {
                    isGround = true;
                    skelAnim.SetBool("isGround", isGround);
                }
            }
        }

        private void Attack()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                skelAnim.SetTrigger("isAtk");

                if (isLeft)
                {
                    if(effL == null)
                    {
                        GameObject pfAtkEff =
                       ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_L");
                        effL = Instantiate(pfAtkEff, effectContainerL);
                    }
                    effL.transform.localPosition = Vector3.zero;
                    effL.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    if(effR == null)
                    {
                        GameObject pfAtkEff =
                          ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_R");
                        effR = Instantiate(pfAtkEff, effectContainerR);
                    }
                    effR.transform.localPosition = Vector3.zero;
                    effR.GetComponent<ParticleSystem>().Play();
                }

                atkColl.Raise();
            }
        }

        public void AttackBy(params object[] param)
        {
            if (isAttacked)
                return;
            isAttacked = true;

            Monster monster = (Monster)param[0];

            rBody.velocity = Vector3.zero;
            bool isLeftAttacked = monster.transform.position.x < transform.position.x;
            Vector3 knockbackDir = isLeftAttacked ? new Vector3(1, 1, 0) : new Vector3(-1, 1, 0);
            rBody.AddForce(knockbackDir * monster.knockbackPower);
            skelAnim.SetTrigger("isKnockback");

            StartCoroutine(BlinkPlayer());
        }

        private IEnumerator BlinkPlayer()
        {
            float slotA;
            float time = 0f;
            bool isDown = true;

            while (isAttacked)
            {
                time += Time.deltaTime * 5f;

                if (isDown)
                {
                    slotA = Mathf.Lerp(1f, 0.5f, time);
                    if (slotA <= 0.5f)
                    {
                        isDown = false;
                        time = 0f;
                    }
                }
                else
                {
                    slotA = Mathf.Lerp(0.5f, 1f, time);
                    if(slotA >= 1f)
                    {
                        isDown = true;
                        time = 0f;
                    }
                }

                foreach (Slot slot in skelMecanim.skeleton.Slots)
                {
                    slot.A = slotA;
                }

                yield return null;
            }
        }
    }
}