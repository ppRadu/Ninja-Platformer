using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    private static GameManager instance;

    [SerializeField]
    private GameObject enemyCoinPrefab;

    [SerializeField]
    private Text scoreValue;

    private int score;

    private int collectedCoins;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    //Pentru a obtine obiectul EnemyCoin pentru a-l pune in scene atunci cand inamicul moare
    public GameObject EnemyCoinPrefab
    {
        get
        {
            return enemyCoinPrefab;
        }
    } 

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            scoreValue.text = (value).ToString();
            score = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
