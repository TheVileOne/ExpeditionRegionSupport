using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = UnityEngine.Vector2;

namespace ExpeditionRegionSupport.Interface
{
    public class PositionSpacer
    {
        /// <summary>
        /// The position to start spacing calculations
        /// </summary>
        private Vector2 basePosition;

        /// <summary>
        /// The distance between one position, and an adjacent position factoring in a common object dimension (width/height), and the spacing desired
        /// </summary>
        private float adjustmentDistance;

        /// <summary>
        /// Should position calculation apply to the x-coordinate (horizontal spacing), or y-coordinate (vertical spacing)
        /// </summary>
        private bool adjustSpacingHorizontally;

        /// <summary>
        /// The number of positions advanced from the base position
        /// </summary>
        public int PositionCount = 0;

        public Vector2 CurrentPosition => PositionAt(PositionCount);

        /// <summary>
        /// Advances and returns the next current position
        ///  Note: If you want to see the next position without advancing the check index, use Peek() instead.
        /// </summary>
        public Vector2 NextPosition => AdvancePosition();

        /// <summary>
        /// Constructs a PositionSpacer object
        /// </summary>
        /// <param name="firstPosition">The position to start spacing calculations at</param>
        /// <param name="objectDimension">A common height (vertical spacing) value or width (horizontal spacing) value</param>
        /// <param name="spacingWanted">The distance between two adjacent objects</param>
        /// <param name="spaceHorizontally">The distance will be applies to the x-value when true. False by default</param>
        public PositionSpacer(Vector2 firstPosition, float objectDimension, float spacingWanted, bool spaceHorizontally = false)
        {
            basePosition = firstPosition;
            adjustmentDistance = objectDimension + spacingWanted;
            adjustSpacingHorizontally = spaceHorizontally;
        }

        /// <summary>
        /// Advances current position to the next adjusted position
        /// </summary>
        /// <returns>The next current position</returns>
        public Vector2 AdvancePosition()
        {
            PositionCount++; //Advance to next adjusted position
            return CurrentPosition;
        }

        /// <summary>
        /// Gets the next current position without affecting the index
        /// </summary>
        /// <returns></returns>
        public Vector2 Peek()
        {
            return PositionAt(PositionCount + 1);
        }

        /// <summary>
        /// Gets the adjusted position at a given index 
        /// </summary>
        public Vector2 PositionAt(int index)
        {
            index = Math.Max(0, index);

            //Horizontal Spacing: x-value increases within screen from left to right: add adjustment to base x
            //Vertical Spacing: y-value increases within screen from bottom to top: subtract adjustment from base y
            if (adjustSpacingHorizontally)
                return new Vector2(basePosition.x + (adjustmentDistance * index), basePosition.y);
            else
                return new Vector2(basePosition.x, basePosition.y - (adjustmentDistance * index));
        }
    }
}
