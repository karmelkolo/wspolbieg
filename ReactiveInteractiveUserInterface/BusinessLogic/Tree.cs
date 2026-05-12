using System.Numerics;
using DataIBall = TP.ConcurrentProgramming.Data.IBall;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal readonly struct BoundingBox
    {
        public double X { get;  }
        public double Y { get; }
        public double HalfWidth { get; }
        public double HalfHeight { get; }

        public BoundingBox(double x, double y, double halfWidth, double halfHeight)
        {
            X = x;
            Y = y;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }

        public bool Contains(Data.IVector position)
        {
            return (position.x >= X - HalfWidth &&
                    position.x <= X + HalfWidth &&
                    position.y >= Y - HalfHeight &&
                    position.y <= Y + HalfHeight);
        }

        public bool Intersects(BoundingBox box)
        {
            return !(box.X - box.HalfWidth > X + HalfWidth ||
                     box.X + box.HalfWidth < X - HalfWidth ||
                     box.Y - box.HalfHeight > Y + HalfHeight ||
                     box.Y + box.HalfHeight < Y - HalfHeight);
        }
    }

    internal class Tree
    {
        private readonly int Capacity;
        private readonly int MaxDepth;
        private readonly int CurrentDepth;
        private readonly BoundingBox Boundary;
        private readonly List<DataIBall> BallList;
        private bool Divided;

        private Tree TopLeft;
        private Tree TopRight;
        private Tree BottomLeft;
        private Tree BottomRight;

        public Tree(BoundingBox boundary, int capacity, int currentDepth = 0, int maxDepth = 5)
        {
            Boundary = boundary;
            Capacity = capacity;
            CurrentDepth = currentDepth;
            MaxDepth = maxDepth;
            BallList = new List<DataIBall>();
            Divided = false;
        }

        public bool Insert(DataIBall ball)
        {
            if (!Boundary.Contains(ball.Position))
            {
                return false;
            }

            if ((BallList.Count < Capacity || CurrentDepth >= MaxDepth) && !Divided)
            {
                BallList.Add(ball);
                return true;
            }

            if (!Divided)
            {
                Divide();
            }

            if (TopLeft.Insert(ball)) return true;
            if (TopRight.Insert(ball)) return true;
            if (BottomLeft.Insert(ball)) return true;
            if (BottomRight.Insert(ball)) return true;

            return false;
        }

        private void Divide()
        {
            double x = Boundary.X;
            double y = Boundary.Y;
            double halfWidth = Boundary.HalfWidth / 2;
            double halfHeight = Boundary.HalfHeight / 2;
            int nextDepth = CurrentDepth + 1;

            TopLeft = new Tree(new BoundingBox(x - halfWidth, y - halfHeight, halfWidth, halfHeight), Capacity, nextDepth, MaxDepth);
            TopRight = new Tree(new BoundingBox(x + halfWidth, y - halfHeight, halfWidth, halfHeight), Capacity, nextDepth, MaxDepth);
            BottomLeft = new Tree(new BoundingBox(x - halfWidth, y + halfHeight, halfWidth, halfHeight), Capacity, nextDepth, MaxDepth);
            BottomRight = new Tree(new BoundingBox(x + halfWidth, y + halfHeight, halfWidth, halfHeight), Capacity, nextDepth, MaxDepth);

            Divided = true;

            foreach (var ball in BallList)
            {
                if (TopLeft.Insert(ball)) continue;
                if (TopRight.Insert(ball)) continue;
                if (BottomLeft.Insert(ball)) continue;
                if (BottomRight.Insert(ball)) continue;
            }

            BallList.Clear();
        }

        public void Search(BoundingBox range, List<DataIBall> BallsFound)
        {
            if (!Boundary.Intersects(range))
            {
                return;
            }

            if (!Divided)
            {
                foreach (var ball in BallList)
                {
                    if (range.Contains(ball.Position)) {
                        BallsFound.Add(ball);
                    }
                }
                return;
            }

            TopLeft.Search(range, BallsFound);
            TopRight.Search(range, BallsFound);
            BottomLeft.Search(range, BallsFound);
            BottomRight.Search(range, BallsFound);
        } 
    }
}
