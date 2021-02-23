using System;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// Grid Layout コンポーネントを使わず自前でグリッド配置を行う際の座標計算
    /// </summary>
    public class GridLayoutSupport
    {
        public Vector2 cellSize { get; private set; }
        public Vector2 spacing  { get; private set; }
        public Vector4 padding  { get; private set; }  // top, right, bottom, left
        public float areaWidth  { get; private set; }
        public int numColumn    { get; private set; }

        bool _isValid;

        //----------------------------------------------------------------------
        // Init
        //----------------------------------------------------------------------

        public GridLayoutSupport(
            Vector2 cellSize, float areaWidth,
            Vector2? _spacing = null, Vector4? _padding = null
        )
        {
            this.cellSize  = cellSize;
            this.areaWidth = areaWidth;
            this.spacing   = _spacing ?? Vector2.zero;
            this.padding   = _padding ?? Vector4.zero;

            ValidateParams();
            CalcNumColumn();
        }

        void ValidateParams()
        {
            _isValid = false;

            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                Debug.LogError($"Invalid cell size : {cellSize}");
                return;
            }
            if (cellSize.x - spacing.x <= 0f)
            {
                Debug.LogError($"Invalid horizontal spacing : {spacing}");
                return;
            }

            _isValid = true;
        }

        void CalcNumColumn()
        {
            if (!_isValid) { return; }

            numColumn = 0;
            float accumWidth = cellSize.x + padding.y + padding.w;
            do
            {
                ++numColumn;
                accumWidth += spacing.x + cellSize.x;
            }
            while(accumWidth < areaWidth);
        }

        //----------------------------------------------------------------------
        // public
        //----------------------------------------------------------------------

        public float CalcHeight(int numCell)
        {
            if (!_isValid) { return 0f; }

            int numRow = (numCell <= 0) ? 0 : ((numCell - 1) / numColumn) + 1;
            int numSpacing = Math.Max(0, numRow - 1);
            float height = (cellSize.y * numRow) + (spacing.y * numSpacing);
            return padding.x + height + padding.z;
        }

        /// <summary>
        /// 左上を (0, 0) とし、下方向に y 軸がマイナスとなる座標系でセルの位置を返す
        /// </summary>
        public Vector2 CalcCellPos(int cellIndex, Vector2? _anchor = null)
        {
            if (!_isValid) { return Vector2.zero; }

            Vector2 anchor = _anchor ?? new Vector2(0.5f, 0.5f);
            anchor = new Vector2(anchor.x, 1 - anchor.y);

            int col = cellIndex % numColumn;
            int row = cellIndex / numColumn;
            float x =  padding.w + (cellSize.x + spacing.x) * col + (cellSize.x * anchor.x);
            float y = -padding.x - (cellSize.y + spacing.y) * row - (cellSize.y * anchor.y);
            return new Vector2(x, y);
        }
    }
}
