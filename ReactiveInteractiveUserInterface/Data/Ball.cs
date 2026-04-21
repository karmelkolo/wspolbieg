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

        internal Ball(Vector initialPosition, Vector initialVelocity, double borderWidth, double borderHeight)
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

        #endregion IBall

        #region private

        private Vector Position;
        private double BorderWidth;
        private double BorderHeight;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move(Vector delta)
        {
            Vector NewPosition = new Vector(Position.x + delta.x, Position.y + delta.y);
            if (NewPosition.x > 0 && NewPosition.y > 0 && NewPosition.y < BorderWidth-30 && NewPosition.x < BorderHeight-30)
            {
                Position = NewPosition;
                RaiseNewPositionChangeNotification();
            }
        }

        #endregion private
    }
}

