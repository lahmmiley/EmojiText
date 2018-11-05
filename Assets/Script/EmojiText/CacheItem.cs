using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EmojiText
{
    public class CacheItem
    {
        public static readonly Vector3 HIDE_POSITION = new Vector3(0, 0, 0);
        public bool Used = false;
        public GameObject Go;
        public RectTransform RectTrans;

        private Transform _poolTransform;
        protected Transform poolTransform
        {
            get
            {
                if (_poolTransform == null)
                {
                    _poolTransform = GameObject.Find(GetPoolPath()).transform;
                }
                return _poolTransform;
            }
        }

        public CacheItem() { }

        public virtual void Init(GameObject go, Transform parent)
        {
            this.Go = go;
            this.RectTrans = go.GetComponent<RectTransform>();
            this.RectTrans.SetParent(parent);
            this.RectTrans.localScale = Vector3.one;
            this.RectTrans.localRotation = Quaternion.identity;
            this.RectTrans.pivot = Vector2.zero;
            this.RectTrans.anchorMin = Vector2.zero;
            this.RectTrans.anchorMax = Vector2.zero;
        }

        public virtual string GetPoolPath()
        {
            return string.Empty;
        }

        public virtual void SetActive(bool active)
        {
            if (!active)
            {
                this.RectTrans.SetParent(poolTransform);
                this.RectTrans.anchoredPosition = HIDE_POSITION;
            }
        }

        public virtual void InitFromPool(Transform parent)
        {
            this.RectTrans.SetParent(parent);
            this.RectTrans.localScale = Vector3.one;
            this.RectTrans.localRotation = Quaternion.identity;
            this.RectTrans.pivot = Vector2.zero;
            this.RectTrans.anchorMin = Vector2.zero;
            this.RectTrans.anchorMax = Vector2.zero;
        }
    }
}