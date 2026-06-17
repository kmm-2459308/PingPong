using UnityEngine;
using System.Collections;

/* 寿司のアニメーション制御 */
public class Sushi : MonoBehaviour {
    public SushiType m_sushiType;   //寿司の種類(アニメーションの指定で使います).

    //アニメーション定義.
    public enum AnimationType {
        sleep,
        dance,
        jump,
    };
    AnimationType m_current;
    Animation m_animation;

    // Use this for initialization
    void Start() {
        m_animation = GetComponent<Animation>();
        m_current = AnimationType.sleep;

        if (m_animation.isPlaying == false) {
            PlayAnimation(AnimationType.sleep);
        }
    }


	// Update is called once per frame
	void FixedUpdate() {
        switch (m_current) {
        case AnimationType.sleep:
            break;
        case AnimationType.jump:
            //ジャンプ終了後は自動でアニメーション遷移させる.
            if (m_animation.isPlaying == false) {
                PlayAnimation(AnimationType.dance);
            }
            break;
        case AnimationType.dance:
            break;
        }
	}

    //アニメーションを再生する.
    public void PlayAnimation(AnimationType anim) {
        m_current = anim;
        
        //寿司の種類に応じてアニメーションを指定する.
        string animName = m_sushiType.ToString() + "_" + m_current.ToString();
        m_animation.Play(animName);
    }

}
