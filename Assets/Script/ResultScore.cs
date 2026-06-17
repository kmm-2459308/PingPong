using UnityEngine;
using System.Collections;
using System;

public class ResultScore : MonoBehaviour {
    GameObject m_icon;
    GameObject m_peke;
    GameObject m_sushiNum;
    GameObject m_score;

    int m_scoreCounter; //カウントアップ用.
    int m_scoreMax;     //表示したいスコア.
    int m_getNum;       //獲得数表示用.

    enum State {
        Wait,       //待機中.
        In,         //入場.
        CountUp,    //カウントアップ中.
        End,        //終わり.
    };
    State m_state;

	// Use this for initialization
	void Start () {
        m_scoreCounter = 0;
        m_scoreMax = 0;
        m_getNum = 0;

        m_state = State.Wait;

        m_icon = transform.Find("sushi_icon").gameObject;
        m_peke = transform.Find("peke").gameObject;
        m_sushiNum = transform.Find("sushinum").gameObject;
        m_score = transform.Find("score").gameObject;

        //表示OFF.
        SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderer) {
            sr.enabled = false;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.Wait:
            break;

        case State.In:
            //表示物ON.
            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderer) {
                sr.enabled = true;
            }
            m_sushiNum.GetComponent<Number>().SetNum(m_getNum);
            m_score.GetComponent<Number>().SetNum(0);

            m_state = State.CountUp;
            break;

        case State.CountUp:
            //カウントアップさせる.
            m_scoreCounter++;
            m_scoreCounter = Math.Min(m_scoreCounter, m_scoreMax);
            m_score.GetComponent<Number>().SetNum(m_scoreCounter);

            if (m_scoreCounter >= m_scoreMax) {
                m_state = State.End;
            }
            break;

        case State.End:
            break;
        }
	}

    
    /**
     * アニメーションスタートさせる.
     * @param getNum    獲得数.
     * @param score     得点.
     */
    public void FadeIn(int getNum, int score) {
        m_state = State.In;

        m_getNum = getNum;
        m_scoreMax = score;
        m_scoreCounter = 0;
    }

    //アニメーション終了ならtrue.
    public bool IsEnd() {
        return (m_state == State.End);
    }
}
