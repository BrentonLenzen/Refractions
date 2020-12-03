using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndigoBossScript : MonoBehaviour
{
    public GameManager gm;
    public GameObject basicBullet; // normal bullet
    public GameObject bounceBullet; // bullet that bounces during rage
    public float health = 105;

    RaycastHit hit; //detect walls and player

    int moveDirect; // 1 or -1 depending on direction - applies to raycast and 
    public bool rage; // is the other boss dead?

    public Animator anim;

    public AudioSource shootSFX;
    public AudioSource slamSFX;

    // Start is called before the first frame update
    void Start()
    {
        moveDirect = 1;
        //rage = false;
        StartCoroutine(MoveSideToSide());
        StartCoroutine(FireDelay());
    }

    //Take damage
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Bullet")
        {
            // Debug.Log("Hit - oof");
            Destroy(other.gameObject);
            health -= .5f;
            gm.SecondBossHealth -= .5f;
            gm.Score(10);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += .1f;
        }
        if (other.gameObject.tag == "BigBullet")
        {
            Destroy(other.gameObject);
            health -= 6f;
            gm.SecondBossHealth -= 6f;
            gm.Score(100);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += 3f;
        }

        //check if dead then rage
        if (health <= 0)
        {
            GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
            foreach (GameObject b in bosses)
            {
                if (b != this.gameObject)
                {
                    b.GetComponent<VioletBossScript>().rage = true;
                }
            }
            Destroy(this.gameObject);
        }
    }

    void FireBullet()
    {
        shootSFX.time = 0;
        shootSFX.Play();

        GameObject p;
        if (rage)
        {
            p = Instantiate(bounceBullet, new Vector3(0, 0, 10), Quaternion.identity);
            p.GetComponent<Rigidbody>().velocity = transform.up * 50;
        }
        else
        {
            p = Instantiate(basicBullet, new Vector3(0, 0, 10), Quaternion.identity);
            p.GetComponent<Rigidbody>().velocity = transform.up * 20;
        }
        p.transform.rotation = transform.rotation;
        p.GetComponent<Rigidbody>().velocity = transform.up * 20;
        p.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) + p.GetComponent<Rigidbody>().velocity;
    }

    IEnumerator MoveSideToSide()
    {
        while (!gm.started)
            yield return null;

        for (; ; )
        {
            //change directions if near wall
            if (Physics.Raycast(transform.position, moveDirect * -transform.right, out hit, 10) && hit.collider.tag == "DoubleBounceWall") 
                moveDirect = -moveDirect;

            // detect player for charge
            if (!rage /*&& Physics.Raycast(transform.position, transform.up, out hit, 250f) && hit.collider.tag == "Player"*/)
            {
                bool wallPresent = false;
                bool playerPresent = false;
                Collider[] inFront = Physics.OverlapBox(transform.position + transform.up * 100, new Vector3(2, 2, 100));
                foreach (Collider c in inFront)
                {
                    if (c.tag == "DoubleBounceWall")
                    {
                        wallPresent = true;
                        break;
                    }
                    if(c.tag == "Player")
                    {
                        playerPresent = true;
                    }
                }

                if (!wallPresent && playerPresent)
                    break;
            }
            if(rage)
                transform.Translate(moveDirect * 2 * transform.right);
            else 
                transform.Translate(moveDirect * transform.right);

            yield return new WaitForSeconds(.01f);
        }
        if (!rage)
            StartCoroutine(Charge());
    }

    // charge at player when detected
    IEnumerator Charge()
    {
        anim.Play("Charge");

        //charge to player
        for (; ; )
        {
            //stop near other boss
            if (Physics.Raycast(transform.position, transform.up, out hit, 10f) && (hit.collider.tag == "Boss" || hit.collider.tag == "DoubleBounceWall"))
                break;

            transform.Translate(2 * -transform.forward);

            yield return new WaitForSeconds(.01f);
        }

        anim.Play("ChargeBack");
        slamSFX.time = 0;
        slamSFX.Play();

        yield return new WaitForSeconds(0.75f);

        //move back to wall
        for (; ; )
        {
            //stop near wall
            if (Physics.Raycast(transform.position, -transform.up, out hit, 15f) && hit.collider.tag == "DoubleBounceWall")
                break;

            transform.Translate(transform.forward);

            yield return new WaitForSeconds(.01f);
        }
        StartCoroutine(MoveSideToSide());
    }

    // Delay in between bullet shots
    IEnumerator FireDelay()
    {
        while (!gm.started)
            yield return null;

        for (; ; )
        {
            FireBullet();
            if(rage)
                yield return new WaitForSeconds(.4f);
            else
                yield return new WaitForSeconds(1.2f);
        }
    }

}
