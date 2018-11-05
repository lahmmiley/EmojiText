using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EmojiText
{
    public class EmojiText : Text
    {
        static readonly Vector2 _one = Vector2.one;

        static List<CacheImage> _imagePoolList;
        private List<CacheImage> _showImageList;

        static List<CacheButton> _buttonPoolList;
        private List<CacheButton> _showButtonList;

        static List<CacheDummyImage> _dummyImagePoolList;
        private List<CacheDummyImage> _showDummyImageList;

        static Dictionary<int, List<CacheEmoji>> _emojiDict = new Dictionary<int, List<CacheEmoji>>();
        private List<CacheEmoji> _showEmojiList;

        private Dictionary<int, TagData> _tagDict;
        private Vector2 _zero = new Vector2(0, 0);
        /// <summary>
        /// _dirty为true 重新创建tag内容
        /// </summary>
        private bool _dirty = false;
        private string _lastText = string.Empty;
        private int _lastSize = -1;

        private static readonly Regex _regex = new Regex(@"<t=(.+?)>", RegexOptions.Singleline);

        public override string text
        {
            set
            {
                _lastText = value;
                _lastSize = fontSize;
                _dirty = false;
                base.text = Parse(value);
            }
        }

        protected override void OnDestroy()
        {
            if (_showImageList != null)
            {
                ListPool<CacheImage>.Release(_showImageList);
            }
            if (_showButtonList != null)
            {
                ListPool<CacheButton>.Release(_showButtonList);
            }
            if (_showDummyImageList != null)
            {
                ListPool<CacheDummyImage>.Release(_showDummyImageList);
            }
            if (_showEmojiList != null)
            {
                ListPool<CacheEmoji>.Release(_showEmojiList);
            }
        }

        private string Parse(string sourceString)
        {
            if (_tagDict != null) _tagDict.Clear();
            MatchCollection mc = _regex.Matches(sourceString);
            StringBuilder sb = null;
            int tagStartIndex = 0;  //解析字符串后的tag的首个匹配下标
            int lastMatchEndIndex = 0;    //sourceString上次匹配成功的结束下标
            int count = mc.Count;
            Debug.LogError(count);
            for (int i = 0; i < count; i++)
            {
                Match match = mc[i];
                TagData quadData = new TagData(match.Groups[1].Value, fontSize);
                if (sb == null) sb = new StringBuilder();
                if (_tagDict == null) _tagDict = new Dictionary<int, TagData>();
                string stringBeforeTag = sourceString.Substring(lastMatchEndIndex, match.Index - lastMatchEndIndex);
                sb.Append(stringBeforeTag);
                tagStartIndex += stringBeforeTag.Length;
                quadData.SetStartIndex(tagStartIndex);
                _tagDict.Add(tagStartIndex, quadData);
                sb.Append(quadData.PopulateText);
                tagStartIndex += quadData.PopulateText.Length;

                lastMatchEndIndex = match.Index + match.Length;
                if (i == (mc.Count - 1))
                {
                    if (lastMatchEndIndex < sourceString.Length)
                    {
                        sb.Append(sourceString.Substring(lastMatchEndIndex, sourceString.Length - lastMatchEndIndex));
                    }
                }
            }
            if (sb != null)
            {
                return sb.ToString();
            }
            return sourceString;
        }

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }
#endif
            if (font == null)
                return;
            if (_lastSize != -1 && _lastSize != fontSize)
            {
                _lastSize = fontSize;
                text = _lastText;
                return;
            }
            if (_dirty)
            {
                _dirty = false;
                CreateTag();
            }
        }

        private void CreateIcon(TagData tagData)
        {
            CacheImage cacheImage = GetFromPool<CacheImage>(_imagePoolList, tagData);
            if (_showImageList == null)
            {
                _showImageList = ListPool<CacheImage>.Get();
            }
            _showImageList.Add(cacheImage);
            cacheImage.Image.sprite = Resources.Load<Sprite>(tagData.GetIconPath()) as Sprite;
            cacheImage.RectTrans.sizeDelta = new Vector2(tagData.Width, tagData.Height);
            cacheImage.RectTrans.anchoredPosition3D = tagData.StartPosition;
        }

        private void CreateButton(TagData tagData)
        {
            CacheButton cacheButton = GetFromPool<CacheButton>(_buttonPoolList, tagData);
            if (_showButtonList == null)
            {
                _showButtonList = ListPool<CacheButton>.Get();
            }
            _showButtonList.Add(cacheButton);
            cacheButton.Button.onClick.RemoveAllListeners();
            cacheButton.Button.onClick.AddListener(() =>
            {
                Debug.LogError("OnClick:" + tagData.Id.ToString());
            });
            cacheButton.Image.sprite = Resources.Load<Sprite>(tagData.GetIconPath()) as Sprite;
            cacheButton.RectTrans.sizeDelta = new Vector2(tagData.Width, tagData.Height);
            cacheButton.RectTrans.anchoredPosition3D = tagData.StartPosition;
        }

        private void CreateHyperlink(TagData tagData)
        {
            List<Vector4> boundList = tagData.boundList;
            if (boundList != null)
            {
                for (int i = 0; i < boundList.Count; i++)
                {
                    Vector4 bound = boundList[i];
                    CacheDummyImage cacheDummyImage = GetFromPool<CacheDummyImage>(_dummyImagePoolList, tagData);
                    if (_showDummyImageList == null)
                    {
                        _showDummyImageList = ListPool<CacheDummyImage>.Get();
                    }
                    _showDummyImageList.Add(cacheDummyImage);
                    cacheDummyImage.Button.onClick.RemoveAllListeners();
                    cacheDummyImage.Button.onClick.AddListener(() =>
                    {
                        Debug.LogError("OnHyperlinkClick:" + tagData.PopulateText);
                    });
                    cacheDummyImage.RectTrans.sizeDelta = new Vector2(bound.z - bound.x, bound.w - bound.y);
                    cacheDummyImage.RectTrans.anchoredPosition3D = new Vector3(bound.x, bound.y, 0);
                }
            }
        }

        public void CreateEmoji(TagData tagData)
        {
            List<CacheEmoji> list = null;
            if (_emojiDict.ContainsKey(tagData.Id))
            {
                list = _emojiDict[tagData.Id];
            }
            CacheEmoji cacheEmoji = GetFromPool<CacheEmoji>(list, tagData);
            cacheEmoji.Key = tagData.Id;
            if (_showEmojiList == null)
            {
                _showEmojiList = ListPool<CacheEmoji>.Get();
            }
            _showEmojiList.Add(cacheEmoji);
            cacheEmoji.RectTrans.sizeDelta = new Vector2(tagData.Width, tagData.Height);
            cacheEmoji.RectTrans.anchoredPosition3D = tagData.StartPosition;
        }

        private void CreateTag()
        {
            _imagePoolList = PushListToPool<CacheImage>(_imagePoolList, _showImageList);
            _buttonPoolList = PushListToPool<CacheButton>(_buttonPoolList, _showButtonList);
            _dummyImagePoolList = PushListToPool<CacheDummyImage>(_dummyImagePoolList, _showDummyImageList);
            PushEmojiToPool();

            if (_tagDict != null)
            {
                foreach (TagData tagData in _tagDict.Values)
                {
                    if (!tagData.Valid)
                    {
                        continue;
                    }
                    switch (tagData.Type)
                    {
                        case TagType.icon:
                            CreateIcon(tagData);
                            break;
                        case TagType.emoji:
                            CreateEmoji(tagData);
                            break;
                        case TagType.button:
                            CreateButton(tagData);
                            break;
                        case TagType.hyperlink:
                            CreateHyperlink(tagData);
                            break;
                    }
                }
            }

            HideUnuse<CacheImage>(_imagePoolList);
            HideUnuse<CacheButton>(_buttonPoolList);
            HideUnuse<CacheDummyImage>(_dummyImagePoolList);
            HideUnuseEmoji();
        }

        protected Vector4 GetCharBound()
        {
            Vector4 result = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector3 position = m_TempVerts[i].position;
                if (position.x < result.x) result.x = position.x;
                if (position.y < result.y) result.y = position.y;
                if (position.x > result.z) result.z = position.x;
                if (position.y > result.w) result.w = position.y;
            }
            return result;
        }


        readonly UIVertex[] m_TempVerts = new UIVertex[4];
        private bool IsTempVertexValid()
        {
            Vector3 vector0 = m_TempVerts[0].position;
            Vector3 vector1 = m_TempVerts[1].position;
            if (vector0.x == vector1.x && vector0.y == vector1.y)
            {
                return false;
            }
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line... (\n)
            int vertCount = verts.Count - 4;

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            if (_tagDict != null)
            {
                foreach (TagData tagData in _tagDict.Values)
                {
                    tagData.SetValid(false);
                }
            }

            TagData hyperlinkTagData = null;
            Vector2 sizeDelta = rectTransform.sizeDelta;
            Vector2 pivot = rectTransform.pivot;
            float xOffset = sizeDelta.x * pivot.x;
            float yOffset = sizeDelta.y * pivot.y;

            Vector4 buttonBound = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            bool buttonBoundInit = false;
            float lastCharMinY = float.MaxValue;
            bool haveRoundingOffset = roundingOffset != Vector2.zero;

            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (haveRoundingOffset)
                {
                    m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                }
                if (tempVertsIndex == 3)
                {
                    int stringIndex = Mathf.FloorToInt(i / 4);
                    if (hyperlinkTagData != null)
                    {
                        CalculateHyperlinkBound(hyperlinkTagData, ref buttonBound, ref buttonBoundInit,ref lastCharMinY, xOffset, yOffset, stringIndex);
                    }
                    else if (_tagDict != null && _tagDict.ContainsKey(stringIndex))
                    {
                        TagData tagData = _tagDict[stringIndex];
                        tagData.SetValid(true);
                        if (tagData.UseQuad())
                        {
                            float minX = float.MaxValue;
                            float minY = float.MaxValue;
                            for (int j = 0; j < 4; j++)
                            {
                                m_TempVerts[j].uv0 = _zero;//清除占位符显示 也可以用<color=#00000000><quad></color>来隐藏

                                if (m_TempVerts[j].position.x < minX) minX = m_TempVerts[j].position.x;//获取占位符左下角坐标
                                if (m_TempVerts[j].position.y < minY) minY = m_TempVerts[j].position.y;//获取占位符左下角坐标

                            }
                            tagData.SetStartPosition(new Vector3(minX + xOffset, minY + yOffset, 0));
                        }
                        else if (tagData.Type == TagType.hyperlink)
                        {
                            tagData.ClearBound();
                            hyperlinkTagData = tagData;
                            if (IsTempVertexValid())
                            {
                                Vector4 charBound = GetCharBound();
                                buttonBound = charBound;
                                lastCharMinY = charBound.y;
                                buttonBoundInit = true;
                            }
                        }
                    }
                    toFill.AddUIVertexQuad(m_TempVerts);
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
            _dirty = true;
        }

        private void CalculateHyperlinkBound(TagData hyperlinkTagData, ref Vector4 buttonBound, ref bool buttonBoundInit, ref float lastCharMinY, float xOffset, float yOffset, int stringIndex)
        {
            if (IsTempVertexValid())
            {
                Vector4 charBound = GetCharBound();
                if (buttonBoundInit)
                {
                    //判断换行
                    if ((lastCharMinY != float.MaxValue) && (lastCharMinY > charBound.w))
                    {
                        buttonBound.x += xOffset;
                        buttonBound.y += yOffset;
                        buttonBound.z += xOffset;
                        buttonBound.w += yOffset;
                        hyperlinkTagData.AddBound(buttonBound);
                        lastCharMinY = charBound.y;
                        buttonBound = charBound;
                    }
                    else
                    {
                        lastCharMinY = charBound.y;
                        if (charBound.x < buttonBound.x) buttonBound.x = charBound.x;
                        if (charBound.y < buttonBound.y) buttonBound.y = charBound.y;
                        if (charBound.z > buttonBound.z) buttonBound.z = charBound.z;
                        if (charBound.w > buttonBound.w) buttonBound.w = charBound.w;
                    }
                }
                else
                {
                    buttonBound = charBound;
                    lastCharMinY = charBound.y;
                    buttonBoundInit = true;
                }
            }

            if ((hyperlinkTagData.GetEndIndex() - 1) == stringIndex)
            {
                if (buttonBoundInit)
                {
                    buttonBound.x += xOffset;
                    buttonBound.y += yOffset;
                    buttonBound.z += xOffset;
                    buttonBound.w += yOffset;
                    hyperlinkTagData.AddBound(buttonBound); ;
                }
                buttonBoundInit = false;
                hyperlinkTagData = null;
            }

        }

        // 缓存操作
        public T GetFromPool<T>(List<T> pool, TagData tagData) where T : CacheItem, new()
        {
            T result = null;
            if (pool != null && pool.Count > 0)
            {
                for (int i = pool.Count - 1; i >= 0; i--)
                {
                    if (!pool[i].Used)
                    {
                        result = pool[i];
                        result.Used = true;
                        result.InitFromPool(transform);
                        pool.RemoveAt(i);
                        return result;
                    }
                }
            }

            Object obj = Resources.Load(tagData.GetPrefabPath(), typeof(GameObject));
            GameObject go = GameObject.Instantiate(obj) as GameObject;
            result = new T();
            result.Init(go, transform);
            result.Used = true;
            result.SetActive(true);
            return result;
        }

        private List<T> PushListToPool<T>(List<T> pool, List<T> list) where T : CacheItem
        {
            if (list == null)
            {
                return pool;
            }
            if (pool == null)
            {
                pool = new List<T>();
            }
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    T item = list[i];
                    item.Used = false;
                    pool.Add(item);
                }
                list.Clear();
            }
            return pool;
        }

        private void PushEmojiToPool()
        {
            if (_showEmojiList != null)
            {
                for (int i = 0; i < _showEmojiList.Count; i++)
                {
                    CacheEmoji cacheEmoji = _showEmojiList[i];
                    cacheEmoji.Used = false;
                    if (!_emojiDict.ContainsKey(cacheEmoji.Key))
                    {
                        _emojiDict.Add(cacheEmoji.Key, ListPool<CacheEmoji>.Get());
                    }
                    List<CacheEmoji> list = _emojiDict[cacheEmoji.Key];
                    list.Add(cacheEmoji);
                }
                _showEmojiList.Clear();
            }
        }

        private void HideUnuse<T>(List<T> list) where T : CacheItem
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Used)
                    {
                        list[i].SetActive(false);
                    }
                }
            }
        }

        private void HideUnuseEmoji()
        {
            if (_emojiDict != null)
            {
                foreach (List<CacheEmoji> list in _emojiDict.Values)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!list[i].Used)
                        {
                            list[i].SetActive(false);
                        }
                    }
                }
            }
        }
    }
}