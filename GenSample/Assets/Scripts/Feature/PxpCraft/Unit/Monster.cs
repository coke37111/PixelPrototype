using Assets.Scripts.Managers;
using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Monster : MonoBehaviour
    {        
        private Transform target;
        private MonsterAttackCollision atkColl;
        private Transform trImage;
        private SkeletonMecanim skelMecanim;
        private Animator skelAnim;
        private CollisionEventListener collEventListener;

        public float speed = 3f;
        public float knockbackPower = 100f;

        private Transform effectContainer;
        private GameObject hitEffect;
        private GameObject hitEffect2;
        private GameObject critHitEffect;

        // Use this for initialization
        void Start()
        {
            skelMecanim = GetComponentInChildren<SkeletonMecanim>();
            skelAnim = skelMecanim.GetComponent<Animator>();
            atkColl = GetComponentInChildren<MonsterAttackCollision>();
            trImage = skelMecanim.transform;
            effectContainer = transform.Find("Effect");
            collEventListener = GetComponentInChildren<CollisionEventListener>();

            collEventListener.RegisterListner("AttackBy", AttackBy);

            atkColl.SetParent(this);
        }

        // Update is called once per frame
        void Update()
        {
            Move();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                target = collision.transform;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if(target == null)
            {
                if (collision.tag == "Player")
                {
                    target = collision.transform;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.tag == "Player")
            {
                target = null;
            }
        }

        private void Move()
        {
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
    }
}