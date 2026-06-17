using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 自動で消えるエフェクト.
public class EffectControl : MonoBehaviour {

	void FixedUpdate(){
        ParticleSystem[] effects = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in effects) {
            if (ps.isStopped == false) {
                return;
            }
        }
        //エフェクト終了したので消す.
        Destroy(this.gameObject);
	}
}
