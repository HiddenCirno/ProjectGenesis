﻿using System;
using System.Collections.Generic;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.UIQTools
{
    public abstract class MyComboBox : MonoBehaviour
    {
        protected int DefaultSprite;
        public int selectIndex;
        public Text labelText;
        public UIComboBox comboBox;
        public Image iconImg;
        public UIButton button;

        public event Action<int> OnItemChange;

        internal static T CreateComboBox<T>(
            float x,
            float y,
            RectTransform parent,
            string label = "",
            int fontSize = 18) where T : MyComboBox
        {
            UIComboBox src = UIRoot.instance.optionWindow.msaaComp;
            GameObject go = Instantiate(src.transform.parent.gameObject);

            go.name = "my-combobox";
            var cb = go.AddComponent<T>();
            cb.selectIndex = 0;

            RectTransform rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);

            cb.comboBox = go.GetComponentInChildren<UIComboBox>();

            DestroyImmediate(cb.GetComponent<Localizer>());
            cb.labelText = cb.GetComponent<Text>();
            cb.labelText.fontSize = fontSize;
            cb.SetLabelText(label);

            Util.CreateSignalIcon("", "", out UIButton button, out Image iconImage);
            cb.iconImg = iconImage;
            cb.button = button;

            Util.NormalizeRectWithTopLeft(button, 170, -5, rect);

            cb.button.button.onClick.RemoveAllListeners();
            cb.button.button.onClick.AddListener(cb.OnUIButtonClick);

            return cb;
        }

        protected void Init(List<string> items, int defaultSprite = 509)
        {
            comboBox.Items = items;
            DefaultSprite = defaultSprite;
            iconImg.sprite = LDB.signals.IconSprite(defaultSprite);
            comboBox.ItemButtons = new List<Button>();
            comboBox.UpdateItems();
            comboBox.onItemIndexChange.RemoveAllListeners();
            comboBox.onItemIndexChange.AddListener(OnItemIndexChange);
            comboBox.itemIndex = 0;
        }

        public void OnUIButtonClick() => comboBox.itemIndex = (comboBox.itemIndex + 1) % comboBox.Items.Count;

        private void OnItemIndexChange()
        {
            selectIndex = comboBox.itemIndex;
            UpdateSprite();
            OnItemChange?.Invoke(selectIndex);
        }

        public abstract void UpdateSprite();

        public void SetLabelText(string val)
        {
            if (labelText != null) labelText.text = val.TranslateFromJson();
        }
    }
}
