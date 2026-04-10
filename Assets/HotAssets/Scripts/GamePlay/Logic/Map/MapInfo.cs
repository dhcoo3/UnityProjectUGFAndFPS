using System.Collections.Generic;
using AAAGame.ScriptsHotfix.GamePlay.Logic.Map;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using Unity.Mathematics;
using UnityEngine;

namespace HotAssets.Scripts.GamePlay.Logic.Map
{
    public class MapInfo
    {
        ///<summary>
        ///地图的单元格信息，对应角色坐标为[x,y]
        ///</summary>
        public GridInfo[,] grid;

        ///<summary>
        ///每个单元格的宽度和高度
        ///单位：米
        ///</summary>
        public fix2 gridSize{get;}

        ///<summary>
        ///获得地图的边界，单位：米
        ///</summary>
        public Rect border{get;}


        public MapInfo(GridInfo[,] map, fix2 gridSize){
            this.grid = map;
            this.gridSize = new fix2(
                fixMath.max(0.1f, gridSize.x),    //最小0.1米
                fixMath.max(0.1f, gridSize.y)
            );
            this.border = new Rect(
                -gridSize.x / 2.00f,
                -gridSize.y / 2.00f,
                gridSize.x * MapWidth(),
                gridSize.y * MapHeight()
            );
        }

        ///<summary>
        ///地图的宽度
        ///<return>单位：单元格</return>
        ///</summary>
        public int MapWidth(){
            return grid.GetLength(0);
        }

        ///<summary>
        ///地图的高度
        ///<return>单位：单元格</return>
        ///</summary>
        public int MapHeight(){
            return grid.GetLength(1);
        }

        ///<summary>
        ///获得某个东西坐落在的位置信息
        ///<param name="pos">要检查的点，单位：米</param>
        ///<return>返回这个点的单元格信息</return>
        ///</summary>
        public GridInfo GetGridInPosition(fix3 pos){
            int2 gPos = GetGridPosByMeter(pos.x, pos.y);
            if (gPos.x < 0 || gPos.x >= MapWidth() || gPos.y < 0 || gPos.y >= MapHeight()) 
                return GridInfo.VoidGrid;
            return grid[gPos.x, gPos.y];
        }

        ///<summary>
        ///从地图上x坐标的米，获得单元格坐标x
        ///<param name="x">角色坐标x，单位：米</param>
        ///<param name="y">角色坐标y，单位：米</param>
        ///<return>单元格坐标，单位：单元格</return>
        ///</summary>
        public int2 GetGridPosByMeter(fix x, fix y){
            return new int2(
                //fixMath.floorToInt((x + gridSize.x / 2.00f) / gridSize.x),
                //fixMath.floorToInt((z + gridSize.y / 2.00f) / gridSize.y)
                fixMath.roundToInt(x / gridSize.x),
                fixMath.roundToInt(y / gridSize.y)
            );
        }

        ///<summary>
        ///判断某种移动模式下，某个单元格是否可过
        ///<param name="gridX">单元格坐标x</param>
        ///<param name="gridY">单元格坐标y</param>
        ///<param name="moveType">移动方式</param>
        ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
        ///<return>是否可过</return>
        ///</summary>
        public bool CanGridPasses(int gridX, int gridY, cfg.Game.MoveType moveType, bool ignoreBorder){
            if (gridX < 0 || gridX >= MapWidth() || gridY < 0 || gridY >= MapHeight()) return ignoreBorder;
            switch (moveType){
                case cfg.Game.MoveType.Ground: return grid[gridX, gridY].groundCanPass;
                case cfg.Game.MoveType.Fly: return grid[gridX, gridY].flyCanPass;
            }
            return false;
        }

        ///<summary>
        ///判断一个单位是否可以站在某个位置上
        ///<param name="pos">单位的位置，单位：米</param>
        ///<param name="radius">单位的半径，单位：米</param>
        ///<param name="moveType">单位移动模式</param>
        ///<return>是否可以站在这里，true代表可以</return>
        ///</summary>
        public bool CanUnitPlacedHere(fix3 pos, fix radius, cfg.Game.MoveType moveType){
            int2 lt = GetGridPosByMeter(pos.x - radius, pos.y - radius);
            int2 rb = GetGridPosByMeter(pos.x + radius, pos.y + radius);
            int aw = rb.x - lt.x + 1;
            int ah = rb.y - lt.y + 1;
            List<Rect> collisionRects = new List<Rect>();
            for (int i = lt.x; i <= rb.x; i++){
                for (int j = lt.y; j <= rb.y; j++){
                    if (CanGridPasses(i, j, moveType, false) == false){
                        collisionRects.Add(new Rect(
                            (i - 0.5f) * gridSize.x,
                            (j - 0.5f) * gridSize.y,
                            gridSize.x,
                            gridSize.y
                        ));
                    }
                    
                }
            }
            return !Utils.CircleHitRects(new fix2(pos.x, pos.y), radius, collisionRects);
            // Vector2Int gPos = GetGridPosByMeter(pos.x, pos.z);
            // if (gPos.x < 0 || gPos.y < 0 || gPos.x >= MapWidth() || gPos.y >= MapHeight()) return false;
            // return grid[gPos.x, gPos.y].groundCanPass;
        }

            
        ///<summary>
        ///随机获得一个坐标，可以让角色站立在那里
        ///获得的坐标单位是米的，而非单元格，其坐标z始终为0
        ///如果实在没有这个位置，就会返回vector3.zero
        ///<param name="range">一个随机的范围，单位：单元格</param>
        ///<param name="chaRadius">角色半径，单位：米</param>
        ///<param name="moveType">单位移动模式</param>
        ///<return>可以落脚的坐标点</return>
        public fix3 GetRandomPosForCharacter(RectInt range, fix chaRadius, cfg.Game.MoveType moveType = cfg.Game.MoveType.Ground){
            List<fix3> mayRes = new List<fix3>();
            for (var i = range.x; i < range.x + range.width; i ++){
                for (var j = range.y; j < range.y + range.height; j++){
                    //if (i >= 0 && i < MapWidth() && j >= 0 && j < MapHeight() && gridInfo[i, j].characterCanPass == true){
                    fix3 ranPos = new fix3(
                        i * gridSize.x, 
                        j * gridSize.y,
                        0
                    );
                    if (CanUnitPlacedHere(ranPos, chaRadius, moveType) == true) {  
                        mayRes.Add(ranPos);
                    }
                }
            }
            return mayRes[fixMath.floorToInt(GamePlayFacade.Instance.Random.NextInt(0, mayRes.Count))];
        }
        
        ///<summary>
        ///从一个点（单位米）出发获得方向上的第一个竖着的阻挡
        ///<param name="pivot">出发的点，单位：米</param>
        ///<param name="dir">查询方向以及长度，单位：米</param>
        ///<param name="radius">假设有一个半径（当做点是圆形中心），也就是额外追加一个距离，单位：米</param>
        ///<param name="moveType">移动方式</param>
        ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
        ///<return>最合适的x坐标</return>
        ///</summary>
        public fix GetNearestVerticalBlock(fix3 pivot, fix dir, fix radius, cfg.Game.MoveType moveType, bool ignoreBorder){
            if (dir == 0) return pivot.x;
            int dv = dir > 0 ? 1 : -1;
            fix bestX = pivot.x + dir;
            int seekWidth = fixMath.ceilToInt((fixMath.abs(dir) + radius) / gridSize.x + 2);
            int2 gPos = GetGridPosByMeter(pivot.x, pivot.y);
            for (var i = 0; i < seekWidth; i++){
                int cgX = gPos.x + dv * i;
                if (this.CanGridPasses(cgX, gPos.y, moveType, ignoreBorder) == false){
                    fix wallX = (cgX - dv * 0.5f) * gridSize.x - dv * radius;
                    if (dv > 0){
                        return fixMath.min(wallX, bestX);
                    }else{
                        return fixMath.max(wallX, bestX);
                    }
                }
            }
            return bestX;
        }

        ///<summary>
        ///从一个点（单位米）出发获得方向上的第一个横着的阻挡
        ///<param name="pivot">出发的点，单位：米</param>
        ///<param name="dir">查询方向以及高度，单位：米</param>
        ///<param name="radius">假设有一个半径（当做点是圆形中心），也就是额外追加一个距离，单位：米</param>
        ///<param name="moveType">移动方式</param>
        ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
        ///<return>最合适的y坐标</return>
        ///</summary>
        public fix GetNearestHorizontalBlock(fix3 pivot, fix dir, fix radius, cfg.Game.MoveType moveType, bool ignoreBorder){
            if (dir == 0) return pivot.y;
            int dv = dir > 0 ? 1 : -1;
            fix bestY = pivot.y + dir;
            int seekHeight = fixMath.ceilToInt((fixMath.abs(dir) + radius) / gridSize.y + 2);
            int2 gPos = GetGridPosByMeter(pivot.x, pivot.y);
            for (var i = 0; i < seekHeight; i++){
                int cgY = gPos.y + dv * i;
                if (this.CanGridPasses(gPos.x, cgY, moveType, ignoreBorder) == false){
                    fix wallZ = (cgY - dv * 0.5f) * gridSize.y - dv * radius;
                    if (dv > 0){
                        return fixMath.min(wallZ, bestY);
                    }else{
                        return fixMath.max(wallZ, bestY);
                    }
                }
            }
            return bestY;
        }

        ///<summary>
        ///根据一个圆（中心点和半径），获得期望移动到某个坐标的最理想的点
        ///TODO 只有移动速度不那么快的时候才在大多数时候工作正常……
        ///<param name="pivot">这个圆形的中心点坐标</param>
        ///<param name="radius">这个圆形的半径</param>
        ///<param name="targetPos">这个圆形期望移动到的坐标</param>
        ///<param name="moveType">圆形的移动方式</param>
        ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
        ///<return>应该移动到的坐标</return>
        ///</summary>
        public MapTargetPosInfo FixTargetPosition(fix3 pivot, fix radius, fix3 targetPos, cfg.Game.MoveType moveType, bool ignoreBorder){
            fix xDir = targetPos.x - pivot.x;
            fix yDir = targetPos.y - pivot.y;
            fix bestX = GetNearestVerticalBlock(pivot, xDir, radius, moveType, ignoreBorder);
            fix bestY = GetNearestHorizontalBlock(pivot, yDir, radius, moveType, ignoreBorder);

            bool obstacled =  (
                fixMath.roundToInt(bestX * 1000) != fixMath.roundToInt(targetPos.x * 1000) ||
                fixMath.roundToInt(bestY * 1000) != fixMath.roundToInt(targetPos.y * 1000)
            );
            return new MapTargetPosInfo(obstacled, new fix3(bestX, bestY, targetPos.z));
        }
    }
}