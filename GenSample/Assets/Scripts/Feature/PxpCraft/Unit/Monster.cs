using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Monster : MonoBehaviour
    {        
        private MonsterAttackCollision atkColl;
        private Transform trImage;
        private SkeletonMecanim skelMecanim;
        private Animator skelAnim;
        private CollisionEventListener collEventListener;
        private MonsterSearchCollision collSearch;
        private Rigidbody2D rBody;
        private BoxCollider2D collBody;

        public float speed = 3f;
        public float knockbackPower = 100f;
        public float jumpPower = 100f;
        public float jumpDelay = 1f;
        private float curJumpDelay;

        private Transform effectContainer;
        private GameObject hitEffect;
        private GameObject hitEffect2;
        private GameObject critHitEffect;

        public LayerMask groundLayer;
        private bool isGround;

        private bool activeJump;
        private bool canJump;

        // Use this for initialization
        void Start()
        {
            skelMecanim = GetComponentInChildren<SkeletonMecanim>();
            skelAnim = skelMecanim.GetComponent<Animator>();
            atkColl = GetComponentInChildren<MonsterAttackCollision>();
            trImage = skelMecanim.transform;
            effectContainer = transform.Find("Effect");
            collEventListener = GetComponentInChildren<CollisionEventListener>();
            collSearch = GetComponentInChildren<MonsterSearchCollision>();
            rBody = GetComponent<Rigidbody2D>();
            collBody = transform.Find("Collider/Body").GetComponent<BoxCollider2D>();

            collEventListener.RegisterListner("AttackBy", AttackBy);

            atkColl.SetParent(this);

            isGround = true;
            curJumpDelay = jumpDelay;
            activeJump = false;
            canJump = false;
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            CheckGround();
            CheckCanJump();
        }

        private void FixedUpdate()
        {
            if (activeJump && canJump)
            {
                activeJump = false;
                Jump();
            }
        }

        private void Move()
        {
            Transform target = collSearch.GetTarget();
            Vector3 dir = Vector3.zero;

            if (target == null)
            {
                // TODO : AI
            }
            else
            {
                Vector3 dist = target.position - transform.position;
                if(dist.magnitude >= .5f)
                {
                    dir = dist.normalized;

                    transform.Translate(Vector3.right * dir.x * speed * Time.deltaTime);

                    if(dist.y >= .5f)
                    {
                        activeJump = true;
                    }
                }
            }

            skelAnim.SetBool("isMove", dir != Vector3.zero);

            Vector3 trScale = trImage.localScale;
            if (dir.x > 0f)
            {
                trScale.x = -Mathf.Abs(trScale.x);
            }
            else if (dir.x < 0f)
            {
                trScale.x = Mathf.Abs(trScale.x);
            }
            trImage.localScale = trScale;
        }

        public void PlayAttackAnim()
        {
            skelAnim.SetTrigger("isAtk");
        }

        public void AttackBy(params object[] param)
        {            
            Player player = (Player)param[0];

            MakeHitEffect();
        }

        public void MakeHitEffect()
        {
            string effPath = $"Prefab/Effect/";

            bool isCrit = Random.Range(0f, 1f) >= .7f;
            if (isCrit)
            {
                effPath += "damage_critical";

                if (critHitEffect == null)
                {
                    GameObject pfCritHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                    critHitEffect = Instantiate(pfCritHitEff, effectContainer);
                }

                critHitEffect.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                critHitEffect.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                if (Random.Range(0f, 1f) > .5f)
                {
                    effPath += "damage_001";

                    if (hitEffect == null)
                    {
                        GameObject pfHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                        hitEffect = Instantiate(pfHitEff, effectContainer);
                    }

                    hitEffect.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                    hitEffect.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    effPath += "damage_002";

                    if (hitEffect2 == null)
                    {
                        GameObject pfHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                        hitEffect2 = Instantiate(pfHitEff, effectContainer);
                    }

                    hitEffect2.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                    hitEffect2.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        private void Jump()
        {
            if (!isGround)
                return;

            rBody.AddForce(Vector3.up * jumpPower);
            isGround = false;
            canJump = false;

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
            float rayDist = .3f;
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

        private void CheckCanJump()
        {
            if (!isGround)
                return;

            if (curJumpDelay >= jumpDelay)
            {
                curJumpDelay = 0f;
                canJump = true;
            }
            else
            {
                curJumpDelay += Time.deltaTime;
            }
        }
    }
}