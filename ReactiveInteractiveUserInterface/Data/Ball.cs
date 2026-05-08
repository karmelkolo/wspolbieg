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

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, int borderWidth, int borderHeight)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            BorderWidth = borderWidth;
            BorderHeight = borderHeight;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        public IVector Position { get; private set; }

        #endregion IBall

        #region private

        private int BorderWidth;
        private int BorderHeight;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move()
        {
            Vector nextPosition = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            if (nextPosition.x <= 0 || nextPosition.x >= BorderWidth - 20)
            {
                Velocity = new Vector(-Velocity.x, Velocity.y);
                double clampedX = Math.Clamp(nextPosition.x, 0, BorderWidth - 20);
                nextPosition = new(clampedX, nextPosition.y);
            }

            if (nextPosition.y <= 0 || nextPosition.y >= BorderHeight - 20)
            {
                Velocity = new Vector(Velocity.x, -Velocity.y);
                double clampedY = Math.Clamp(nextPosition.y, 0, BorderHeight - 20);
                nextPosition = new(nextPosition.x, clampedY);
            }

            Position = nextPosition;
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}

