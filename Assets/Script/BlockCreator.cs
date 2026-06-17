using UnityEngine;
using System.Collections;

public class BlockCreator : MonoBehaviour {
    public SushiType m_sushiType; //作りたい寿司を指定するため.


	// Use this for initialization
	void Start () {
        Create();
	}
	

    void Create() {
        string blockName = m_sushiType.ToString();
        GameObject block = Instantiate(Resources.Load(blockName), transform.position, transform.rotation) as GameObject;

        block.transform.parent = this.gameObject.transform; //Hierarchyが散らばらないように.
    }

}
