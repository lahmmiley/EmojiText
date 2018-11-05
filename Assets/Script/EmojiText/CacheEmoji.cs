using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EmojiText
{
    public class CacheEmoji : CacheItem
    {
        public int Key;

        public override string GetPoolPath()
        {
            return "EmojiTextPool/Emoji";
        }

        public override void InitFromPool(Transform parent)
        {
            base.InitFromPool(parent);
            SetActive(true);
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            Go.SetActive(active);
        }
    }
}
