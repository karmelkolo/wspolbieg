//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            lock (_lock)
            {
                _balls.Clear();
            }

            layerBellow.Start(numberOfBalls, (startingPosition, dataBall) =>
            {
                lock (_lock)
                {
                    if (!_balls.Contains(dataBall))
                    {
                        _balls.Add(dataBall);
                        dataBall.NewPositionNotification += (sender, pos) => CheckCollision(sender, pos);
                    }
                }
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(dataBall));
            });
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private readonly object _lock = new object();
        private readonly List<Data.IBall> _balls = new();
        private readonly double borderWidth = 390;
        private readonly double borderHeight = 410;

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;

        private void CheckCollision(object? sender, Data.IVector newPosition)
        {
            if (sender is not Data.IBall ball1)
            {
                return;
            }
            CheckBorder(ball1);

            lock (_lock)
            {
                foreach (var ball2 in _balls)
                {
                    if (ball1 == ball2)
                    {
                        continue;
                    }
                    CheckBalls(ball1, ball2);
                }
            }
        }

        private void CheckBorder(Data.IBall ball)
        {
            var position = ball.Position;
            var velocity = ball.Velocity;

            Data.Vector nextPosition = new(position.x + velocity.x, position.y + velocity.y);

            if (nextPosition.x <= 0 || nextPosition.x >= borderWidth - ball.Diameter)
            {
                ball.Velocity = new Data.Vector(-velocity.x, velocity.y);
                double clampedX = Math.Clamp(nextPosition.x, 0, borderWidth - ball.Diameter);
                ball.Position = new Data.Vector(clampedX, nextPosition.y);
            }

            if (nextPosition.y <= 0 || nextPosition.y >= borderHeight - ball.Diameter)
            {
                ball.Velocity = new Data.Vector(velocity.x, -velocity.y);
                double clampedY = Math.Clamp(nextPosition.y, 0, borderHeight - ball.Diameter);
                ball.Position = new Data.Vector(nextPosition.x, clampedY);
            }
        }

        private void CheckBalls(Data.IBall b1, Data.IBall b2)
        {
            object firstLock = b1.GetHashCode() < b2.GetHashCode() ? b1 : b2;
            object secondLock = b1.GetHashCode() < b2.GetHashCode() ? b2 : b1;

            lock (firstLock)
            {
                lock (secondLock)
                {
                    var p1 = b1.Position;
                    var p2 = b2.Position;

                    double center1X = p1.x + b1.Diameter / 2;
                    double center1Y = p1.y + b1.Diameter / 2;
                    double center2X = p2.x + b2.Diameter / 2;
                    double center2Y = p2.y + b2.Diameter / 2;

                    double dx = center1X - center2X;
                    double dy = center1Y - center2Y;
                    double distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < 1.0) return;
                    double minDistance = (b1.Diameter / 2) + (b2.Diameter / 2);

                    if (distanceSquared < minDistance * minDistance)
                    {
                        var v1 = b1.Velocity;
                        var v2 = b2.Velocity;

                        double relativeVelocityX = v1.x - v2.x;
                        double relativeVelocityY = v1.y - v2.y;

                        if ((relativeVelocityX * dx + relativeVelocityY * dy) > 0) return;

                        double m1 = 10;
                        double m2 = 10;
                        double commonPart = 2 * (relativeVelocityX * dx + relativeVelocityY * dy) / ((m1 + m2) * distanceSquared);

                        double v1x = v1.x - commonPart * m2 * dx;
                        double v1y = v1.y - commonPart * m2 * dy;
                        double v2x = v2.x + commonPart * m1 * dx;
                        double v2y = v2.y + commonPart * m1 * dy;

                        b1.Velocity = new Data.Vector(v1x, v1y);
                        b2.Velocity = new Data.Vector(v2x, v2y);
                    }
                }
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}