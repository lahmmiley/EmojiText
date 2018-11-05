using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiText
{
    public class CacheImage : CacheItem
    {
        public Image Image;

        public override string GetPoolPath()
        {
            return "EmojiTextPool/Image";
        }

        public override void Init(GameObject go, Transform parent)
        {
            base.Init(go, parent);
            this.Image = go.GetComponent<Image>();
        }
    }
}
