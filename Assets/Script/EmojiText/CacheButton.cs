using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiText
{
    public class CacheButton : CacheItem
    {
        public Image Image;
        public Button Button;

        public override string GetPoolPath()
        {
            return "EmojiTextPool/Button";
        }

        public override void Init(GameObject go, Transform parent)
        {
            base.Init(go, parent);
            this.Image = go.GetComponent<Image>();
            this.Button = go.GetComponent<Button>();
        }
    }
}


