using UnityEngine;
using System.Collections;

public class ScoreSushi : MonoBehaviour {
    public SushiType m_sushiType;       //寿司の種類.

    Vector3 m_startPos; //フェードアウト時の初期位置.
    Vector3 m_target;   //フェードアウトの行き先.
    
    float m_timer;
    const float FADE_TIME = 2.0f;
    
    enum State {
        Wait,
        FadeOut,
    };
    State m_state;


	// Use this for initialization
	void Start () {
        m_state = State.Wait;
        m_timer = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate() {
        switch (m_state) {
        case State.Wait:
            break;
        case State.FadeOut:
            m_timer += Time.fixedDeltaTime;
            
            //targetに向かって座標を動かす.
            float rate = m_timer / FADE_TIME;
            rate = Mathf.Min(rate, 1.0f);
            Vector3 pos = Vector3.Lerp(m_target, m_startPos, Mathf.Exp(-5.0f*rate));
            transform.position = pos;

            //スケール変化.小さくなり過ぎないように調整.
            transform.localScale = Vector3.one * (0.3f + 0.7f * Mathf.Exp(-5.0f*rate));
            
            break;
        }

	}


    //targetに合わせてフェードアウトする.
    public void StartFadeOut(Vector3 target) {
        m_startPos = transform.position;
        m_target = target;
        
        m_timer = 0;
        m_state = State.FadeOut;
    }

    //フェードアウト中ならtrue.
    public bool IsFadeOut() {
        return (m_state == State.FadeOut);
    }
    //フェードアウトが終わっていればtrue.
    public bool IsFadeOutEnd() {
        if (IsFadeOut()) {
            if (m_timer > FADE_TIME) {
                return true;
            }
        }
        return false;
    }

}
