using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EmojiText
{
    public class TagData
    {
        public const float ICON_SCALE = 1.5f;
        public const float EMOJI_SCALE = 1.5f;
        public const float BUTTON_SCALE = 2f;

        public TagType Type;
        public int Id;
        public int Length;
        public string PopulateText;//填充文本
        public Vector3 StartPosition;
        public float Width;
        public float Height;
        public int Size;
        public bool Valid = false;//在显示范围内的

        private int _startIndex;
        private List<Vector4> _boundList;
        public List<Vector4> boundList
        {
            get
            {
                return _boundList;
            }
        }

        public TagData(string param, int size)
        {
            string[] splitArray = param.Split(',');
            this.Type = (TagType)int.Parse(splitArray[0]);
            this.Size = size;
            switch (this.Type)
            {
                case TagType.icon:
                    this.Id = int.Parse(splitArray[1]);
                    PopulateText = string.Format("<quad Size={0} Width={1}>", size.ToString(), ICON_SCALE.ToString());
                    Width = size * ICON_SCALE;
                    Height = size * ICON_SCALE;
                    break;
                case TagType.emoji:
                    this.Id = int.Parse(splitArray[1]);
                    PopulateText = string.Format("<quad Size={0} Width={1}>", size.ToString(), EMOJI_SCALE.ToString());
                    Width = size * EMOJI_SCALE;
                    Height = size * EMOJI_SCALE;
                    break;
                case TagType.button:
                    this.Id = int.Parse(splitArray[1]);
                    PopulateText = string.Format("<quad Size={0}, Width={1}>", size.ToString(), BUTTON_SCALE.ToString());
                    Width = size * 2;
                    Height = size;
                    break;
                case TagType.hyperlink:
                    PopulateText = string.Format("<color=#{1}>{0}</color>", splitArray[1], splitArray[2]);
                    break;
            }
            this.Length = PopulateText.Length;
        }

        public void SetStartIndex(int index)
        {
            _startIndex = index;
        }

        public int GetEndIndex()
        {
            return _startIndex + this.Length;
        }

        public string GetPrefabPath()
        {
            string result = string.Empty;
            switch (this.Type)
            {
                case TagType.icon:
                    result = "Emoji/Prefab/Image";
                    break;
                case TagType.emoji:
                    result = string.Format("Emoji/Face/Face_{0}", this.Id.ToString());
                    break;
                case TagType.button:
                    result = "Emoji/Prefab/Button";
                    break;
                case TagType.hyperlink:
                    result = "Emoji/Prefab/DummyImage";
                    break;
            }
            return result;
        }

        public string GetIconPath()
        {
            string result = string.Empty;
            switch (this.Type)
            {
                case TagType.icon:
                    result = string.Format("Emoji/Currency/{0}", this.Id.ToString());
                    break;
                case TagType.button:
                    result = string.Format("Emoji/Button/{0}", this.Id.ToString());
                    break;
                default:
                    Debug.LogError("找不到类型:" + this.Type.ToString());
                    break;
            }
            return result;
        }

        public void SetStartPosition(Vector3 position)
        {
            float offsetY = (this.Height - this.Size) / 2f + 2; //2为固定偏移值 可以根据项目情况微调
            position.Set(position.x, position.y - offsetY, position.z);
            StartPosition = position;
        }

        public bool UseQuad()
        {
            return this.Type != TagType.hyperlink;
        }

        public void SetValid(bool valid)
        {
            this.Valid = valid;
        }

        public void AddBound(Vector4 bound)
        {
            if (_boundList == null)
            {
                _boundList = new List<Vector4>();
            }
            _boundList.Add(bound);
        }

        public void ClearBound()
        {
            if (_boundList != null)
            {
                _boundList.Clear();
            }
        }
    }
}