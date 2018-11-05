using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<EmojiText.EmojiText>().text = "本组件支持图片：<t=1,90001>，表情：<t=2,2>，按钮：<t=3,1>，<t=4,超链接,FFFF00>";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
