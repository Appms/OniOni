using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

    [Range(-1,1)]
    public float Speed = 1;
    [Range(-1, 1)]
    public float DirX = 0;
    public float Life = 5;
    [Range(0, 1)]
    public float Happiness = 1;
    public bool Dead = false;

    private Animator anim;
    private Material mat;
    private SkinnedMeshRenderer skinnedMesh;

    //private ParticleSystem part;
    //private ParticleSystem footPart;
    //public GameObject foot;

	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();
        skinnedMesh = transform.FindChild("Onion").GetComponent<SkinnedMeshRenderer>();
        mat = skinnedMesh.material;
        //part = GameObject.Find("Onion").GetComponent<ParticleSystem>();
        //footPart = foot.GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!Dead) {
            string animation = "none";

            bool leftClick = Input.GetKeyDown(KeyCode.Mouse0);
            bool rightClick = Input.GetKeyDown(KeyCode.Mouse1);

            AnimatorClipInfo[] anims = anim.GetCurrentAnimatorClipInfo(1);
            if (anims.Length > 0) animation = anims[0].clip.name;



            if (leftClick && animation == "none")
            {
                anim.Play("Attack", 1, 0);
                skinnedMesh.SetBlendShapeWeight(1, 100);
            }

            if (rightClick)
            {
                anim.Play(Mathf.PerlinNoise(Time.time, Time.time) >= 0.5f ? "Hurt01" : "Hurt02", 1, 0);
                Life--;
                Happiness -= 0.2f;
                anim.SetFloat("Happiness", Happiness);
                skinnedMesh.SetBlendShapeWeight(2, 100);
            }

            if (Life == 0)
            {
                Dead = true;
                //anim.Play("Death", 0);
                anim.speed = 0.2f;
                anim.Play("Death", 1);
            }

            anim.SetFloat("Speed", Speed);
            anim.SetFloat("DirX", DirX);
            anim.SetFloat("Happiness", Happiness);
            skinnedMesh.SetBlendShapeWeight(0, 100 - Happiness*100);
        }

        else
        {
            anim.SetTrigger("Death");
            anim.SetLayerWeight(1, 0);
            mat.SetFloat("_DissolveFactor", Mathf.Lerp(mat.GetFloat("_DissolveFactor"), 1, Time.deltaTime * 2));
        }
	}

    public void Step(int strength)
    {
        //footPart.Emit(5 * strength);
    }
}
