using System.Collections.Generic;
using Builtin.Scripts.Game;
using HotAssets.Scripts.UI;
using UnityEngine;

namespace HotAssets.Scripts.Common
{
    public class Utils
    {
        public static void CopyToClipboard(string content)
        {
#if  MINIGAME_WECHAT
        var data = new WeChatWASM.SetClipboardDataOption();
        data.data = content;
        data.fail = result =>
        {
            Debug.LogError("copy failed : " + result.errMsg);
        };
        WeChatWASM.WX.SetClipboardData(data);
#elif MINIGAME_BYTEDANCE
        StarkSDKSpace.StarkSDK.API.GetStarkClipboard().SetClipboardData(content, (isSucc, errMsg) =>
        {
            if (isSucc)
            {
                Debug.Log("copy to clip board succ");
            }
            else
            {
                Debug.Log("copy to clip board error");
            }
        });
#else
            GUIUtility.systemCopyBuffer = content;
#endif
        }
        
        public static bool CircleHitRects(fix2 circlePivot, fix circleRadius, List<Rect> rects){
            if (rects.Count <= 0) return false;
            for (var i = 0; i < rects.Count; i++){
                if (Utils.CircleHitRect(circlePivot, circleRadius, rects[i]) == true){
                    return true;
                }
            }
            return false;
        }
        public static bool CircleHitRects(fix2 circlePivot, fix circleRadius, Rect[] rects){
            if (rects == null || rects.Length == 0) return false;
            for (var i = 0; i < rects.Length; i++){
                if (Utils.CircleHitRect(circlePivot, circleRadius, rects[i]) == true){
                    return true;
                }
            }
            return false;
        }

        public static bool CircleHitRect(fix2 circlePivot, fix circleRadius, Rect rect){
            int xp = circlePivot.x < rect.x ? 0 : (circlePivot.x > rect.x + rect.width ? 2 : 1);
            int yp = circlePivot.y < rect.y ? 0 : (circlePivot.y > rect.y + rect.height ? 2 : 1);
        
            if (yp == 1 && xp == 1) return true;  //在中间，则一定命中
        
            if (yp != 1 && xp == 1){
                fix halfRect = rect.height / 2;
                fix toHeart = fixMath.abs(circlePivot.y - (rect.y + halfRect));
                return (toHeart <= circleRadius + halfRect);
            }else
            if (yp == 1 && xp != 1){
                fix halfRect = rect.width / 2;
                fix toHeart = fixMath.abs(circlePivot.x - (rect.x + halfRect));
                return (toHeart <= circleRadius + halfRect);
            }else{
                return InRange(
                    circlePivot.x, circlePivot.y, 
                    yp == 0 ? rect.x : (rect.x + rect.width), 
                    xp == 0 ? rect.y : (rect.y + rect.height), 
                    circleRadius
                );
            }
        }
        
        /// <summary>
        /// 判断两点距离是否在范围内
        /// </summary>
        public static bool InRange(fix x1, fix y1, fix x2, fix y2, fix range){
            fix dx = x1 - x2;
            fix dy = y1 - y2;
            return dx * dx + dy * dy <= range * range;
        }
        
        
        /// <summary>
        /// 根据面得到一个八方向
        /// <param name="faceDegree">面向角度</param>
        /// </summary>
        public static cfg.Anim.Direction GetEightDirection(fix faceDegree)
        {
            if (faceDegree == 0)
            {
                return cfg.Anim.Direction.Right;
            }
            else if (faceDegree >= 15 && faceDegree < 60)
            {
                return cfg.Anim.Direction.RightTop;
            }
            else if (faceDegree >= 60 && faceDegree < 120)
            {
                return cfg.Anim.Direction.Top;
            }
            else if (faceDegree >= 120 && faceDegree < 165)
            {
                return cfg.Anim.Direction.LeftTop;
            }
            else if (faceDegree >= 165 && faceDegree < 195)
            {
                return cfg.Anim.Direction.Left;
            }
            else if (faceDegree >= 195 && faceDegree < 240)
            {
                return cfg.Anim.Direction.LeftBottom;
            }
            else if (faceDegree >= 240 && faceDegree < 300)
            {
                return cfg.Anim.Direction.Bottom;
            }
            else if (faceDegree >= 300 && faceDegree < 345)
            {
                return cfg.Anim.Direction.RightBottom;
            }
        
            return cfg.Anim.Direction.Null;
        }
        
        
        // 2D版本（XY平面）
        public static bool IsTargetInSectorArea2D(fix2 selfPos, fix facingAngle, fix2 enemyPos, 
            fix sectorRadius, fix sectorAngle)
        {
            // 计算相对位置向量
            fix2 directionToEnemy = enemyPos - selfPos;
        
            // 检查距离是否在半径范围内
            fix distance = directionToEnemy.length;
            if (distance > sectorRadius)
                return false;
    
            // 计算敌人相对于正X轴的角度（弧度）
            fix enemyAbsoluteAngle = fixMath.atan2(directionToEnemy.y, directionToEnemy.x);
    
            // 将facingAngle转换为弧度并计算相对角度
            fix enemyRelativeAngle = enemyAbsoluteAngle - facingAngle;
    
            // 规范化角度到 [-π, π] 范围
            enemyRelativeAngle = NormalizeAngle(enemyRelativeAngle);
    
            // 检查角度是否在扇形区域内（将sectorAngle转换为弧度）
            fix halfSectorAngle = fixMath.deg(sectorAngle / 2);
            return fixMath.abs(enemyRelativeAngle) <= halfSectorAngle;
        }
        
        
        // 规范化角度到 [-π, π] 范围
        private static fix NormalizeAngle(fix angle)
        {
            angle = fixMath.WrapAngle(angle);
            return angle;
        }
        
        /// <summary>
        /// 将战斗相机的世界坐标，转换到UI坐标上
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static Vector2 WorldToUIPoint(Vector3 worldPosition)
        {
            if (!AppEntry.MainCamera)
            {
                return Vector2.zero;
            }
        
            // 1. 将3D世界坐标转换为战斗相机的视口坐标
            Vector3 viewportPoint = AppEntry.MainCamera.WorldToViewportPoint(worldPosition);
        
            // 3. 将视口坐标转换为UI相机的屏幕坐标
            Vector2 screenPoint = new Vector2(
                viewportPoint.x * Screen.width,
                viewportPoint.y * Screen.height
            );
        
            // 4. 将屏幕坐标转换为UI Canvas的本地坐标
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                AppEntry.UIRoot,
                screenPoint,
                AppEntry.UICamera,
                out localPoint
            );
            return localPoint;
        }

    }
}
