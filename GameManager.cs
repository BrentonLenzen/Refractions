using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject player ;
    public float bossHealth = 700;
    public float SecondBossHealth = 80;
    public int level;// = 0;
    public float maxHealth = 8;
    public float health = 8;
    public Image healthUI;
    public Image bombUI;
    public GameObject curr;
    public bool inDialouge = true;
    public float hypeLevel;
    public GameObject hypeMeterNum;
    public GameObject hypeMeterBase;
    public GameObject hypeMeterMid;
    public GameObject gameOverText;    

    public ScoreManager sm;

    public bool started = false;
    bool gameOver = false;
    int gameOverClicks = 0;

    public AudioSource bgm;
    public AudioClip bossDeath;
    public AudioClip playerDeath;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerControler>().isPaused = true;
        //Time.timeScale =  0;
        float dialouge = Random.Range(0, 4);
        // dialogue system removed for conciseness

        sm = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
        sm.gm = this;
        sm.Load();

        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        while(sm == null && !gameOver)
        {
            sm = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
            sm.gm = this;
            sm.Load();
        }

        if (gameOver && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Shoot") || Input.GetButtonDown("Xbutton")))
        {
            gameOverClicks++;
            if (gameOverClicks >= 2)
            {
                sm.Save();
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }

    }
     void OnGUI()
    {
        hypeLevel = player.GetComponent<PlayerControler>().hypeLevel;

        updateUI();
        if (!gameOver && ((bossHealth <= 0 && player.GetComponent<PlayerControler>().isPaused==false && level != 2) || (level == 2 && bossHealth <= 0 && SecondBossHealth <= 0 && player.GetComponent<PlayerControler>().isPaused == false)))
        {
            player.GetComponent<PlayerControler>().isPaused = true;
            EndBattle();
            curr.SetActive(true);
            inDialouge = true;
            // Dialogue system removed for sample for conciseness
        }
        //Game Over
        if(health <= 0 && !gameOver)
        {
            gameOver = true;
            StartCoroutine(GameOver());
        }

        

    }

    public void nextLevel()
    {
        StartCoroutine(FadeOut());
    }


    //updates UI
    void updateUI()
    {
        Color hype0 = new Color(172, 172, 172);
        Color hype1 = new Color(170, 141, 141);
        Color hype2 = new Color(170, 69, 69);
        Color hype3 = new Color(235, 50, 50);
        if (hypeLevel <= 3)
        {
            hypeMeterBase.GetComponent<Image>().color = hype0;
            hypeMeterMid.GetComponent<Image>().color = hype1;
            hypeMeterNum.GetComponent<Text>().text = "0";
            hypeMeterMid.GetComponent<Image>().fillAmount = hypeLevel / 3.0f;
        }
        else if (hypeLevel <= 10)
        {
            hypeMeterBase.GetComponent<Image>().color = hype1;
            hypeMeterMid.GetComponent<Image>().color = hype2;
            hypeMeterNum.GetComponent<Text>().text = "1";
            hypeMeterMid.GetComponent<Image>().fillAmount = (hypeLevel-3) / (10-3.0f);
        }
        else if (hypeLevel < 100)
        {
            hypeMeterBase.GetComponent<Image>().color = hype2;
            hypeMeterMid.GetComponent<Image>().color = hype3;
            hypeMeterNum.GetComponent<Text>().text = "2";
            hypeMeterMid.GetComponent<Image>().fillAmount = (hypeLevel - 10) / (100 - 10.0f);
        }
        else if (hypeLevel >= 10)
        {
            hypeMeterBase.GetComponent<Image>().color = hype3;
            hypeMeterMid.GetComponent<Image>().color = hype3;
            hypeMeterNum.GetComponent<Text>().text = "3";
            hypeMeterMid.GetComponent<Image>().fillAmount = 1;//(hypeLevel - 3) / (10 - 3.0f);
        }


        //////////////////// health bar /////////////////////////////
      //  GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Text>().text = "HYPE: " + player.GetComponent<PlayerControler>().hypeLevel;
        health = player.GetComponent<PlayerControler>().health;
        //Debug.Log(health);
        healthUI.fillAmount = health / maxHealth;
        bombUI.fillAmount = player.GetComponent<PlayerControler>().bombs / 4f;
        // GameObject.FindGameObjectWithTag("BombCount").GetComponent<Text>().text = "BOMBS: " + player.GetComponent<PlayerControler>().bombs;

        GameObject.FindGameObjectWithTag("BossHealth").GetComponent<Text>().text = "BOSS HEALTH: " + bossHealth;
        if(level == 2)
            GameObject.FindGameObjectWithTag("BossHealth2").GetComponent<Text>().text = "BOSS HEALTH: " + SecondBossHealth;

        GameObject.FindGameObjectWithTag("Scores").GetComponent<Text>().text = "HISCORE: " + sm.hiscore + "\n  SCORE: " + sm.score;
    }

    ///////SCENE TRANSITIONS///////
    
    //wipe field
    void EndBattle()
    {
        player.GetComponent<PlayerControler>().invulnerablility = float.MaxValue;

        bgm.clip = bossDeath;
        bgm.loop = false;
        bgm.Play(0);

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject[] bBullets = GameObject.FindGameObjectsWithTag("BigBullet");
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if(boss != null)
            Destroy(boss.gameObject);
        foreach (GameObject b in bullets)
            Destroy(b.gameObject);
        foreach (GameObject b in bBullets)
            Destroy(b.gameObject);
    }

    IEnumerator FadeIn()
    {
        Image fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();
        Color shade = Color.black;
        shade.a = 1;

        bgm.volume = 0;

        for (int i = 0; i < 100; i++)
        {
            shade.a -= .01f;
            fade.color = shade;
            yield return new WaitForSeconds(.01f);
        }

        bgm.Play();
        for (int i = 0; i < 100; i++)
        {
            bgm.volume += .01f;
            yield return new WaitForSeconds(.01f);
        }
    }

    IEnumerator FadeOut()
    {
        Image fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();
        Color shade = Color.black;
        shade.a = 0;

        for (int i = 0; i < 100; i++)
        {
            shade.a += .01f;
            fade.color = shade;
            yield return new WaitForSeconds(.01f);
        }

        // load next level
        if (sm.fullPlaythrough && level != 7)
        {
            SceneManager.LoadScene("Level_" + ((int)level + 1), LoadSceneMode.Single);
        }
        else
        {
            sm.Save();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }

    IEnumerator GameOver()
    {
        AudioSource[] aus = FindObjectsOfType<AudioSource>();
        foreach(AudioSource a in aus)
        {
            a.volume = 0;
        }

        bgm.volume = 1;
        bgm.clip = playerDeath;
        bgm.loop = false;
        bgm.volume = .75f;
        bgm.Play(0);

        player.GetComponent<PlayerControler>().isPaused = true;
        player.GetComponent<Renderer>().enabled = false;
        player.GetComponent<PlayerControler>().hitbox.SetActive(false);

        Image fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();
        Text text1 = gameOverText.transform.GetChild(0).GetComponent<Text>();
        Text text2 = gameOverText.transform.GetChild(1).GetComponent<Text>();
        Color shade1 = Color.black;
        Color shade2 = Color.white;
        shade1.a = 0;
        shade2.a = 0;

        for (int i = 0; i < 100; i++)
        {
            shade1.a += .01f;
            shade2.a += .01f;
            fade.color = shade1;
            text1.color = shade2;
            text2.color = shade2;
            yield return new WaitForSeconds(.01f);
        }

        yield return new WaitForSeconds(3);

        for (int i = 0; i < 150; i++)
        {
            shade2.a -= .00666f;
            text1.color = shade2;
            text2.color = shade2;
            yield return new WaitForSeconds(.01f);
        }

        yield return new WaitForSeconds(.5f);

        sm.Save();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    ///////////////////
    //SCORE MECHANICS//
    ///////////////////

    public void Score(int amount)
    {
        sm.Score(amount, hypeLevel + 1);
    }

}
