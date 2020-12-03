using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YellowBossScript : MonoBehaviour
{
    public GameManager gm;
    public GameObject bullet;
    public GameObject minion;
    public float health = 100;

    Vector3[] moveLocations;
    Vector3[] turretPositions;

    //reset mechanics
    List<Vector3> bulletVelocities;
    List<Vector3> bulletLocations;
    List<int> bulletBounces;
    Vector3 resetLocation;
    Vector3 playerReset;
    public GameObject bigHand;
    public GameObject littleHand;
    Quaternion bigHandRotation;
    Quaternion littleHandRotation;
    List<Vector3> minionLocations;
    float bossHPReset;

    public Animator anim;
    public Image flash;

    public AudioSource timeSFX;
    public AudioSource throwSFX;

    // Start is called before the first frame update
    void Start()
    {
        Random.seed = System.Environment.TickCount;

        minionLocations = new List<Vector3>();
        moveLocations = new Vector3[] { new Vector3(0, 9, 100), new Vector3(0, 9, -100), new Vector3(100, 9, 0), new Vector3(-100, 9, 0)};
        turretPositions = new Vector3[] { new Vector3(0, 8, 100), new Vector3(0, 8, -100), new Vector3(100, 8, 0), new Vector3(-100, 8, 0),
            new Vector3(50, 8, 90), new Vector3(-50, 8, 90), new Vector3(50, 8, -90), new Vector3(-50, 8, -90),
            new Vector3(90, 8, 50), new Vector3(-90, 8, 50), new Vector3(90, 8, -50), new Vector3(-90, 8, -50)};
        StartCoroutine(Move());
    }

    //Take damage
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Bullet")
        {
            // Debug.Log("Hit - oof");
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

    void SpawnMinion(Vector3 position, bool reset)
    {
        throwSFX.time = 0;
        throwSFX.Play();

        anim.Play("Throw");

        if (!minionLocations.Contains(position) || reset)
        {
            if(!minionLocations.Contains(position))
                minionLocations.Add(position);
            GameObject m = Instantiate(minion, transform.position, Quaternion.identity);
            m.GetComponent<YellowMinion>().targetLocation = position;
        }
        else
        {
            GameObject m = Instantiate(minion, position, Quaternion.identity);
            m.GetComponent<YellowMinion>().targetLocation = position;
        }
    }

    //Get information needed to reset time
    void TimeResetSetup()
    {
        bulletLocations = new List<Vector3>();
        bulletVelocities = new List<Vector3>();
        bulletBounces = new List<int>();

        //copy player bullet information
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject b in bullets)
        {
            if (b.GetComponent<SplitBullet>() != null)
            {
                bulletLocations.Add(b.transform.position);
                bulletVelocities.Add(b.GetComponent<Rigidbody>().velocity);
                bulletBounces.Add(b.GetComponent<SplitBullet>().splitsRemaining);
            }
        }

        resetLocation = transform.position;
        bossHPReset = health;
        playerReset = gm.player.transform.position;

        bigHandRotation = bigHand.transform.rotation;
        littleHandRotation = littleHand.transform.rotation;
        
    }

    //Move in a diamond pattern
    IEnumerator Move()
    {
        while (!gm.started)
            yield return null;

        for (int i = 0; i < 3; i++)
        {
            if (i == 2)
            {
                TimeResetSetup();
                int index = -1;
                while(minionLocations.Count < turretPositions.Length)
                {
                    index = Random.Range(0, 12);
                    Debug.Log(index);
                    if (!minionLocations.Contains(turretPositions[index]))
                        break;
                }
                if(index != -1)
                    SpawnMinion(turretPositions[index], false);
            }

            yield return new WaitForSeconds(5);

            //select location to move to
            int moveIndex = 0;
            for (; ; )
            {
                moveIndex = Random.Range(0, 4);
                if (moveLocations[moveIndex] != transform.position)
                    break;
            }

            //quick dash then teleport to location
            if(i != 2)
            {
                anim.Play("Dash");
            }
            else
            {
                anim.Play("Smash");
            }
            for (int j = 0; j < 30; j++)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveLocations[moveIndex], 1.5f);
                yield return new WaitForSeconds(.01f);
            }

            transform.position = moveLocations[moveIndex];
        }

        StartCoroutine(ResetTime());
    }

    IEnumerator ResetTime()
    {
        timeSFX.time = 0;
        timeSFX.Play();

        //TODO activate flash of light
        Color temp = Color.white;
        temp.a = 0;
        for(int i = 0; i < 5; i++)
        {
            temp.a += .2f;
            flash.color = temp;
            yield return new WaitForSeconds(.01f);
        }

        //delete everything
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");

        foreach(GameObject b in bullets)
        {
            Destroy(b);
        }
        foreach (GameObject m in minions)
        {
            Debug.Log("destroy");
            Destroy(m);
        }

        //reset boss
        transform.position = resetLocation;
        health = bossHPReset;
        gm.bossHealth = health;

        //player reset
        gm.player.transform.position = playerReset;

        bigHand.transform.rotation = bigHandRotation;
        littleHand.transform.rotation = littleHandRotation;

        //bullet reset
        for(int i = 0; i < bulletLocations.Count; i++)
        {
            GameObject b = Instantiate(bullet, bulletLocations[i], Quaternion.Euler(90,0,0));
            b.GetComponent<Rigidbody>().velocity = bulletVelocities[i];
            b.GetComponent<SplitBullet>().splitsRemaining = bulletBounces[i];
        }

        for(int i = 0; i < minionLocations.Count; i++)
        {
            if (i == minionLocations.Count - 1)
                SpawnMinion(minionLocations[i], true);
            else
                SpawnMinion(minionLocations[i], false);
        }

        for (int i = 0; i < 5; i++)
        {
            temp.a -= .2f;
            flash.color = temp;
            yield return new WaitForSeconds(.01f);
        }

        StartCoroutine(Move());
    }

}
