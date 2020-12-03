using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBossScript : MonoBehaviour
{
    public GameManager gm;
    public GameObject bullet; // heat seeking bullet prefab
    public GameObject minion; // turrert enemy
    public float health = 100;

    public Animator anim;

    public AudioSource shootSFX;
    public AudioSource throwSFX;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FireDelay());
        StartCoroutine(MoveAndSpawn());
    }

    //Take damage
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Bullet")
        {
            Destroy(other.gameObject);
            health -= .5f;
            gm.bossHealth -= .5f;
            gm.Score(10);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += .1f;
        }
        if (other.gameObject.tag == "BigBullet")
        {
            Destroy(other.gameObject);
            health -= 6f;
            gm.bossHealth -= 6f;
            gm.Score(100);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += 3f;
        }
    }

    // spawn two minions nearby
    void SpawnServant() 
    {
        throwSFX.time = 0;
        throwSFX.Play();

        GameObject m1 = Instantiate(minion, transform.position + (12 * transform.right), Quaternion.identity);
        GameObject m2 = Instantiate(minion, transform.position - (12 * transform.right), Quaternion.identity);

        m1.transform.rotation = Quaternion.Euler(90, 0, 0);
        m1.GetComponent<Rigidbody>().velocity = transform.right * 12;
        m2.transform.rotation = Quaternion.Euler(90, 0, 0);
        m2.GetComponent<Rigidbody>().velocity = -transform.right * 12;
    }

    // fire heat seeking bullet
    void FireBullet()
    {
        shootSFX.time = 0;
        shootSFX.Play();

        GameObject p = Instantiate(bullet, new Vector3(0, 0, 10), Quaternion.identity);
        p.transform.rotation = Quaternion.Euler(90, 0, 0);
        p.GetComponent<Rigidbody>().velocity = -transform.up * 16;
        p.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) + p.GetComponent<Rigidbody>().velocity;
    }


    // Alternate between moving and stopping to spawn servants
    IEnumerator MoveAndSpawn()
    {
        while (!gm.started)
            yield return null;

        bool spawnTurn = false;
        for (; ; )
        {

            if (spawnTurn)
            {
                anim.SetBool("isSpawning", true);
                yield return new WaitForSeconds(1.5f);
                SpawnServant();
                anim.SetBool("isSpawning", false);
            }
            
            yield return new WaitForSeconds(2f);

            anim.SetBool("isDashing", true);
            for(int i = 0; i < 80; i++)
            {
                transform.RotateAround(new Vector3(0, 0, 0), transform.forward, -1);
                yield return new WaitForSeconds(.01f);
            }
            anim.SetBool("isDashing", false);

            spawnTurn = !spawnTurn;
        }
    }

    // Delay in between seeking bullet shots
    IEnumerator FireDelay()
    {
        while (!gm.started)
            yield return null;

        yield return new WaitForSeconds(2);
        for (; ; )
        {
            FireBullet();
            yield return new WaitForSeconds(.1f);
            FireBullet();
            yield return new WaitForSeconds(.1f);
            FireBullet();
            yield return new WaitForSeconds(2.75f);
        }
    }
}
