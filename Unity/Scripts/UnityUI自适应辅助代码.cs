using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustUIManualHeight : MonoBehaviour {
    public UISprite background;
    public UIRoot uiRoot;
    public float uiWidth;
    public float uiHeight;

	void Start () {
        float screenAspectRatio = (float)Screen.width / Screen.height;
        Debug.Log("screenAspectRatio = " + screenAspectRatio + 
            ", width = " + Screen.width + ", height = " + Screen.height);

        float uiAspectRatio = uiWidth / uiHeight;
        Debug.Log("uiAspectRatio = " + uiAspectRatio + ", " +
            "width = " + uiWidth + ", height = " + uiHeight);

        float backgroundAspectRatio = (float)background.width / background.height;
        Debug.Log("backgroundAspectRatio = " + backgroundAspectRatio +
            ", width = " + background.width + ", height = " + background.height);

        // 屏幕宽高比小于 ui 的宽高比, 以原参数缩放 ui 在水平方向上将超出屏幕范围.
        // 以屏幕和 ui 宽高比的比例关系调整 UIRoot.manualHeight 的值,使 ui 可以
        // 在水平方向上完全显示,但在垂直方向上可能与 ui 原始设计存在一定的位置偏差.
        // 这个时候背景在垂直方向上会留下空白,以屏幕和 ui 宽高比的比例关系调整背景的
        // 大小,以在垂直方向上充满屏幕,水平方向上超出屏幕部分将不能显示.
        if (screenAspectRatio < uiAspectRatio) {
            float times = uiAspectRatio / screenAspectRatio;

            // 调整 ui 缩放参考高度,以在水平方向上完整显示 ui .
            uiRoot.manualHeight = (int)(uiRoot.manualHeight * times);
            Debug.Log("times = " + times + ", manualHeight = " + uiRoot.manualHeight);

            // 等比例缩放背景图片.
            background.height = (int)(times * background.height);
            background.width = (int)(times * background.width);
            Debug.Log("times = " + times + ", background.height = " + background.height +
                ", background.width = " + background.width);
        
        }
        // 屏幕宽高比大于等于 ui 宽高比,屏幕可以完全显示 ui.

        // 屏幕宽高比大于背景宽高比,在水平方向上拉伸背景图片,以铺满屏幕.
        if ( screenAspectRatio > backgroundAspectRatio) {
            float times = screenAspectRatio / backgroundAspectRatio;
            background.width = (int)(background.width * times);
            Debug.Log("times = " + times + ", background.height = " + background.height +
                ", background.width = " + background.width);
        }

        /* 屏幕背景如果以最大屏幕宽高比设计,则始终可以等比例缩放,不会产生变形,但背景在
         * 宽高比小于背景宽高比的屏幕上显示时,在水平方向上两边会有一定的部分不能显示,
         * 在拉伸变形和部分不能显示方面需要根据需求进行权衡.
         * 
         * 以高度为参数进行缩放,可以确保 ui 在水平方向上始终与原设计保持一致,但在垂直方向上
         * 由于屏幕宽高比的不同可能造成偏差,但一般屏幕的宽都大于高(手机以横屏方式计算),所以
         * 这样造成的偏差是较小的,所以建议以高度为参考进行缩放,在设计 ui 时应注意到这一点,
         * 确保不会对 ui 造成不好的影响.
         * 
         * 屏幕宽高比大于 ui 宽高比时,屏幕左右两边会出现多余空间,在布局时应注意到这一点,
         * 合理利用.
         * 
         * 屏幕, ui, 背景宽高比之间的比例关系,既是他们宽度之间的比例关系,因为UIRoot以
         * UIRoot.manualHeight 和 Screen.height 为根据对所有 ui 进行了缩放,而 ui,
         * 背景的高度相同.
         */
    }
}
