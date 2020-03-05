﻿using System.Collections.ObjectModel;
/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace XCharts
{
    /// <summary>
    /// Tooltip component.
    /// 提示框组件
    /// </summary>
    [System.Serializable]
    public class Tooltip : MainComponent
    {
        /// <summary>
        /// Indicator type.
        /// 指示器类型。
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// line indicator.
            /// 直线指示器
            /// </summary>
            Line,
            /// <summary>
            /// shadow crosshair indicator.
            /// 阴影指示器
            /// </summary>
            Shadow,
            /// <summary>
            /// no indicator displayed.
            /// 无指示器
            /// </summary>
            None,
            /// <summary>
            /// crosshair indicator, which is actually the shortcut of enable two axisPointers of two orthometric axes.
            /// 十字准星指示器。坐标轴显示Label和交叉线。
            /// </summary>
            Corss
        }

        [SerializeField] private bool m_Show;
        [SerializeField] private Type m_Type;
        [SerializeField] private string m_Formatter;
        [SerializeField] private string m_ItemFormatter;
        [SerializeField] private string m_TitleFormatter;
        [SerializeField] private float m_FixedWidth = 0;
        [SerializeField] private float m_FixedHeight = 0;
        [SerializeField] private float m_MinWidth = 0;
        [SerializeField] private float m_MinHeight = 0;
        [SerializeField] private bool m_ForceENotation = false;
        [SerializeField] private float m_PaddingLeftRight = 5f;
        [SerializeField] private float m_PaddingTopBottom = 5f;
        [SerializeField] private Sprite m_BackgroundImage;
        [SerializeField] private TextStyle m_TextStyle = new TextStyle(18, FontStyle.Normal);
        [SerializeField] private LineStyle m_LineStyle = new LineStyle(LineStyle.Type.Solid, 0.7f);

        private GameObject m_GameObject;
        private GameObject m_Content;
        private Text m_ContentText;
        private Image m_ContentImage;
        private RectTransform m_ContentRect;
        private RectTransform m_ContentTextRect;
        private List<int> lastDataIndex { get; set; }

        /// <summary>
        /// Whether to show the tooltip component.
        /// 是否显示提示框组件。
        /// </summary>
        public bool show
        {
            get { return m_Show; }
            set { if (PropertyUtility.SetStruct(ref m_Show, value)) { SetAllDirty(); SetActive(value); } }
        }
        /// <summary>
        /// Indicator type.
        /// 提示框指示器类型。
        /// </summary>
        public Type type
        {
            get { return m_Type; }
            set { if (PropertyUtility.SetStruct(ref m_Type, value)) SetAllDirty(); }
        }
        /// <summary>
        /// 提示框总内容的字符串模版格式器。支持用 \n 或 "<br/>" 换行。当formatter不为空时，优先使用formatter，否则使用itemFormatter。
        /// 模板变量有 {a}, {b}，{c}，{d}，{e}，分别表示系列名，数据名，数据值等。{a0},{b1},c{1}等可指定serie。
        /// 其中变量{a}, {b}, {c}, {d}在不同图表类型下代表数据含义为：
        /// <list type="bullet">
        /// <item><description>折线（区域）图、柱状（条形）图、K线图 : {a}（系列名称），{b}（类目值），{c}（数值）, {d}（无）。</description></item>
        /// <item><description>散点图（气泡）图 : {a}（系列名称），{b}（数据名称），{c}（数值数组）, {d}（无）。</description></item>
        /// <item><description>地图 : {a}（系列名称），{b}（区域名称），{c}（合并数值）, {d}（无）。</description></item>
        /// <item><description>饼图、仪表盘、漏斗图: {a}（系列名称），{b}（数据项名称），{c}（数值）, {d}（百分比）。</description></item>
        /// </list>
        /// 示例："{a}:{c}","{a1}:{c1:f1}"
        /// </summary>
        public string formatter { get { return m_Formatter; } set { m_Formatter = value; } }
        /// <summary>
        /// 提示框标题内容的字符串模版格式器。支持用 \n 或 "<br/>" 换行。仅当itemFormatter生效时才有效。
        /// 模板变量有 {a}, {b}，{c}，{d}，{e}，分别表示系列名，数据名，数据值等。{a0},{b1},c{1}等可指定serie。
        /// 其中变量{a}, {b}, {c}, {d}在不同图表类型下代表数据含义为：
        /// <list type="bullet">
        /// <item><description>折线（区域）图、柱状（条形）图、K线图 : {a}（系列名称），{b}（类目值），{c}（数值）, {d}（无）。</description></item>
        /// <item><description>散点图（气泡）图 : {a}（系列名称），{b}（数据名称），{c}（数值数组）, {d}（无）。</description></item>
        /// <item><description>地图 : {a}（系列名称），{b}（区域名称），{c}（合并数值）, {d}（无）。</description></item>
        /// <item><description>饼图、仪表盘、漏斗图: {a}（系列名称），{b}（数据项名称），{c}（数值）, {d}（百分比）。</description></item>
        /// </list>
        /// 示例："{a}:{c}","{a1}:{c1:f1}"
        /// </summary>
        public string titleFormatter { get { return m_TitleFormatter; } set { m_TitleFormatter = value; } }
        /// <summary>
        /// 提示框单个serie或数据项内容的字符串模版格式器。支持用 \n 或 "<br/>" 换行。当formatter不为空时，优先使用formatter，否则使用itemFormatter。
        /// 模板变量有 {a}, {b}，{c}，{d}，{e}，分别表示系列名，数据名，数据值等。
        /// 其中变量{a}, {b}, {c}, {d}在不同图表类型下代表数据含义为：
        /// <list type="bullet">
        /// <item><description>折线（区域）图、柱状（条形）图、K线图 : {a}（系列名称），{b}（类目值），{c}（数值）, {d}（无）。</description></item>
        /// <item><description>散点图（气泡）图 : {a}（系列名称），{b}（数据名称），{c}（数值数组）, {d}（无）。</description></item>
        /// <item><description>地图 : {a}（系列名称），{b}（区域名称），{c}（合并数值）, {d}（无）。</description></item>
        /// <item><description>饼图、仪表盘、漏斗图: {a}（系列名称），{b}（数据项名称），{c}（数值）, {d}（百分比）。</description></item>
        /// </list>
        /// 示例："{a}:{c}","{a}:{c:f1}"
        /// </summary>
        public string itemFormatter { get { return m_ItemFormatter; } set { m_ItemFormatter = value; } }

        /// <summary>
        /// 固定宽度。比 minWidth 优先。
        /// </summary>
        public float fixedWidth { get { return m_FixedWidth; } set { m_FixedWidth = value; } }
        /// <summary>
        /// 固定高度。比 minHeight 优先。
        /// </summary>
        public float fixedHeight { get { return m_FixedHeight; } set { m_FixedHeight = value; } }
        /// <summary>
        /// 最小宽度。如若 fixedWidth 设有值，优先取 fixedWidth。
        /// </summary>
        public float minWidth { get { return m_MinWidth; } set { m_MinWidth = value; } }
        /// <summary>
        /// 最小高度。如若 fixedHeight 设有值，优先取 fixedHeight。
        /// </summary>
        public float minHeight { get { return m_MinHeight; } set { m_MinHeight = value; } }
        [Obsolete("Use Tooltip.textStyle.fontSize instead.", true)]
        public int fontSize { get; set; }
        [Obsolete("Use Tooltip.textStyle.fontStyle instead.", true)]
        public FontStyle fontStyle { get; set; }
        /// <summary>
        /// 是否强制使用科学计数法格式化显示数值。默认为false，当小数精度大于3时才采用科学计数法。
        /// </summary>
        public bool forceENotation { get { return m_ForceENotation; } set { m_ForceENotation = value; } }
        /// <summary>
        /// the text padding of left and right. defaut:5.
        /// 左右边距。
        /// </summary>
        public float paddingLeftRight { get { return m_PaddingLeftRight; } set { m_PaddingLeftRight = value; } }
        /// <summary>
        /// the text padding of top and bottom. defaut:5.
        /// 上下边距。
        /// </summary>
        public float paddingTopBottom { get { return m_PaddingTopBottom; } set { m_PaddingTopBottom = value; } }
        /// <summary>
        /// The image of icon.
        /// 图标的图片。
        /// </summary>
        public Sprite backgroundImage { get { return m_BackgroundImage; } set { m_BackgroundImage = value; SetBackground(m_BackgroundImage); } }
        /// <summary>
        /// 提示框内容文本样式。
        /// </summary>
        public TextStyle textStyle
        {
            get { return m_TextStyle; }
            set { if (value != null) { m_TextStyle = value; SetComponentDirty(); } }
        }
        /// <summary>
        /// 指示线样式。
        /// </summary>
        public LineStyle lineStyle
        {
            get { return m_LineStyle; }
            set { if (value != null) m_LineStyle = value; SetComponentDirty(); }
        }

        /// <summary>
        /// 组件是否需要刷新
        /// </summary>
        public override bool componentDirty
        {
            get { return m_ComponentDirty || lineStyle.componentDirty || textStyle.componentDirty; }
        }

        internal override void ClearComponentDirty()
        {
            base.ClearComponentDirty();
            lineStyle.ClearComponentDirty();
            textStyle.ClearComponentDirty();
        }

        /// <summary>
        /// The data index currently indicated by Tooltip.
        /// 当前提示框所指示的数据项索引。
        /// </summary>
        public List<int> runtimeDataIndex { get; internal set; }
        /// <summary>
        /// the value for x indicator label.
        /// 指示器X轴上要显示的值。
        /// </summary>
        public float[] runtimeXValues { get; internal set; }
        /// <summary>
        /// the value for y indicator label. 
        /// 指示器Y轴上要显示的值。
        /// </summary>
        public float[] runtimeYValues { get; internal set; }
        /// <summary>
        /// the current pointer position.
        /// 当前鼠标位置。
        /// </summary>
        public Vector2 runtimePointerPos { get; internal set; }
        /// <summary>
        /// the width of tooltip. 
        /// 提示框宽。
        /// </summary>
        public float runtimeWidth { get { return m_ContentRect.sizeDelta.x; } }
        /// <summary>
        /// the height of tooltip. 
        /// 提示框高。
        /// </summary>
        public float runtimeHeight { get { return m_ContentRect.sizeDelta.y; } }
        /// <summary>
        /// Whether the tooltip has been initialized. 
        /// 提示框是否已初始化。
        /// </summary>
        public bool runtimeInited { get { return m_GameObject != null; } }
        /// <summary>
        /// the gameObject of tooltip. 
        /// 提示框的gameObject。
        /// </summary>
        public GameObject runtimeGameObject { get { return m_GameObject; } }

        public static Tooltip defaultTooltip
        {
            get
            {
                var tooltip = new Tooltip
                {
                    m_Show = true,
                    runtimeXValues = new float[2] { -1, -1 },
                    runtimeYValues = new float[2] { -1, -1 },
                    runtimeDataIndex = new List<int>() { -1, -1 },
                    lastDataIndex = new List<int>() { -1, -1 }
                };
                return tooltip;
            }
        }

        /// <summary>
        /// 绑定提示框gameObject
        /// </summary>
        /// <param name="obj"></param>
        public void SetObj(GameObject obj)
        {
            m_GameObject = obj;
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// 绑定提示框的文本框gameObject
        /// </summary>
        /// <param name="content"></param>
        public void SetContentObj(GameObject content)
        {
            m_Content = content;
            m_ContentRect = m_Content.GetComponent<RectTransform>();
            m_ContentImage = m_Content.GetComponent<Image>();
            m_ContentText = m_Content.GetComponentInChildren<Text>();
            if (m_ContentText != null)
            {
                m_ContentTextRect = m_ContentText.gameObject.GetComponentInChildren<RectTransform>();
            }
            SetBackground(backgroundImage);
        }

        /// <summary>
        /// Keep Tooltiop displayed at the top. 
        /// 保持Tooltiop显示在最顶上
        /// </summary>
        public void UpdateToTop()
        {
            int count = m_GameObject.transform.parent.childCount;
            m_GameObject.GetComponent<RectTransform>().SetSiblingIndex(count - 1);
        }

        /// <summary>
        /// 设置提示框文本背景色
        /// </summary>
        /// <param name="color"></param>
        public void SetContentBackgroundColor(Color color)
        {
            if (m_ContentImage != null)
                m_ContentImage.color = color;
        }

        /// <summary>
        /// 设置提示框文本背景图片
        /// </summary>
        /// <param name="sprite"></param>
        public void SetBackground(Sprite sprite)
        {
            if (m_ContentImage != null)
            {
                m_ContentImage.type = Image.Type.Sliced;
                m_ContentImage.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置提示框文本字体颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetContentTextColor(Color color)
        {
            if (m_ContentText)
            {
                m_ContentText.color = color;
            }
        }

        /// <summary>
        /// 设置提示框文本内容
        /// </summary>
        /// <param name="txt"></param>
        public void UpdateContentText(string txt)
        {
            if (m_ContentText)
            {
                m_ContentText.text = txt;
                float wid, hig;
                if (m_FixedWidth > 0) wid = m_FixedWidth;
                else if (m_MinWidth > 0 && m_ContentText.preferredWidth < m_MinWidth) wid = m_MinWidth;
                else wid = m_ContentText.preferredWidth + m_PaddingLeftRight * 2;
                if (m_FixedHeight > 0) hig = m_FixedHeight;
                else if (m_MinHeight > 0 && m_ContentText.preferredHeight < m_MinHeight) hig = m_MinHeight;
                else hig = m_ContentText.preferredHeight + m_PaddingTopBottom * 2;
                if (m_ContentRect != null) m_ContentRect.sizeDelta = new Vector2(wid, hig);
                if (m_ContentTextRect != null)
                {
                    m_ContentTextRect.anchoredPosition = new Vector3(m_PaddingLeftRight, -m_PaddingTopBottom);
                }
            }
        }

        /// <summary>
        /// 清除提示框指示数据
        /// </summary>
        internal void ClearValue()
        {
            runtimeDataIndex[0] = runtimeDataIndex[1] = -1;
            runtimeXValues[0] = runtimeXValues[1] = -1;
            runtimeYValues[0] = runtimeYValues[1] = -1;
        }

        /// <summary>
        /// 提示框是否显示
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return m_GameObject != null && m_GameObject.activeInHierarchy;
        }

        /// <summary>
        /// 设置提示框是否显示
        /// </summary>
        /// <param name="flag"></param>
        public void SetActive(bool flag)
        {
            lastDataIndex[0] = lastDataIndex[1] = -1;
            if (m_GameObject && m_GameObject.activeInHierarchy != flag)
                m_GameObject.SetActive(flag);
        }

        /// <summary>
        /// 更新文本框位置
        /// </summary>
        /// <param name="pos"></param>
        public void UpdateContentPos(Vector2 pos)
        {
            if (m_Content)
                m_Content.transform.localPosition = pos;
        }

        /// <summary>
        /// 获得当前提示框的位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetContentPos()
        {
            if (m_Content)
                return m_Content.transform.localPosition;
            else
                return Vector3.zero;
        }

        /// <summary>
        /// Whether the data item indicated by tooltip has changed. 
        /// 提示框所指示的数据项是否发生变化。
        /// </summary>
        /// <returns></returns>
        public bool IsDataIndexChanged()
        {
            return runtimeDataIndex[0] != lastDataIndex[0] ||
                runtimeDataIndex[1] != lastDataIndex[1];
        }

        /// <summary>
        /// 当前索引缓存
        /// </summary>
        internal void UpdateLastDataIndex()
        {
            lastDataIndex[0] = runtimeDataIndex[0];
            lastDataIndex[1] = runtimeDataIndex[1];
        }

        /// <summary>
        /// 当前提示框是否选中数据项
        /// </summary>
        /// <returns></returns>
        public bool IsSelected()
        {
            return runtimeDataIndex[0] >= 0 || runtimeDataIndex[1] >= 0;
        }

        /// <summary>
        /// 指定索引的数据项是否被提示框选中
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsSelected(int index)
        {
            return runtimeDataIndex[0] == index || runtimeDataIndex[1] == index;
        }

        public bool IsNoFormatter()
        {
            return string.IsNullOrEmpty(m_Formatter) && string.IsNullOrEmpty(m_ItemFormatter);
        }

        internal string GetFormatterContent(int dataIndex, Series series, string category, ThemeInfo themeInfo = null, DataZoom dataZoom = null)
        {
            if (string.IsNullOrEmpty(m_Formatter))
            {
                if (string.IsNullOrEmpty(m_ItemFormatter)) return "";
                else
                {
                    var sb = ChartHelper.sb;
                    var title = m_TitleFormatter;
                    var formatTitle = !string.IsNullOrEmpty(title);
                    var needCategory = false;
                    var first = true;
                    sb.Length = 0;
                    for (int i = 0; i < series.Count; i++)
                    {
                        var serie = series.GetSerie(i);
                        var serieData = serie.GetSerieData(dataIndex, dataZoom);
                        var percent = serieData.GetData(1) / serie.yTotal * 100;
                        needCategory = needCategory || (serie.type == SerieType.Line || serie.type == SerieType.Bar);
                        if (serie.show)
                        {
                            string content = m_ItemFormatter;
                            content = content.Replace("{a}", serie.name);
                            content = content.Replace("{b}", needCategory ? category : serieData.name);
                            content = content.Replace("{c}", ChartCached.FloatToStr(serieData.GetData(1), 0, m_ForceENotation));
                            content = content.Replace("{d}", ChartCached.FloatToStr(percent, 1));
                            if (!first) sb.Append("\n");
                            sb.Append("<color=#").Append(themeInfo.GetColorStr(i)).Append(">● </color>");
                            sb.Append(content);
                            first = false;
                        }
                        if (formatTitle)
                        {
                            if (i == 0)
                            {
                                title = title.Replace("{a}", serie.name);
                                title = title.Replace("{b}", needCategory ? category : serieData.name);
                                title = title.Replace("{c}", ChartCached.FloatToStr(serieData.GetData(1), 0, m_ForceENotation));
                                title = title.Replace("{d}", ChartCached.FloatToStr(percent, 1));
                            }
                            title = title.Replace("{a" + i + "}", serie.name);
                            title = title.Replace("{b" + i + "}", needCategory ? category : serieData.name);
                            title = title.Replace("{c" + i + "}", ChartCached.FloatToStr(serieData.GetData(1), 0, m_ForceENotation));
                            title = title.Replace("{d" + i + "}", ChartCached.FloatToStr(percent, 1));
                        }
                    }
                    if (string.IsNullOrEmpty(title))
                    {
                        if (needCategory) return category + "\n" + sb.ToString();
                        else return sb.ToString();
                    }
                    else
                    {
                        title = title.Replace("\\n", "\n");
                        title = title.Replace("<br/>", "\n");
                        return title + "\n" + sb.ToString();
                    }
                }
            }
            else
            {
                string content = m_Formatter;
                for (int i = 0; i < series.Count; i++)
                {
                    var serie = series.GetSerie(i);
                    if (serie.show)
                    {
                        var needCategory = serie.type == SerieType.Line || serie.type == SerieType.Bar;
                        var serieData = serie.GetSerieData(dataIndex, dataZoom);
                        var percent = serieData.GetData(1) / serie.yTotal * 100;
                        if (i == 0)
                        {
                            content = content.Replace("{a}", serie.name);
                            content = content.Replace("{b}", needCategory ? category : serieData.name);
                            content = content.Replace("{c}", ChartCached.FloatToStr(serieData.GetData(1), 0, m_ForceENotation));
                            content = content.Replace("{d}", ChartCached.FloatToStr(percent, 1));
                        }
                        content = content.Replace("{a" + i + "}", serie.name);
                        content = content.Replace("{b" + i + "}", needCategory ? category : serieData.name);
                        content = content.Replace("{c" + i + "}", ChartCached.FloatToStr(serieData.GetData(1), 0, m_ForceENotation));
                        content = content.Replace("{d" + i + "}", ChartCached.FloatToStr(percent, 1));
                    }
                }
                content = content.Replace("\\n", "\n");
                content = content.Replace("<br/>", "\n");
                return content;
            }
        }

        internal string GetFormatterContent(string serieName, string dataName, float dataValue)
        {
            if (string.IsNullOrEmpty(m_Formatter))
                return ChartCached.FloatToStr(dataValue, 0, m_ForceENotation);
            else
            {
                var content = m_Formatter.Replace("{a}", serieName);
                content = content.Replace("{b}", dataName);
                content = content.Replace("{c}", ChartCached.FloatToStr(dataValue, 0, m_ForceENotation));
                content = content.Replace("\\n", "\n");
                content = content.Replace("<br/>", "\n");
                return content;
            }
        }

        internal Color GetLineColor(ThemeInfo theme)
        {
            if (lineStyle.color != Color.clear)
            {
                var color = lineStyle.color;
                color.a *= lineStyle.opacity;
                return color;
            }
            else
            {
                var color = (Color)theme.tooltipLineColor;
                color.a *= lineStyle.opacity;
                return color;
            }
        }
    }
}