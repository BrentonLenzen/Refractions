using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreenBossScript : MonoBehaviour
{
    public GameManager gm;
    public GameObject bullet;
    public float health = 100;
    float shieldHP;
    public GameObject shieldText;
    public GameObject shield;

    public Animator anim;

    public AudioSource shootSFX;
    public AudioSource specialSFX;

    // Start is called before the first frame update
    void Start()
    {
        shieldHP = 10;
        StartCoroutine(RotateShield());
        StartCoroutine(FireDelay());
        StartCoroutine(RechargeShield());
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bullet" && shieldHP > 0)
        {
            Destroy(other.gameObject);
            shieldHP -= .5f;
        }
        else if (other.gameObject.tag == "BigBullet" && shieldHP > 0)
        {
            Destroy(other.gameObject);
            shieldHP -= 5f;
        }
        else if (other.gameObject.tag == "Bullet")
        {
            // Debug.Log("Hit - oof");
            Destroy(other.gameObject);
            health -= .5f;
            gm.bossHealth -= .5f;
            gm.Score(10);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += .1f;
        }
        else if (other.gameObject.tag == "BigBullet")
        {
            Destroy(other.gameObject);
            health -= 6f;
            gm.bossHealth -= 6f;
            gm.Score(100);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += 3f;
        }

        if (shieldHP <= 0)
            shieldHP = 0;
        shieldText.GetComponent<Text>().text = "SHIELD: " + shieldHP;
    }

    void FireBullet(Vector3 direction)
    {
        shootSFX.time = 0;
        shootSFX.Play();

        GameObject p = Instantiate(bullet, new Vector3(0, 0, 10), Quaternion.identity);
        p.transform.rotation = Quaternion.Euler(90, 0, 0);
        p.GetComponent<Rigidbody>().velocity = direction * 16;
        p.transform.position = transform.position + p.GetComponent<Rigidbody>().velocity;
    }

    IEnumerator FireDelay()
    {
        while (!gm.started)
            yield return null;

        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(5);

            if (i == 0)
                anim.Play("GreenAggro1");
            else
                anim.Play("GreenAggro2");

            //fire split bullets in three directions
            FireBullet(-transform.up + 2*transform.right);
            yield return new WaitForSeconds(.2f);
            FireBullet(-transform.up + transform.right);
            yield return new WaitForSeconds(.2f);
            FireBullet(-transform.up);
            yield return new WaitForSeconds(.2f);
            FireBullet(-transform.up - transform.right);
            yield return new WaitForSeconds(.2f);
            FireBullet(-transform.up - 2*transform.right);
        }

        yield return new WaitForSeconds(5);
        StartCoroutine(RedirectBullets());
    }

    IEnumerator RedirectBullets()
    {
        anim.Play("GreenSpecial");
        specialSFX.time = 0;
        specialSFX.Play();

        //get a list of every bullet in the scene
        GameObject[] allBullets = GameObject.FindGameObjectsWithTag("Bullet");

        //stop all bullets
        foreach(GameObject b in allBullets)
        {
            if (b == null)
                continue;
            Rigidbody rb = b.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(0, 0, 0);
        }

        yield return new WaitForSeconds(2);

        //redirect bullets at player
        foreach(GameObject b in allBullets)
        {
            if (b == null)
                continue;
            Rigidbody rb = b.GetComponent<Rigidbody>();
            rb.velocity = 30 * Vector3.Normalize(gm.player.transform.position - b.transform.position);
            yield return new WaitForSeconds(.00001f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControler>().hypeLevel += .1f;
        }

        anim.Play("GreenIdle");

        StartCoroutine(FireDelay());
    }

    IEnumerator RechargeShield()
    {
        while (!gm.started)
            yield return null;

        Color temp = Color.white;

        for (; ; )
        {
            if (shieldHP < 10)
                shieldHP += .5f;
            if (shieldHP <= 0)
                break;

            temp.a = shieldHP * 0.1f;
            shield.GetComponent<SpriteRenderer>().material.color = temp;
            shieldText.GetComponent<Text>().text = "SHIELD: " + shieldHP;
            yield return new WaitForSeconds(.15f);
        }
        
    }

    IEnumerator RotateShield()
    {
        for (; ; )
        {
            shield.transform.RotateAround(shield.transform.forward, .01f);
            yield return new WaitForSeconds(.01f);
        }
    }

}
